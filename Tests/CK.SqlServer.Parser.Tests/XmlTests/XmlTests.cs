using CK.Core;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CK.SqlServer.Parser.Tests.XmlTests
{

    [TestFixture]
    public class XmlTests
    {
        class XmlSqlTest
        {
            public readonly bool CombineElementType;
            public readonly XElement Expected;
            public readonly XElement ExpectedStatements;
            public readonly string[] ShortenForms;

            public XmlSqlTest( XElement xmlTestElement )
            {
                CombineElementType = xmlTestElement != null ? xmlTestElement.GetAttributeBoolean( "CombineElementType", false ) : false;
                Expected = xmlTestElement != null ? xmlTestElement.Element( "Sql" ) : null;
                var s = (string)xmlTestElement.Attribute( "Shorten" );
                if( s != null ) ShortenForms = s.Split( ',' ).Select( f => f.Trim() ).ToArray();
                else ShortenForms = Util.EmptyStringArray;
                ExpectedStatements = xmlTestElement != null ? xmlTestElement.Element( "Statements" ) : null;
            }

            public void ParseAndCheck( string text, ParseMode mode )
            {
                ISqlNode e;
                SqlAnalyser.ErrorResult r = SqlAnalyser.Parse( out e, mode, text );
                Assert.That( r.IsError, Is.False, r.ToString() );
                Assert.That( e.ToString( true ).NormalizeEOL(), Is.EqualTo( text ) );
                if( Expected != null )
                {
                    using( TestHelper.ConsoleMonitor.OpenInfo().Send( "Checking detailed Xml." ) )
                    {
                        XElement visited = new SqlToXmlVisitor( CombineElementType, ShortenForms ).ToXml( "Sql", e );
                        string visitedString = visited.ToString();
                        TestHelper.ConsoleMonitor.Trace().Send( visitedString );
                        if( !XNode.DeepEquals( visited, Expected ) )
                        {
                            AssertOnXmlString( visitedString, Expected );
                        }
                    }
                }
                if( ExpectedStatements != null )
                {
                    using( TestHelper.ConsoleMonitor.OpenInfo().Send( "Checking statements only Xml." ) )
                    {
                        XElement visited = new SqlToXmlStatementVisitor().ToXml( "Statements", e );
                        string visitedString = visited.ToString();
                        TestHelper.ConsoleMonitor.Trace().Send( visitedString );
                        if( !XNode.DeepEquals( visited, ExpectedStatements ) )
                        {
                            AssertOnXmlString( visitedString, ExpectedStatements );
                        }
                    }
                }
            }

            void AssertOnXmlString( string visitedString, XElement expected )
            {
                visitedString = Regex.Replace( visitedString, @"\s+", " ", RegexOptions.CultureInvariant | RegexOptions.Compiled );
                string es = expected.ToString();
                es = Regex.Replace( es, @"\s+", " ", RegexOptions.CultureInvariant | RegexOptions.Compiled );
                Assert.That( visitedString, Is.EqualTo( es ) );
            }
        }

        [TestCase( "LiteralTokens.xml" )]
        [TestCase( "Identifiers.xml" )]
        [TestCase( "Comma lists.xml" )]
        [TestCase( "Between expressions.xml" )]
        [TestCase( "Logical operators.xml" )]
        [TestCase( "Simple expressions.xml" )]
        [TestCase( "Unmodeled stuff.xml" )]
        [TestCase( "If.xml" )]
        [TestCase( "Simple Procedures.xml" )]
        [TestCase( "Simple Selects.xml" )]
        [TestCase( "Cursors.xml" )]
        [TestCase( "IsNull.xml" )]
        [TestCase( "Not so Simple Procedures.xml" )]
        [TestCase( "Sequence.xml" )]
        [TestCase( "CTE.xml" )]
        [TestCase( "Multi Statements.xml" )]
        [TestCase( "Insert.xml" )]
        [TestCase( "OpenXml.xml" )]
        [TestCase( "Update.xml" )]
        public void file_test( string fileName )
        {
            using( TestHelper.ConsoleMonitor.OpenInfo().Send( $"Running {fileName}." ) )
            {
                XElement tests = XDocument.Load( TestHelper.GetFolder( "XmlTests", fileName ) ).Root;
                int i = 0;
                foreach( var t in tests.Elements( "Test" ) )
                {
                    ParseMode mode = t.GetAttributeEnum<ParseMode>( "Mode", ParseMode.AllStatements );
                    // TrimEnd the text because the last trivia is skipped.
                    string text = t.Element( "Text" ).Value.TrimEnd().NormalizeEOL();
                    string desc = t.Elements( "Description" ).Select( e => e.Value ).FirstOrDefault();
                    XmlSqlTest xmlSql = new XmlSqlTest( t.Element( "Xml" ) );
                    using( TestHelper.ConsoleMonitor.OpenInfo().Send( $"n°{i}-{desc} ({mode.ToString()})" ) )
                    {
                        TestHelper.ConsoleMonitor.Trace().Send( text );
                        xmlSql.ParseAndCheck( text, mode );
                        ++i;
                    }
                }
            }
        }

    }
}
