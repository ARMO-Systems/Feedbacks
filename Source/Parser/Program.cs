using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ionic.Zip;

namespace StatisticApp
{
    internal static class Program
    {
        private const string DatabasesTimexFeedbacks = @"d:\Databases\Timex 2\Feedbacks\";

        private static int Main( string[] args )
        {
            try
            {
                switch ( args[ 0 ] )
                {
                    case "2":
                        ExtractTimexLog( args[ 1 ] );
                        break;
                    case "3":
                        GetStatisticData( args[ 1 ], args[ 2 ], args[ 3 ] );
                        break;
                }
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
            new Statistic( dirTemp, file, verbose == "1" );
        }

        private static void ExtractTimexLog( string dirTemp )
        {
            if ( Directory.Exists( dirTemp ) )
                Directory.Delete( dirTemp, true );

            foreach (
                var dir in Directory.EnumerateDirectories( DatabasesTimexFeedbacks, "3.8*", SearchOption.TopDirectoryOnly ).Union( Directory.EnumerateDirectories( DatabasesTimexFeedbacks, "3.8*", SearchOption.TopDirectoryOnly ) )
                )
            {
// ReSharper disable LoopCanBePartlyConvertedToQuery
                foreach ( var fileZ in
// ReSharper restore LoopCanBePartlyConvertedToQuery
                    Directory.EnumerateFiles( dir, "*.zip", SearchOption.AllDirectories ) )

                {
                    var zip = ZipFile.Read( fileZ );
                    var zipExtTemp = Path.Combine( dirTemp, Path.GetFileNameWithoutExtension( fileZ ) + Guid.NewGuid() );
                    foreach ( var timexLog in zip.Where( ze => !ze.IsDirectory && IsTimexLog( Path.GetFileName( ze.FileName ) ) ) )
                    {
                        try
                        {
                            timexLog.Extract( zipExtTemp );
                        }
                        catch ( Exception )
                        {
                        }
                    }
                }
            }
        }

        private static bool IsTimexLog( string fileName )
        {
            var listContain = new List< string > { ".log" };
            return listContain.All( fileName.ToLower().Contains );
        }
    }
}