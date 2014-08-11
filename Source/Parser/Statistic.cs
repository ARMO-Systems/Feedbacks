using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Gurock.SmartInspect;
using Gurock.SmartInspect.SDK;

namespace StatisticApp
{
    internal class Statistic
    {
        private const string separator = ";";
        private const string haspid = "HaspID";
        private const string version = "VersionInfo";

        private readonly List< string > DBInfoFields = new List< string >
            {
                "CategoryXPO",
                "KorrektirovkaXPO",
                "DepartmentXPO",
                "CompanyXPO",
                "DopSchituvatel1XPO",
                "BadgeLayoutXPO",
                "PhotoVerificationXPO",
                "RabochayaStanciyaXPO",
                "ReportLayoutXPO",
                "ReportPluginXPO",
                "PullZKDevice1XPO",
                "SagemDevice1XPO",
                "ZKDevice1XPO",
                "LogicalElementXPO",
                "LogicalLinkXPO",
                "PunktDostupa2XPO",
                "ShluzXPO",
                "TochkaRegistraciiXPO",
                "PullZKTochkaRegistraciiXPO",
                "HirschTochkaRegistraciiXPO",
                "KeriTochkaRegistraciiXPO",
                "SagemTochkaRegistraciiXPO",
                "ZKTochkaRegistraciiXPO",
                "EmployerXPO",
                "GrafikRabotiXPO",
                "GruppaKontrolnixTochekXPO",
                "KontrolnayaTochkaXPO",
                "LenelTochkaRegistraciiXPO",
                "PostXPO",
                "RabochayaOblastXPO",
                "RoleXPO",
                "RuleXPO",
                "SagemGruppaDostupaXPO",
                "SmenaXPO",
                "VhodXPO",
                "VihodXPO",
                "SystemPluginXPO",
                "TimexOperator2XPO",
                "UrovenDostupaXPO",
                "VremennayaZonaXPO",
                "OblastXPO"
            };

        private readonly string dirS;
        private readonly string dirTemp;
        private readonly bool verboseStatistic;
        private Dictionary< string, string > dicNamesForDBData;
        private Dictionary< string, string > dicNamesForOSVer;
        private readonly string dirScreenshots;

        public Statistic( string dirTemp, string dir, bool verbose = false )
        {
            this.dirTemp = dirTemp;
            //this.dirTemp = @"d:\2\";
            dirS = dir;
            if ( !Directory.Exists( dirS ) )
                Directory.CreateDirectory( dirS );

            dirScreenshots = Path.Combine( dirS, "Screenshots" );

            if ( !Directory.Exists( dirScreenshots ) )
                Directory.CreateDirectory( dirScreenshots );
            verboseStatistic = verbose;
            Init();
            GetStatisticData();
        }

        private void Init()
        {
            Directory.EnumerateFiles( dirS, "*.txt" ).ForEach( File.Delete );

            dicNamesForOSVer = new Dictionary< string, string >();
            dicNamesForOSVer[ "MachineName" ] = "MachineName";
            dicNamesForOSVer[ "OSVersion" ] = "OSVersion";
            dicNamesForOSVer = dicNamesForOSVer.OrderBy( item => item.Value ).ToDictionary( item => item.Key, item => item.Value );

            dicNamesForDBData = new Dictionary< string, string >();
            dicNamesForDBData = DBInfoFields.OrderBy( item => item ).ToDictionary( item => item, item => item );
        }

        private IEnumerable< StatisticData > GetEntrySDs( LogEntry entry )
        {
            var entryValue = new Lazy< string >( () =>
                                                     {
                                                         using ( var sr = new StreamReader( entry.Data ) )
                                                             return sr.ReadToEnd();
                                                     } );

            Func< string, List< Tuple< string, string > > > getValuesByRegex = regex =>
                                                                                   {
                                                                                       var matchResult = new Regex( regex ).Match( entryValue.Value );
                                                                                       var ret = new List< Tuple< string, string > >();
                                                                                       while ( matchResult.Success )
                                                                                       {
                                                                                           ret.Add( new Tuple< string, string >( matchResult.Groups[ 1 ].Value, matchResult.Groups[ 2 ].Value ) );
                                                                                           matchResult = matchResult.NextMatch();
                                                                                       }
                                                                                       return ret;
                                                                                   };

            const string regexBeforeKateLogDic38 = "(.*): ([^\r]*)";
            const string regexAfterKateLogDic38 = "(.*)=(.*)\r\n";

            var infos = new Lazy< List< Tuple< string, string > > >( () => getValuesByRegex( entry.ViewerId == ViewerId.ValueList ? regexAfterKateLogDic38 : regexBeforeKateLogDic38 ) );
            Func< bool, Func< Tuple< string, string >, StatisticData >, IEnumerable< StatisticData > > infoFromDic =
                ( predic, func ) => predic && entry.Data != null ? infos.Value.Select( func ) : Enumerable.Empty< StatisticData >();

            var dbInfo = DBInfoFields.Where( item => entry.Title == item ).Select( item => new StatisticData( item, entry.Timestamp, infos.Value.First( item1 => item1.Item1 == "Count" ).Item2 ) );
            var haspInfo = infoFromDic( entry.Title == "Инфа о HASP", tp => new StatisticData( tp.Item1, entry.Timestamp, tp.Item2 ) );
            var osInfo = infoFromDic( entry.Title == "OSInfo" && !entry.Title.ToLower().Contains( "дамп реестра с эмуляторами" ), tp => new StatisticData( tp.Item1, entry.Timestamp, GetSimpleVersion( tp.Item2 ) ) );

            Func< IEnumerable< StatisticData > > oldVer = () =>
                                                              {
                                                                  if ( string.IsNullOrEmpty( entry.Title ) || entry.Title[ 0 ] != '3' )
                                                                      return Enumerable.Empty< StatisticData >();

                                                                  var cifry = entry.Title.Split( '.' );
                                                                  return entry.LogEntryType == LogEntryType.Message && cifry.Count() == 4 && cifry[ 0 ] == "3"
                                                                             ? EnumerableEx.Return( new StatisticData( version, entry.Timestamp, cifry[ 0 ] + "." + cifry[ 1 ] ) )
                                                                             : Enumerable.Empty< StatisticData >();
                                                              };

            var verInfo = entry.Title == "AppVersion" ? infoFromDic( true, tp => new StatisticData( tp.Item1, entry.Timestamp, tp.Item2 ) ) : oldVer();

            if ( entry.Title == "Скриншот" )
            {
                var fileName = Path.Combine( dirScreenshots, entry.Timestamp.ToString( "yyyy_MM_dd_hh_mm_ss_FFFFFFF" ) + ".jpeg" );
                var fs = new FileStream( fileName, FileMode.Create, FileAccess.ReadWrite );
                var memoryStream = new MemoryStream();
                entry.Data.CopyTo( memoryStream );

                var bw = new BinaryWriter( fs );
                bw.Write( memoryStream.ToArray() );
                bw.Close();
            }

            return dbInfo.Concat( haspInfo ).Concat( osInfo ).Concat( verInfo );
        }

