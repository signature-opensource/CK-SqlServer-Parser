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
        public void file_test( string fileName )
        {
            using( TestHelper.ConsoleMonitor.OpenInfo().Send( $"Running {fileName}." ) )
            {
                XElement tests = XDocument.Load( TestHelper.GetFolder( "XmlTests", fileName ) ).Root;
                int i = 0;
                foreach( var t in tests.Elements( "Test" ) )
                {
                    ParseMode mode = t.GetAttributeEnum<ParseMode>( "Mode", ParseMode.AllStatements );
                    string text = t.Element( "Text" ).Value.NormalizeEOL();
                    string desc = t.Elements( "Description" ).Select( e => e.Value ).FirstOrDefault();
                    bool combineElementType = t.Element( "Xml" ).GetAttributeBoolean( "CombineElementType", false );
                    XElement expected = t.Element( "Xml" ).Element( "Sql" );
                    using( TestHelper.ConsoleMonitor.OpenInfo().Send( $"n°{i}-{desc}: {text}. ({mode.ToString()})" ) )
                    {
                        ISqlNode e;
                        SqlAnalyser.ErrorResult r = SqlAnalyser.Parse( out e, mode, text );
                        Assert.That( r.IsError, Is.False, r.ToString() );
                        SqlToXmlVisitor v = new SqlToXmlVisitor( combineElementType );
                        XElement x = v.ToXml( "Sql", e );
                        string xs = x.ToString();
                        TestHelper.ConsoleMonitor.Trace().Send( xs );
                        Assert.That( e.ToString( true ).NormalizeEOL(), Is.EqualTo( text ) );
                        if( !XNode.DeepEquals( x, expected ) )
                        {
                            xs = Regex.Replace( xs, @"\s+", " ", RegexOptions.CultureInvariant | RegexOptions.Compiled );
                            string es = expected.ToString();
                            es = Regex.Replace( es, @"\s+", " ", RegexOptions.CultureInvariant | RegexOptions.Compiled );
                            Assert.That( xs, Is.EqualTo( es ) );
                        }
                        ++i;
                    }
                }
            }
        }

    }
}
