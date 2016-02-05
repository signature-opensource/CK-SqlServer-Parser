using CK.Core;
using CK.SqlServer.Parser;
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
        public readonly bool CombineElementType;
        public readonly XElement Expected;
        public readonly XElement ExpectedStatements;
        public readonly string[] ToStringCompactForms;

        public XmlSqlTester( XElement t )
        {
            TestElement = t;
            Mode = t.GetAttributeEnum( "Mode", ParseMode.AllStatements );
            // TrimEnd the text because the last trivia is skipped.
            Text = t.Element( "Text" ).Value.TrimEnd().NormalizeEOL();
            Description = t.Elements( "Description" ).Select( e => e.Value ).FirstOrDefault();
            XElement xmlTestElement = t.Element( "Xml" );
            CombineElementType = xmlTestElement != null ? xmlTestElement.GetAttributeBoolean( "CombineElementType", false ) : false;
            Expected = xmlTestElement != null ? xmlTestElement.Element( "Sql" ) : null;
            var s = (string)xmlTestElement.Attribute( "ToStringCompact" );
            if( s != null ) ToStringCompactForms = s.Split( ',' ).Select( f => f.Trim() ).ToArray();
            else ToStringCompactForms = Util.EmptyStringArray;
            ExpectedStatements = xmlTestElement != null ? xmlTestElement.Element( "Statements" ) : null;
        }

        public virtual void ParseAndCheck()
        {
            ISqlNode e;
            SqlAnalyser.ErrorResult r = SqlAnalyser.Parse( out e, Mode, Text );
            Assert.That( r.IsError, Is.False, r.ToString() );
            Assert.That( e.ToString( true, true ).NormalizeEOL(), Is.EqualTo( Text ) );
            e = OnParsed( e );
            if( Expected != null )
            {
                using( TestHelper.ConsoleMonitor.OpenInfo().Send( "Checking detailed Xml." ) )
                {
                    XElement visited = new SqlToXmlVisitor( CombineElementType, ToStringCompactForms ).ToXml( "Sql", e );
                    string visitedString = visited.ToString();
                    TestHelper.ConsoleMonitor.Trace().Send( visitedString );
                    if( !XNode.DeepEquals( visited, Expected ) )
                    {
                        TestHelper.AssertXmlStringEqual( visitedString, Expected );
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
                        TestHelper.AssertXmlStringEqual( visitedString, ExpectedStatements );
                    }
                }
            }
        }

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
