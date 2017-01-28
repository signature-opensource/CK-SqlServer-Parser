using CK.SqlServer.Parser;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Transform.Tests.Transform
{
    [TestFixture]
    public class SimpleTransformerParseTests
    {
        [TestCase( "create transformer as begin end", null, null )]
        [TestCase( "create transformer a_schema.a_name as begin end", "a_schema.a_name", null )]
        [TestCase( "create transformer on CK/*ignored*/./**/sText as begin end", null, "CK.sText" )]
        [TestCase( "create transformer theName on CK.sText as begin end", "theName", "CK.sText" )]
        public void empty_transformer_are_valid( string text, string name, string targetFullName )
        {
            var r = new SqlServerParser().Parse( text );
            Assert.That( r.IsError, Is.False );
            Assert.That( r.Result, Is.Not.Null );
            Assert.That( r.Result, Is.InstanceOf<SqlTransformer>() );
            var t = (SqlTransformer)r.Result;
            Assert.That( t.FullName?.ToStringHyperCompact(), Is.EqualTo( name ) );
            Assert.That( t.TargetSchemaName, Is.EqualTo( targetFullName ) );
            Assert.That( t.Body.Count, Is.EqualTo( 0 ) );
        }

        [TestCase( "create transformer as begin add parameter @Added int = null output after @First; end;", null, "@First" )]
        [TestCase( "create transformer as begin add parameter @Added int before @Last; end;", "@Last", null )]
        public void transformer_with_add_parameter( string text, string beforeName, string afterName )
        {
            var t = (SqlTransformer)new SqlServerParser().Parse( text ).Result;
            Assert.That( t.Body.Count, Is.EqualTo( 1 ) );
            SqlTAddParameter a = (SqlTAddParameter)t.Body[0];
            Assert.That( a.Parameters.Count, Is.EqualTo( 1 ) );
            Assert.That( a.Parameters[0].Name, Is.EqualTo( "@Added" ) );
            Assert.That( a.Parameters[0].Variable.TypeDecl.DbType, Is.EqualTo( SqlDbType.Int ) );

            if( afterName != null )
            {
                Assert.That( a.AfterOrBeforeT.TokenType, Is.EqualTo( SqlTokenType.After ) );
                Assert.That( a.ParameterName.Name, Is.EqualTo( afterName ) );
            }
            if( beforeName != null )
            {
                Assert.That( a.AfterOrBeforeT.TokenType, Is.EqualTo( SqlTokenType.Before ) );
                Assert.That( a.ParameterName.Name, Is.EqualTo( beforeName ) );
            }
        }

    }
}
