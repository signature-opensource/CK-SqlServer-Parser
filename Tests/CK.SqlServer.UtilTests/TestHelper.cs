using System.IO;
using NUnit.Framework;
using CK.Core;
using System;
using System.Linq;
using System.Diagnostics;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using CK.SqlServer.Parser;
using CK.Text;
using System.Reflection;
using NUnit.Framework.Constraints;
using System.Collections.Generic;

namespace CK.SqlServer.UtilTests
{
#if NET451
    public static class Does
    {
        public static SubstringConstraint Contain(string expected) => Is.StringContaining(expected);

        public static EndsWithConstraint EndWith(string expected) => Is.StringEnding(expected);

        public static StartsWithConstraint StartWith(string expected) => Is.StringStarting(expected);

        public static ConstraintExpression Not => Is.Not;

        public static SubstringConstraint Contain(this ConstraintExpression @this, string expected) => @this.StringContaining(expected);
    }
#endif


    public static class TestHelper
    {
        static string _solutionFolder;
        static string _configuration;

        static IActivityMonitor _monitor;
        static ActivityMonitorConsoleClient _console;

        static TestHelper()
        {
            _monitor = new ActivityMonitor();
            _monitor.Output.BridgeTarget.HonorMonitorFilter = false;
            _console = new ActivityMonitorConsoleClient();
        }

        public static IActivityMonitor ConsoleMonitor => _monitor;

        public static bool LogsToConsole
        {
            get { return _monitor.Output.Clients.Contains( _console ); }
            set
            {
                if( value != LogsToConsole )
                {
                    if( value )
                    {
                        _monitor.Output.RegisterUniqueClient( c => c == _console, () => _console );
                        _monitor.Info().Send( "Enabled Logs to console." );
                    }
                    else
                    {
                        _monitor.Info().Send( "Disabled Logs to console." );
                        _monitor.Output.UnregisterClient( _console );
                    }
                }
            }
        }

        public static string SolutionFolder
        {
            get
            {
                if (_solutionFolder == null) InitalizePaths();
                return _solutionFolder;
            }
        }

        public static string Configuration
        {
            get
            {
                if (_solutionFolder == null) InitalizePaths();
                return _configuration;
            }
        }

        public static string CurrentTestProjectName
        {
            get
            {
                var transform = SimpleTypeFinder.WeakResolver("CK.SqlServer.Transform.SqlTransformHost, CK.SqlServer.Transform", false);
                string project;
                if (transform != null)
                {
                    project = "CK.SqlServer.Transform.Tests";
                }
                else project = "CK.SqlServer.Parser.Tests";
                return project;
            }
        }

        public static string BuildPathInCurrentTestProject( params string[] subNames )
        {
            var all = new List<string>();
            all.Add(SolutionFolder);
            all.Add("Tests");
            all.Add(CurrentTestProjectName);
            all.AddRangeArray( subNames );
            return Path.Combine( all.ToArray() );
        }

        public static string LoadTextFromParsingScripts( string fileName )
        {
            return File.ReadAllText( TestHelper.BuildPathInCurrentTestProject( "Parsing", "Scripts", fileName ) ).NormalizeEOL();
        }

        public static void AssertXmlStringEqual( string visitedString, XElement expected )
        {
            visitedString = Regex.Replace( visitedString, @"\s+", " ", RegexOptions.CultureInvariant );
            string es = expected.ToString();
            es = Regex.Replace( es, @"\s+", " ", RegexOptions.CultureInvariant );
            Assert.That( visitedString, Is.EqualTo( es ) );
        }


        [DebuggerStepThrough]
        public static T ParseOneStatementAndCheckString<T>( string text, bool addSemiColon = false ) where T : ISqlStatement
        {
            text = text.NormalizeEOL();
            if( addSemiColon ) text += ';';
            ISqlStatement statement;
            SqlAnalyser.ErrorResult r = SqlAnalyser.ParseStatement( out statement, text );
            Assert.That( !r.IsError, r.ToString() );
            Assert.That( statement, Is.InstanceOf<T>() );
            T s = (T)statement;
            Assert.That( statement.ToString( true ).NormalizeEOL(), Is.EqualTo( text ) );
            if( TestHelper.LogsToConsole ) Console.WriteLine( statement.ToXml() );
            return s;
        }

        /// <summary>
        /// Parses the one statement that must be the first one (other statements may follow).
        /// </summary>
        /// <typeparam name="T">Type of the statement to parse.</typeparam>
        /// <param name="text">Text to parse.</param>
        /// <returns>Statement.</returns>
        [DebuggerStepThrough]
        public static T ParseOneStatement<T>( string text ) where T : ISqlStatement
        {
            text = text.NormalizeEOL();
            ISqlStatement statement;
            SqlAnalyser.ErrorResult r = SqlAnalyser.ParseStatement( out statement, text );
            Assert.That( !r.IsError, r.ToString() );
            Assert.That( statement, Is.InstanceOf<T>() );
            return (T)statement;
        }

        static void InitalizePaths()
        {
#if NET451
            string p = new Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).LocalPath;
            p = Path.GetDirectoryName(p);
#else
            string p = Directory.GetCurrentDirectory();
#endif
#if DEBUG
            _configuration = "Debug";
#else
            _configuration = "Release";
#endif
            while (!Directory.EnumerateFiles(p).Where(f => f.EndsWith(".sln")).Any())
            {
                p = Path.GetDirectoryName(p);
            }
            _solutionFolder = p;

            Console.WriteLine($"SolutionFolder is: {_solutionFolder}.");
            Console.WriteLine($"Core path: {typeof(string).GetTypeInfo().Assembly.CodeBase}.");
        }

    }
}
