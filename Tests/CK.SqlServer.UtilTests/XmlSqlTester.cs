using CK.Core;
using CK.SqlServer.Parser;
using CK.Text;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CK.SqlServer.UtilTests
{
    public class XmlSqlTester
    {
        public readonly XElement TestElement;
        public readonly string Text;
        public readonly string Description;
        public readonly ParseMode Mode;

        public readonly XElement ExpectedXml;
        public readonly bool CombineElementType;
        public readonly string[] ToStringCompactForms;

        public readonly XElement ExpectedStatementsXml;

        public XmlSqlTester( XElement t )
        {
            TestElement = t;
            Mode = t.AttributeEnum( "Mode", ParseMode.OneOrMoreStatements );
            // TrimEnd the text because the last trivia is skipped.
            Text = t.Element( "Text" ).Value.TrimEnd().NormalizeEOL();
            Description = t.Elements( "Description" ).Select( e => e.Value.NormalizeEOL() ).FirstOrDefault();

            XElement xmlTestElement = t.Element( "Xml" );
            if( xmlTestElement != null )
            {
                var ce = xmlTestElement.Attribute( "CombineElementType" );
                CombineElementType = ce == null ? false : (bool)ce;
                ExpectedXml = xmlTestElement.Element( "Sql" );
                if( ExpectedXml != null )
                {
                    ExpectedXml.DescendantNodes().OfType<XComment>().Remove();
                }
                var s = (string)xmlTestElement.Attribute( "ToStringCompact" );
                if( s != null ) ToStringCompactForms = s.Split( ',' ).Select( f => f.Trim() ).ToArray();
                else ToStringCompactForms = Util.Array.Empty<string>();

                ExpectedStatementsXml = xmlTestElement.Element( "Statements" );
                if( ExpectedStatementsXml != null )
                {
                    ExpectedStatementsXml.DescendantNodes().OfType<XComment>().Remove();
                }
            }
        }

        public virtual void ParseAndCheck()
        {
            ISqlNode e = ParseAndCheckSqlText( Text );
            e = OnParsed( e );
            if( e != null )
            {
                if( ExpectedXml != null )
                {
                    using( TestHelper.ConsoleMonitor.OpenInfo().Send( "Checking detailed Xml." ) )
                    {
                        XElement visited = new SqlToXmlVisitor( CombineElementType, ToStringCompactForms ).ToXml( "Sql", e );
                        string visitedString = visited.ToString();
                        TestHelper.ConsoleMonitor.Trace().Send( visitedString );
                        if( !XNode.DeepEquals( visited, ExpectedXml ) )
                        {
                            TestHelper.AssertXmlStringEqual( visitedString, ExpectedXml );
                        }
                    }
                }
                if( ExpectedStatementsXml != null )
                {
                    using( TestHelper.ConsoleMonitor.OpenInfo().Send( "Checking statements only Xml." ) )
                    {
                        XElement visited = new SqlToXmlStatementVisitor().ToXml( "Statements", e );
                        string visitedString = visited.ToString();
                        TestHelper.ConsoleMonitor.Trace().Send( visitedString );
                        if( !XNode.DeepEquals( visited, ExpectedStatementsXml ) )
                        {
                            TestHelper.AssertXmlStringEqual( visitedString, ExpectedStatementsXml );
                        }
                    }
                }
            }
        }

        protected ISqlNode ParseAndCheckSqlText( string text )
        {
            ISqlNode e;
            SqlAnalyser.ErrorResult r = SqlAnalyser.Parse( out e, Mode, text );
            Assert.That( r.IsError, Is.False, r.ToString() );
            string backFromTree = e.ToString( true, true );
            if( backFromTree != text )
            {
                var tokens = new SqlTokenizer().Parse( text )
                                .Where( t => t.TokenType != SqlTokenType.EndOfInput )
                                .ToList();

                string backFromTokens = string.Join( "", tokens.Select( t => t.ToString( true, true ) ) );
                if( backFromTokens != text )
                {
                    Assert.That( backFromTokens, Is.EqualTo( text ), "Bug in tokenizer." );
                }
                Assert.That( backFromTree, Is.EqualTo( text ), "Bug in parser." );
            }
            return e;
        }

        /// <summary>
        /// If this returns null, all checks are skipped.
        /// </summary>
        /// <param name="e">The parsed node.</param>
        /// <returns>The parsed node, a transformed node, or null.</returns>
        protected virtual ISqlNode OnParsed( ISqlNode e )
        {
            return e;
        }

        public static void RunAllTests( string fileName, Func<XElement, XmlSqlTester> oneTestCreate, string folderName = "XmlTests" )
        {
            using( TestHelper.ConsoleMonitor.OpenInfo().Send( $"Running {fileName}." ) )
            {
                XElement tests = XDocument.Load( TestHelper.GetFolder( folderName, fileName ) ).Root;
                int i = 0;
                foreach( var t in tests.Elements( "Test" ) )
                {
                    XmlSqlTester x = oneTestCreate( t );
                    using( TestHelper.ConsoleMonitor.OpenInfo().Send( $"n°{i}-{x.Description} ({x.Mode.ToString()})" ) )
                    {
                        TestHelper.ConsoleMonitor.Trace().Send( x.Text );
                        x.ParseAndCheck();
                        ++i;
                    }
                }
            }

        }
    }

}
