using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using CK.SqlServer.UtilTests;

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
            Assert.That( e.Variable.Name, Is.EqualTo( "@v" ) );
            Assert.That( e.Value, Is.InstanceOf<SqlTokenLiteralInteger>() );
            Assert.That( ((SqlTokenLiteralInteger)e.Value).LiteralValue, Is.EqualTo( "5" ) );
        }

        [TestCase( "set transaction isolation level" )]
        [TestCase( "set transaction isolation level;" )]
        [TestCase( "set transaction isolation level; select 1;" )]
        [TestCase( "set transaction isolation level select 1;" )]
        public void setting_an_option_is_an_unmodelled( string text )
        {
            var e = TestHelper.ParseOneStatement<SqlSetOption>( text );
            Assert.That( e.SetT.Name, Is.EqualTo( "set" ) );
            Assert.That( e.Options.ToString(), Is.EqualTo( "transaction isolation level" ) );
        }

        [Test]
        public void setting_with_syntax_error()
        {
            SqlSetVariable e;
            SqlAnalyser.ErrorResult r = new SqlAnalyser( "set @v = (select 1==0);" ).ParseStatement( out e );
            Assert.That( r.IsError );
            Assert.That( r.ErrorMessage, Does.StartWith( "¤Error: Expected expression" ) );
        }
    }
}
