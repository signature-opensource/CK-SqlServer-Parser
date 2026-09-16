using CK.SqlServer.UtilTests;
using Shouldly;
using NUnit.Framework;
using static CK.Testing.SqlTransformTestHelper;

namespace CK.SqlServer.Parser.Tests;

[TestFixture]
public class SetStatementTests
{
    [TestCase( "set @v = 5" )]
    [TestCase( "set @v = 5;" )]
    [TestCase( "set @v = 5; select 1" )]
    [TestCase( "set @v = 5 declare @i" )]
    public void setting_a_simple_variable( string text )
    {
        var e = TestHelper.ParseOneStatement<SqlSetVariable>( text );
        e.Variable.Name.ShouldBe( "@v" );
        e.Value.ShouldBeAssignableTo<SqlTokenLiteralInteger>().ShouldNotBeNull();
        e.Value.ShouldBeOfType<SqlTokenLiteralInteger>().LiteralValue.ShouldBe( "5" );
    }

    [TestCase( "set transaction isolation level" )]
    [TestCase( "set transaction isolation level;" )]
    [TestCase( "set transaction isolation level; select 1;" )]
    [TestCase( "set transaction isolation level select 1;" )]
    public void setting_an_option_is_an_unmodelled( string text )
    {
        var e = TestHelper.ParseOneStatement<SqlSetOption>( text );
        e.SetT.Name.ShouldBe( "set" );
        e.Options.ToString().ShouldBe( "transaction isolation level" );
    }

    [Test]
    public void setting_with_syntax_error()
    {
        SqlSetVariable e;
        SqlAnalyser.ErrorResult r = new SqlAnalyser( "set @v = (select 1==0);" ).ParseStatement( out e );
        r.IsError.ShouldBeTrue();
        r.ErrorMessage.ShouldStartWith( "¤Error: Expected expression" );
    }
}
