using System.IO;
using NUnit.Framework;
using CK.Core;
using System;
using System.Linq;
using System.Diagnostics;

namespace CK.SqlServer.Parser.Tests
{
    static class TestHelper
    {
        static string _projectFolder;
        static string _scriptFolder;

        static IActivityMonitor _monitor;
        static ActivityMonitorConsoleClient _console;

        static TestHelper()
        {
            _monitor = new ActivityMonitor();
            _monitor.Output.BridgeTarget.HonorMonitorFilter = false;
            _console = new ActivityMonitorConsoleClient();
        }

        public static IActivityMonitor ConsoleMonitor
        {
            get { return _monitor; }
        }

        public static bool LogsToConsole
        {
            get { return _monitor.Output.Clients.Contains( _console ); }
            set
            {
                if( value ) _monitor.Output.RegisterUniqueClient( c => c == _console, () => _console );
                else _monitor.Output.UnregisterClient( _console );
            }
        }

        public static string FolderScript
        {
            get { if( _scriptFolder == null ) InitalizePaths(); return _scriptFolder; }
        }

        public static string GetScriptsFolder( string testName )
        {
            return Path.Combine( FolderScript, testName );
        }

        public static string GetFolder( params string[] subNames )
        {
            if( _projectFolder == null ) InitalizePaths();
            var a = new string[ subNames.Length + 1 ];
            a[0] = _projectFolder;
            Array.Copy( subNames, 0, a, 1, subNames.Length );
            return Path.Combine( a );
        }

        public static string LoadTextFromParsingScripts( string fileName )
        {
            return File.ReadAllText( TestHelper.GetFolder( "Parsing", "Scripts", fileName ) ).NormalizeEOL();
        }

        [DebuggerStepThrough]
        public static T ParseOneStatementAndCheckString<T>( string text, bool addSemiColon = false ) where T : SqlExprBaseSt
        {
            text = text.NormalizeEOL();
            if( addSemiColon ) text += ';';
            SqlExprBaseSt statement;
            SqlAnalyser.ErrorResult r = SqlAnalyser.ParseStatement( out statement, text );
            Assert.That( !r.IsError, r.ToString() );
            Assert.That( statement, Is.InstanceOf<T>() );
            T s = (T)statement;
            Assert.That( statement.ToString( true ).NormalizeEOL(), Is.EqualTo( text ) );
            return s;
        }

        /// <summary>
        /// Parses the one statement that must be the first one (other statements may follow).
        /// </summary>
        /// <typeparam name="T">Type of the statement to parse.</typeparam>
        /// <param name="text">Text to parse.</param>
        /// <returns>Statement.</returns>
        [DebuggerStepThrough]
        public static T ParseOneStatement<T>( string text ) where T : SqlExprBaseSt
        {
            text = text.NormalizeEOL();
            SqlExprBaseSt statement;
            SqlAnalyser.ErrorResult r = SqlAnalyser.ParseStatement( out statement, text );
            Assert.That( !r.IsError, r.ToString() );
            Assert.That( statement, Is.InstanceOf<T>() );
            return (T)statement;
        }

        private static void InitalizePaths()
        {
            string p = new Uri( System.Reflection.Assembly.GetExecutingAssembly().CodeBase ).LocalPath;
            // => CK.XXX.Tests/bin/Debug/
            p = Path.GetDirectoryName( p );
            // => CK.XXX.Tests/bin/
            p = Path.GetDirectoryName( p );
            // => CK.XXX.Tests/
            p = Path.GetDirectoryName( p );
            _projectFolder = p;
            _scriptFolder = Path.Combine( _projectFolder, "Scripts" );
        }

    }
}
