using CK.Core;
using CK.SqlServer.Parser;
using CK.Testing;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using static CK.Testing.SqlTransformTestHelper;

namespace CK.SqlServer.UtilTests
{

    public static class TestHelperExtensions
    {
        public static string BuildPathInCurrentTestProject( this IBasicTestHelper @this, params string[] subNames )
        {
            var all = new List<string>();
            all.Add( BasicTestHelper.TestHelper.TestProjectFolder.ToString().Replace(".NetCore", "" ) );
            all.AddRangeArray( subNames );
            return Path.Combine( all.ToArray() );
        }

        public static string LoadTextFromParsingScripts( this IBasicTestHelper @this, string fileName )
        {
            return File.ReadAllText( BuildPathInCurrentTestProject( @this, "Parsing", "Scripts", fileName ) ).ReplaceLineEndings();
        }

        public static void AssertXmlStringEqual( this IBasicTestHelper @this, string visitedString, XElement expected )
        {
            visitedString = Regex.Replace( visitedString, @"\s+", " ", RegexOptions.CultureInvariant );
            string es = expected.ToString();
            es = Regex.Replace( es, @"\s+", " ", RegexOptions.CultureInvariant );
            visitedString.Should().Be( es );
        }


        [DebuggerStepThrough]
        public static T ParseOneStatementAndCheckString<T>( this IBasicTestHelper @this, string text, bool addSemiColon = false ) where T : ISqlStatement
        {
            text = text.ReplaceLineEndings();
            if( addSemiColon ) text += ';';
            ISqlStatement statement;
            SqlAnalyser.ErrorResult r = SqlAnalyser.ParseStatement( out statement, text );
            r.IsError.Should().BeFalse( r.ToString() );
            statement.Should().BeAssignableTo<T>();
            T s = (T)statement;
            statement.ToString( true ).ReplaceLineEndings().Should().Be( text );
            if( MonitorTestHelper.TestHelper.LogToConsole ) Console.WriteLine( statement.ToXml() );
            return s;
        }

        /// <summary>
        /// Parses the one statement that must be the first one (other statements may follow).
        /// </summary>
        /// <typeparam name="T">Type of the statement to parse.</typeparam>
        /// <param name="text">Text to parse.</param>
        /// <returns>Statement.</returns>
        [DebuggerStepThrough]
        public static T ParseOneStatement<T>( this IBasicTestHelper @this, string text ) where T : ISqlStatement
        {
            text = text.ReplaceLineEndings();
            ISqlStatement statement;
            SqlAnalyser.ErrorResult r = SqlAnalyser.ParseStatement( out statement, text );
            r.IsError.Should().BeFalse( r.ToString() );
            statement.Should().BeAssignableTo<T>();
            return (T)statement;
        }

    }
}
