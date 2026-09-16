using CK.SqlServer.Parser;
using NUnit.Framework;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Transform.Tests.Transform;

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
        r.IsError.ShouldBeFalse();
        r.Result.ShouldNotBeNull();
        r.Result.ShouldBeAssignableTo<SqlTransformer>();
        var t = (SqlTransformer)r.Result;
        (t.FullName?.ToStringHyperCompact()).ShouldBe( name );
        t.TargetSchemaName.ShouldBe( targetFullName );
        t.Body.Count.ShouldBe( 0 );
    }

    [TestCase( "create transformer as begin add parameter @Added int = null output after @First; end;", null, "@First" )]
    [TestCase( "create transformer as begin add parameter @Added int before @Last; end;", "@Last", null )]
    public void transformer_with_add_parameter( string text, string beforeName, string afterName )
    {
        var t = (SqlTransformer)new SqlServerParser().Parse( text ).Result;
        t.Body.Count.ShouldBe( 1 );
        SqlTAddParameter a = (SqlTAddParameter)t.Body[0];
        a.Parameters.Count.ShouldBe( 1 );
        a.Parameters[0].Name.ShouldBe( "@Added" );
        a.Parameters[0].Variable.TypeDecl.DbType.ShouldBe( SqlDbType.Int );

        if( afterName != null )
        {
            a.AfterOrBeforeT.TokenType.ShouldBe( SqlTokenType.After );
            a.ParameterName.Name.ShouldBe( afterName );
        }
        if( beforeName != null )
        {
            a.AfterOrBeforeT.TokenType.ShouldBe( SqlTokenType.Before );
            a.ParameterName.Name.ShouldBe( beforeName );
        }
    }

}
