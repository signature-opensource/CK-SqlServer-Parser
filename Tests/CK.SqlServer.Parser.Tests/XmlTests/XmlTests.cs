using CK.Core;
using CK.SqlServer.UtilTests;
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
        [TestCase( "Between expressions.xml" )]
        [TestCase( "Big Procedures.xml" )]
        [TestCase( "Comma lists.xml" )]
        [TestCase( "CTE.xml" )]
        [TestCase( "Cursors.xml" )]
        [TestCase( "Delete.xml" )]
        [TestCase( "Functions.xml" )]
        [TestCase( "GrantDenyRevoke.xml" )]
        [TestCase( "Identifiers.xml" )]
        [TestCase( "If.xml" )]
        [TestCase( "Insert.xml" )]
        [TestCase( "IsNull.xml" )]
        [TestCase( "LiteralTokens.xml" )]
        [TestCase( "Logical operators.xml" )]
        [TestCase( "Merge.xml" )]
        [TestCase( "Multi Statements.xml" )]
        [TestCase( "Not so Simple Procedures.xml" )]
        [TestCase( "OpenXml.xml" )]
        [TestCase( "Sequence.xml" )]
        [TestCase( "Simple expressions.xml" )]
        [TestCase( "Simple Procedures.xml" )]
        [TestCase( "Simple Selects.xml" )]
        [TestCase( "Trigger.xml" )]
        [TestCase( "TryCatch.xml" )]
        [TestCase( "Unmodeled stuff.xml" )]
        [TestCase( "Update.xml" )]
        public void file_test( string fileName )
        {
            XmlSqlTester.RunAllTests( fileName, e => new XmlSqlTester( e ) );
        }

    }
}