        private static string GetSafeValueFromSDS( IReadOnlyDictionary< string, StatisticData > dic, string key )
        {
            return dic.ContainsKey( key ) ? dic[ key ].Value : string.Empty;
        }

        private void GetStatisticData()
        {
            Func< string, Dictionary< string, StatisticData > > getSdFromDir = dir =>
                                                                                   {
                                                                                       var ses =
                                                                                           Directory.EnumerateFiles( dir, "*.*", SearchOption.AllDirectories ).
                                                                                               SelectMany( item => new LogFile( item ).OfType< LogEntry >() ).
                                                                                               AsParallel().
                                                                                               SelectMany( GetEntrySDs ).
                                                                                               ToList();

                                                                                       return ses.GroupBy( sd => sd.Name ).Select( gr => gr.MaxBy( item => item.Time ).First() ).ToArray().ToDictionary( sd => sd.Name, sd => sd );
                                                                                   };

            Func< Dictionary< string, StatisticData >, string > getVersionFromSds = sds => GetSafeValueFromSDS( sds, version );
            Func< Dictionary< string, StatisticData >, string > getHaspAndCompFromSds = sds => GetSafeValueFromSDS( sds, haspid ) + "_" + GetSafeValueFromSDS( sds, "MachineName" );

            var entriesByFile = Directory.EnumerateDirectories( dirTemp, "*.*" ).Select( getSdFromDir ).Where( dic => dic.Count > 0 && dic.ContainsKey( version ) ).ToList();
            var info =
                entriesByFile.GroupBy( getVersionFromSds ).
                    SelectMany(
                        item =>
                        item.GroupBy( getHaspAndCompFromSds ).
                            Select( item1 => item1.SelectMany( i => i ).GroupBy( sd => sd.Key ).Select( gr => gr.MaxBy( item2 => item2.Value.Time ).First() ).ToDictionary( i => i.Key, i => i.Value ) ) );
            AppendFile( "OS", dicNamesForOSVer, info );

            var infoDb =
                entriesByFile.GroupBy( getVersionFromSds ).
                    SelectMany(
                        item =>
                        item.GroupBy( sds => GetSafeValueFromSDS( sds, haspid ) ).
                            Select( item1 => item1.SelectMany( i => i ).GroupBy( sd => sd.Key ).Select( gr => gr.MaxBy( item2 => item2.Value.Time ).First() ).ToDictionary( i => i.Key, i => i.Value ) ) );
            AppendFile( "DB", dicNamesForDBData, infoDb );
        }

        private void AppendFile( string stat, Dictionary< string, string > dicNames, IEnumerable< Dictionary< string, StatisticData > > info )
        {
            var file = Path.Combine( dirS, string.Format( "{0}.txt", stat ) );

            File.AppendAllText( file, string.Join( separator, new List< string > { haspid, version, "Time" }.Concat( dicNames.Keys ) ) + Environment.NewLine );
            File.AppendAllText( file,
                                string.Join( Environment.NewLine,
                                             info.OrderBy( item => GetSafeValueFromSDS( item, haspid ) ).
                                                 Select(
                                                     item =>
                                                     string.Join( separator,
                                                                  ( !verboseStatistic
                                                                        ? new List< string > { GetSafeValueFromSDS( item, haspid ), GetSafeValueFromSDS( item, version ), item.First().Value.Time.ToShortDateString() }.Concat(
                                                                            dicNames.Keys.Select( i1 => GetSafeValueFromSDS( item, i1 ) ) )
                                                                        : item.Values.Select( i => i.Name + "=" + i.Value ) ) ) ) ) );
            File.AppendAllText( file, Environment.NewLine );
        }

        private static string GetSimpleVersion( string osVersion )
        {
            var groups = Regex.Match( osVersion, @"Microsoft Windows NT (\d)\.(\d)" ).Groups;
            switch ( groups[ 1 ].Value )
            {
                case "5":
                    return "Windows XP";
                case "6":
                    switch ( groups[ 2 ].Value )
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
            private readonly string name;
            private readonly DateTime time;
            private readonly string value;

            public StatisticData( string name, DateTime time, string value )
            {
                this.name = name;
                this.time = time;
                this.value = value;
            }

            public string Name
            {
                get { return name; }
            }

            public DateTime Time
            {
                get { return time; }
            }

            public string Value
            {
                get { return value; }
            }
        }
    }
}