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
        class XmlSqlTesterWithTransform : XmlSqlTester
        {
            public XmlSqlTesterWithTransform( XElement t )
                : base( t )
            {
            }

            protected override ISqlNode OnParsed( ISqlNode e )
            {
                return base.OnParsed( e );
            }
        }


        //[TestCase( "Insert.xml" )]
        public void file_test( string fileName )
        {
            XmlSqlTester.RunAllTests( fileName, e => new XmlSqlTester( e ) );
        }

    }
}
