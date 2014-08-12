using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using DevExpress.Xpo;
using DevExpress.Xpo.DB;
using DevExpress.Xpo.Metadata;
using Ionic.Zip;

namespace StatisticApp
{
    internal static class Program
    {
        private const string DatabasesTimexFeedbacks = @"d:\Temp\Feedbacks\";

        private static int Main()
        {
            try
            {
               // ExtractTimexLog();
                var reflectionDict = new ReflectionDictionary();
                reflectionDict.GetDataStoreSchema( typeof ( ClientXPO ).Assembly );
                XpoDefault.DataLayer = new ThreadSafeDataLayer( reflectionDict,
                    new MSSqlConnectionProvider( new SqlConnection( ConfigurationManager.ConnectionStrings[ "ConnectionString" ].ConnectionString ), AutoCreateOption.DatabaseAndSchema ) );
                XpoDefault.Session = null;

                new Statistic();
                /*switch ( args[ 0 ] )
                {
                    case "2":
                        
                        break;
                    case "3":
                        GetStatisticData( args[ 1 ], args[ 2 ], args[ 3 ] );
                        break;
                }*/
                return 0;
            }
            catch ( Exception ex )
            {
                Console.WriteLine( ex.Message + Environment.NewLine + ex.StackTrace );
                return -1;
            }
        }

        private static void GetStatisticData( string dirTemp, string file, string verbose )
        {
            //dirTemp = @"c:\temp\1\";
            //new Statistic( dirTemp, file, verbose == "1" );
        }

        private static void ExtractTimexLog()
        {
            const string tempDir = @"C:\Temp\Feedbacks";
            if ( Directory.Exists( tempDir ) )
                Directory.Delete( tempDir, true );

            foreach ( var file in Directory.EnumerateFiles( DatabasesTimexFeedbacks, "*.zip", SearchOption.AllDirectories ) )
            {
                using ( var zip1 = ZipFile.Read( file ) )
                {
                    // here, we extract every entry, but we could extract conditionally
                    // based on entry name, size, date, checkbox status, etc.  
                    var zipExtTemp = Path.Combine( tempDir, Guid.NewGuid().ToString() );
                    foreach ( var e in zip1.Where( item => !item.IsDirectory && item.LastModified > new DateTime( 2013, 08, 31 ) && Path.GetFileName( item.FileName ).Contains( ".log" ) ) )
                    {
                        try
                        {
                            e.Extract( zipExtTemp );
                        }
                        catch ( Exception )
                        {
                        }
                    }
                }
            }
        }
    }
}