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

        private readonly Dictionary< string, ClientXPO > clients;

        private readonly string dirScreenshots;
        private readonly string dirTemp;
        private readonly UnitOfWork uow;
        private Dictionary< string, string > dicNamesForOSVer;

        public Statistic()
        {
            dirTemp = @"d:\Temp\Extracted\";
            uow = new UnitOfWork();
            clients = new XPCollection< ClientXPO >( uow ).ToDictionary( item => item.HASP, item => item );
            dirScreenshots = @"c:\Temp\Screeens";

            if ( !Directory.Exists( dirScreenshots ) )
                Directory.CreateDirectory( dirScreenshots );
            Init();
            GetStatisticData();
        }

        private void Init()
        {
            //Directory.EnumerateFiles( dirS, "*.txt" ).ForEach( File.Delete );

            dicNamesForOSVer = new Dictionary< string, string >();
            dicNamesForOSVer[ "MachineName" ] = "MachineName";
            dicNamesForOSVer[ "OSVersion" ] = "OSVersion";
            dicNamesForOSVer = dicNamesForOSVer.OrderBy( item => item.Value ).ToDictionary( item => item.Key, item => item.Value );
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

        private void GetStatisticData()
        {
            Func< string, Dictionary< string, StatisticData > > getSdFromDir = file =>
                                                                               {
                                                                                   try
                                                                                   {
                                                                                       var ses = new LogFile( file ).OfType< LogEntry >().AsEnumerable().AsParallel().SelectMany( GetEntrySDs ).ToList();

                                                                                       return ses.GroupBy( sd => sd.Name ).Select( gr => gr.MaxBy( item => item.Time ).First() ).ToArray().ToDictionary( sd => sd.Name, sd => sd );
                                                                                   }
                                                                                   catch ( Exception )
                                                                                   {
                                                                                       return new Dictionary< string, StatisticData >();
                                                                                   }
                                                                               };

            //var list = new List< String >();
            foreach ( var log in Directory.EnumerateFiles( dirTemp, "*.*", SearchOption.AllDirectories ) )
            {
                var s = getSdFromDir( log );
                if ( !s.ContainsKey( haspid ) )
                    continue;

                var hasp = s[ haspid ].Value;
                var client = clients.ContainsKey( hasp ) ? clients[ hasp ] : clients[ hasp ] = new ClientXPO( uow ) { HASP = hasp };

                SetValueInt( s, "PullZKDevice1XPO", client.PullDevicesCountLastTime, i => client.PullDevicesCount = i, t => client.PullDevicesCountLastTime = t );
                SetValueInt( s, "ZKDevice1XPO", client.ZKDevicesCountLastTime, i => client.ZKDevicesCount = i, t => client.ZKDevicesCountLastTime = t );
                SetValueInt( s, "EmployerXPO", client.EmpsCountLastTime, i => client.EmpsCount = i, t => client.EmpsCountLastTime = t );
                SetValueInt( s, "TimexOperator2XPO", client.OperatorsCountLastTime, i => client.OperatorsCount = i, t => client.OperatorsCountLastTime = t );
                SetValueInt( s, "Количество сотрудников для учета рабочего времени", client.LicEmpsLastTime, i => client.LicEmps = i, t => client.LicEmpsLastTime = t );
                SetValueInt( s, "Количество операторов", client.LicOpersLastTime, i => client.LicOpers = i, t => client.LicOpersLastTime = t );
                SetValueInt( s, "Количество операторов SDK", client.LicOpersSDKLastTime, i => client.LicOpersSDK = i, t => client.LicOpersSDKLastTime = t );
                SetValueBool( s, "Модуль рабочего времени", client.LicTALastTime, i => client.LicTA = i, t => client.LicTALastTime = t );
                SetValueBool( s, "Модуль контроля доступа", client.LicACLastTime, i => client.LicAC = i, t => client.LicACLastTime = t );
                SetValueBool( s, "Модуль фотоверификации", client.LicPhotoLastTime, i => client.LicPhoto = i, t => client.LicPhotoLastTime = t );
                SetValueBool( s, "Модуль печати", client.LicBadjLastTime, i => client.LicBadj = i, t => client.LicBadjLastTime = t );
                SetValueString( s, "AppVersion", client.AppVersionLastTime, i => client.AppVersion = i, t => client.AppVersionLastTime = t );
                SetValueString( s, "ActivationStatus", client.ActivationStatusLastTime, i => client.ActivationStatus = i, t => client.ActivationStatusLastTime = t );
                SetValueString( s, "Поддержка до", client.SupportToLastTime, i => client.SupportTo = i, t => client.SupportToLastTime = t );
                // list.AddRange( s.Keys );
                //var b = MoreEnumerable.ToDelimitedString( list.Distinct().OrderBy( item => item ).ToList(), Environment.NewLine );
                uow.CommitChanges();
            }
        }

        private static void SetValueInt( IReadOnlyDictionary< string, StatisticData > s, string key, DateTime time, Action< int? > store, Action< DateTime > timeStore )
        {
            if ( !s.ContainsKey( key ) )
                return;
            if ( s[ key ].Time <= time )
                return;

            timeStore( s[ key ].Time );
            store( Convert.ToInt32( s[ key ].Value ) );
        }

        private static void SetValueString( IReadOnlyDictionary< string, StatisticData > s, string key, DateTime time, Action< string > store, Action< DateTime > timeStore )
        {
            if ( !s.ContainsKey( key ) )
                return;
            if ( s[ key ].Time <= time )
                return;

            timeStore( s[ key ].Time );
            store( s[ key ].Value );
        }

        private static void SetValueBool( IReadOnlyDictionary< string, StatisticData > s, string key, DateTime time, Action< bool? > store, Action< DateTime > timeStore )
        {
            if ( !s.ContainsKey( key ) )
                return;
            if ( s[ key ].Time <= time )
                return;

            timeStore( s[ key ].Time );
            store( Convert.ToBoolean( s[ key ].Value ) );
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