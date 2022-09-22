using CK.SqlServer.UtilTests;
using FluentAssertions;
using NUnit.Framework;
using static CK.Testing.SqlTransformTestHelper;

namespace CK.SqlServer.Parser.Tests
{
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
            e.Variable.Name.Should().Be( "@v" );
            e.Value.Should().BeAssignableTo<SqlTokenLiteralInteger>();
            e.Value.Should().BeOfType< SqlTokenLiteralInteger>().Subject.LiteralValue.Should().Be( "5" );
        }

        [TestCase( "set transaction isolation level" )]
        [TestCase( "set transaction isolation level;" )]
        [TestCase( "set transaction isolation level; select 1;" )]
        [TestCase( "set transaction isolation level select 1;" )]
        public void setting_an_option_is_an_unmodelled( string text )
        {
            var e = TestHelper.ParseOneStatement<SqlSetOption>( text );
            e.SetT.Name.Should().Be( "set" );
            e.Options.ToString().Should().Be( "transaction isolation level" );
        }

        [Test]
        public void setting_with_syntax_error()
        {
            SqlSetVariable e;
            SqlAnalyser.ErrorResult r = new SqlAnalyser( "set @v = (select 1==0);" ).ParseStatement( out e );
            r.IsError.Should().BeTrue();
            r.ErrorMessage.Should().StartWith( "¤Error: Expected expression" );
        }
    }
}
