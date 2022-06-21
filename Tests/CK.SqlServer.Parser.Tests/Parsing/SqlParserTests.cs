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
using Microsoft.Data.SqlClient;


namespace CK.SqlServer.Parser.Tests
{
    [TestFixture]
    public class SqlParserTests
    {
        [TestCase( "typedView.sql", typeof( ISqlServerView ), true )]
        [TestCase( "typedprocedure.sql", typeof( ISqlServerStoredProcedure ), false )]
        [TestCase( "typedInlineTableFunction.sql", typeof( ISqlServerFunctionInlineTable ), false )]
        [TestCase( "typedScalarFunction.sql", typeof( ISqlServerFunctionScalar ), true )]
        [TestCase( "typedTableFunction.sql", typeof( ISqlServerFunctionTable ), false )]
        [TestCase( "typedScript.sql", typeof( ISqlServerScript ), false )]
        [TestCase( "typedTransformer.sql", typeof( ISqlServerTransformer ), false )]
        public void SqlServerParser_Parse_detects_type( string name, Type expectedType, bool schemaBinding )
        {
            string text = TestHelper.LoadTextFromParsingScripts( name );
            var result = new SqlServerParser().Parse( text );
            Assert.That( result.IsError, Is.False );
            Assert.That( result.Result, Is.InstanceOf( expectedType ) );
            ISqlServerObject oSql = result.Result as ISqlServerObject;
            if( oSql != null )
            {
                Assert.That( oSql.Options.SchemaBinding, Is.EqualTo( schemaBinding ) );
            }
        }

        [TestCase( "create view simple as select 1;", null )]
        [TestCase( "create view simple( C ) as select 1;", "C" )]
        [TestCase( "create view simple() as select 1;", "THIS IS AN ERROR" )]
        [TestCase( "create view simple( C1, C2 ) as select 1, 2;", "C1,C2" )]
        [TestCase( "create view simple( [ a ], [a * b] ) as select 1, 2;", "[ a ],[a * b]" )]
        public void getting_formal_column_list( string text, string columns )
        {
            var result = new SqlServerParser().ParseView( text );
            if( columns == "THIS IS AN ERROR" )
            {
                Assert.That( result.IsError );
            }
            else
            {
                Assert.That( result.IsError, Is.False );
                Assert.That( (result.Result.FormalColumnList == null && columns == null)
                                || columns.Split( ',' ).SequenceEqual( result.Result.FormalColumnList ) );
            }
        }


        [TestCase( "create view simple as select 1;", null, "simple", "simple" )]
        [TestCase( "create view [d].[a] as select 1;", "d", "a", "d.a" )]
        [TestCase( "create view [ [3]] nimp].[*µ ù%'B' m] as select 1;", " [3] nimp", "*µ ù%'B' m", "[ [3]] nimp].[*µ ù%'B' m]" )]
        [TestCase( @"
create view 
[on
multiples... 
lines]

.

[

nimp!] as select 1;", @"on
multiples... 
lines", @"

nimp!", @"[on
multiples... 
lines].[

nimp!]" )]
        public void check_schema_and_name( string text, string schema, string name, string schemaName )
        {
            var result = new SqlServerParser().ParseView( text );
            Assert.That( result.IsError, Is.False );
            Assert.That( result.Result.Name, Is.EqualTo( name ) );
            Assert.That( result.Result.Schema, Is.EqualTo( schema ) );
            Assert.That( result.Result.SchemaName, Is.EqualTo( schemaName ) );
        }

    }
}
