using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Ionic.Zip;

namespace CleanOldFeedbacks
{
    internal static class Program
    {
        private const string DatabasesTimexFeedbacks = @"d:\Feedbacks\";
        private static readonly DateTime DummyDate = new DateTime( 1900, 1, 1 );
        private static readonly List< string > ExceptList = new List< string > { "3.1.", "Bugs" };

        private static int Main( string[] args )
        {
            try
            {
                var funcs = new List< Func< string, IEnumerable< string > > > { GetBadFiles, GetEmptyDirs };
                var listInternalDirs = new List< string > { "Error", "Contrafact" };
                File.WriteAllText( args[ 1 ],
                    string.Join( Environment.NewLine,
                        Directory.EnumerateDirectories( DatabasesTimexFeedbacks ).
                            Where( dir => ExceptList.All( el => !dir.Contains( el ) ) ).
                            SelectMany( dir => EnumerableEx.Return( dir ).Union( listInternalDirs.Select( id => Path.Combine( dir, id ) ) ) ).
                            SelectMany( dir => funcs[ Convert.ToInt32( args[ 0 ] ) ]( dir ) ).
                            OrderByDescending( file => file.Length ) ), Encoding.UTF8 );
                return 0;
            }
            catch ( Exception ex )
            {
                Console.WriteLine( ex.Message );
                return -1;
            }
        }

        private static IEnumerable< string > GetEmptyDirs( string path )
        {
            return !Directory.Exists( path ) || Directory.EnumerateFiles( path, "*", SearchOption.AllDirectories ).Any() ? Enumerable.Empty< string >() : EnumerableEx.Return( path );
        }

        private static IEnumerable< string > GetBadFiles( string path )
        {
            if ( !Directory.Exists( path ) )
                return Enumerable.Empty< string >();
            var groupsFiles = from file in Directory.EnumerateFiles( path ) let fileInfo = ParseInfoFromFileName( file ) group new { file, fileInfo } by fileInfo.Item1 + fileInfo.Item2;
            var badIPs = new[] { "62.141.68.238" };
            var badComputerNames = new[] { "DONNA-PC" };
            return groupsFiles.SelectMany( gr =>
                                           {
                                               var badFiles = gr.Where( fl => fl.fileInfo.Item3 == DummyDate || badIPs.Any( ip => fl.fileInfo.Item1 == ip ) || badComputerNames.Any( comp => fl.fileInfo.Item2 == comp ) ).ToList();
                                               return ( from fileWitInfo in gr.Except( badFiles ) orderby fileWitInfo.fileInfo.Item3 descending select fileWitInfo.file ).Skip( 1 ).Union( badFiles.Select( fl => fl.file ) );
                                           } );
        }

        private static Tuple< string, string, DateTime > ParseInfoFromFileName( string file )
        {
            var match = Regex.Match( file,
                @"(\b25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\b)\s(.*)\s(\d*)\.(\d*)\s(\d*)_(\d*)" );

            var lastTime = DummyDate;
            try
            {
                using ( var zipFile = ZipFile.Read( file ) )
                    lastTime = ( from zipEntry in zipFile select zipEntry.LastModified ).Max();
            }
            catch ( Exception )
            {
            }

            return new Tuple< string, string, DateTime >( match.Groups[ 1 ].Value, match.Groups[ 5 ].Value, lastTime );
        }
    }
}