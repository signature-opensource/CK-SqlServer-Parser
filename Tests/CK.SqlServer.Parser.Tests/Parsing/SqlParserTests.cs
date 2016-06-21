using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NUnit.Framework;
using System.Xml.Linq;
using CK.Core;
using CK.SqlServer.UtilTests;
using System.Data.SqlClient;
using CK.Text;

namespace CK.SqlServer.Parser.Tests
{
    [TestFixture]
    public class SqlParserTests
    {
        [TestCase( "typedView.sql", typeof( ISqlServerView ) )]
        [TestCase( "typedprocedure.sql", typeof( ISqlServerStoredProcedure ) )]
        [TestCase( "typedInlineTableFunction.sql", typeof( ISqlServerFunctionInlineTable ) )]
        [TestCase( "typedScalarFunction.sql", typeof( ISqlServerFunctionScalar ) )]
        [TestCase( "typedTableFunction.sql", typeof( ISqlServerFunctionTable ) )]
        [TestCase( "typedScript.sql", typeof( ISqlServerScript ) )]
        [TestCase( "typedTransformer.sql", typeof( ISqlServerTransformer ) )]
        public void SqlServerParser_Parse_detects_type( string name, Type expectedType )
        {
            string text = TestHelper.LoadTextFromParsingScripts( name );
            var result = new SqlServerParser().Parse( text );
            Assert.That( result.IsError, Is.False );
            Assert.That( result.Result, Is.InstanceOf( expectedType ) );
        }

    }
}
