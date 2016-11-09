using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DevExpress.Xpo;
using Gurock.SmartInspect;
using Gurock.SmartInspect.SDK;

namespace StatisticApp
{
    internal class Statistic
    {
        private const string haspid = "HaspID";

        private readonly Dictionary<string, ClientXPO> clientsDics;


        private readonly string dirScreenshots;
        private readonly string dirTemp;
        private readonly UnitOfWork uow;

        public Statistic()
        {
            dirTemp = @"d:\Temp\Extracted1\";
            uow = new UnitOfWork();
            // ReSharper disable once CollectionNeverUpdated.Local
            var clients = new XPCollection<ClientXPO>(uow);
            clients.ForEach(item => item.AddFields());
            uow.CommitChanges();

            clientsDics = new XPCollection<ClientXPO>(uow).ToDictionary(GetClientID,
                item => item);

            dirScreenshots = @"c:\Temp\Screeens";

            if (!Directory.Exists(dirScreenshots))
                Directory.CreateDirectory(dirScreenshots);
            GetStatisticData();
        }

        private string GetClientID(ClientXPO client) => client.GetField("HaspID").Description;


        private IEnumerable<StatisticData> GetEntrySDs(LogEntry entry)
        {
            var entryValue = new Lazy<string>(() =>
            {
                using (var sr = new StreamReader(entry.Data))
                    return sr.ReadToEnd();
            });

            Func<string, List<Tuple<string, string>>> getValuesByRegex = regex =>
            {
                var matchResult = new Regex(regex).Match(entryValue.Value);
                var ret = new List<Tuple<string, string>>();
                while (matchResult.Success)
                {
                    ret.Add(new Tuple<string, string>(matchResult.Groups[1].Value, matchResult.Groups[2].Value));
                    matchResult = matchResult.NextMatch();
                }

                return ret;
            };

            const string regexBeforeKateLogDic38 = "(.*): ([^\r]*)";
            const string regexAfterKateLogDic38 = "(.*)=(.*)\r\n";

            var infos =
                new Lazy<List<Tuple<string, string>>>(
                    () =>
                        getValuesByRegex(entry.ViewerId == ViewerId.ValueList
                            ? regexAfterKateLogDic38
                            : regexBeforeKateLogDic38));

            Func<bool, Func<Tuple<string, string>, StatisticData>, IEnumerable<StatisticData>> infoFromDic =
                (predic, func) =>
                    predic && entry.Data != null ? infos.Value.Select(func) : Enumerable.Empty<StatisticData>();

            var dbInfo =
                Fields.DBInfoFields.Where(item => entry.Title == item)
                    .Select(
                        item =>
                            new StatisticData
                            {
                                Name = item,
                                LastTime = entry.Timestamp,
                                Count = Convert.ToInt32(infos.Value.First(item1 => item1.Item1 == "Count").Item2),
                                Description =
                                    string.Join(Environment.NewLine, infos.Value.Select(item1 => item1.Item2).Skip(1))
                            });

            var haspInfo = infoFromDic(entry.Title == "Инфа о HASP",
                tp => new StatisticData {Name = tp.Item1, LastTime = entry.Timestamp, Description = tp.Item2});

            var osInfo =
                infoFromDic(entry.Title == "OSInfo" && !entry.Title.ToLower().Contains("дамп реестра с эмуляторами"),
                    tp => new StatisticData {Name = tp.Item1, LastTime = entry.Timestamp, Description = tp.Item2});

            Func<IEnumerable<StatisticData>> oldVer = () =>
            {
                if (string.IsNullOrEmpty(entry.Title) || entry.Title[0] != '3')
                    return Enumerable.Empty<StatisticData>();

                var cifry = entry.Title.Split('.');
                return entry.LogEntryType == LogEntryType.Message && cifry.Count() == 4 && cifry[0] == "3"
                    ? EnumerableEx.Return(new StatisticData
                    {
                        Name = "AppVersion",
                        LastTime = entry.Timestamp,
                        Description = cifry[0] + "." + cifry[1]
                    })
                    : Enumerable.Empty<StatisticData>();
            };

            var verInfo = entry.Title == "AppVersion"
                ? infoFromDic(true,
                    tp => new StatisticData {Name = "AppVersion", LastTime = entry.Timestamp, Description = tp.Item2})
                : oldVer();
            /*
            if (entry.Title == "Скриншот")
            {
                var fileName = Path.Combine(dirScreenshots,
                    entry.Timestamp.ToString("yyyy_MM_dd_hh_mm_ss_FFFFFFF") + ".jpeg");
                var fs = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite);
                var memoryStream = new MemoryStream();
                entry.Data.CopyTo(memoryStream);

                var bw = new BinaryWriter(fs);
                bw.Write(memoryStream.ToArray());
                bw.Close();
            }*/

            return dbInfo.Concat(haspInfo).Concat(osInfo).Concat(verInfo);
        }

        private void GetStatisticData()
        {
            Func<string, Dictionary<string, StatisticData>> getSdFromDir = file =>
            {
                try
                {
                    var ses =
                        new LogFile(file).OfType<LogEntry>()
                            .AsEnumerable()
                            .AsParallel()
                            .SelectMany(GetEntrySDs)
                            .ToList();

                    return
                        ses.GroupBy(sd => sd.Name)
                            .Select(gr => gr.MaxBy(item => item.LastTime).First())
                            .ToArray()
                            .ToDictionary(sd => sd.Name, sd => sd);
                }
                catch (Exception)
                {
                    return new Dictionary<string, StatisticData>();
                }
            };

            foreach (var log in Directory.EnumerateFiles(dirTemp, "*.*", SearchOption.AllDirectories))
            {
                Console.WriteLine(log);
                var s = getSdFromDir(log);
                if (!s.ContainsKey(haspid))
                    continue;

                var hasp = s[haspid];

                var client = clientsDics.ContainsKey(hasp.Description)
                    ? clientsDics[hasp.Description]
                    : clientsDics[hasp.Description] = new ClientXPO(uow);

                foreach (var statisticData in s)
                {
                    var statisticDataXpo = client.GetField(statisticData.Key);
                    if (statisticDataXpo == null)
                    {
                        Console.WriteLine(statisticData.Key);
                        continue;
                    }

                    if (statisticDataXpo.LastTime > statisticData.Value.LastTime)
                        continue;

                    statisticDataXpo.LastTime = statisticData.Value.LastTime;
                    statisticDataXpo.Name = statisticData.Value.Name;
                    statisticDataXpo.Description = statisticData.Value.Description.Length > 4000
                        ? statisticData.Value.Description.Substring(0, 4000)
                        : statisticData.Value.Description;
                    statisticDataXpo.Count = statisticData.Value.Count;
                }

                uow.CommitChanges();
            }
        }

        private static string GetSimpleVersion(string osVersion)
        {
            var groups = Regex.Match(osVersion, @"Microsoft Windows NT (\d)\.(\d)").Groups;
            switch (groups[1].Value)
            {
                case "5":
                    return "Windows XP";
                case "6":
                    switch (groups[2].Value)
                    {
                        case "0":
                            return "Windows Vista";
                        case "1":
                            return "Windows 7";
                        case "2":
                            return "Windows 8";
                        default:
                            return osVersion;
                    }
                default:
                    return osVersion;
            }
        }

        private sealed class StatisticData
        {
            public DateTime LastTime { get; set; }
            public int Count { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
        }
    }
}