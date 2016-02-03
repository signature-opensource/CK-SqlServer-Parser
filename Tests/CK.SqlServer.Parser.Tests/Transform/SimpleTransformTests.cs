using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Parser.Tests.Transform
{
    [TestFixture]
    public class SimpleTransformTests
    {
        [TestCase( "procedure test( @i int ) as begin select 0; end" )]
        [TestCase( "function fTest( @i int ) returns int begin return 0; end" )]
        [TestCase( "function fTestMultiStatement( @i int ) returns @T table (Id int) begin return; end" )]
        [TestCase( "function fTestITVF( @i int ) returns table return select 1;" )]
        public void alter_or_create_can_be_toggled( string text )
        {
            ISqlServerObject sqlObject;
            ISqlServerParserError r = new SqlAnalyser( "create " + text ).ParseStatement( out sqlObject );
            Assert.That( !r.IsError );
            ISqlServerAlterOrCreateStatement st = sqlObject as ISqlServerAlterOrCreateStatement;
            Assert.That( st, Is.Not.Null );
            Assert.That( st.IsAlterKeyword, Is.False );
            ISqlServerAlterOrCreateStatement stA = st.ToggleAlterKeyword();
            Assert.That( stA.IsAlterKeyword, Is.True );
            string alterV = stA.ToFullString();
            Assert.That( alterV, Is.EqualTo( "alter " + text ) );

            ISqlServerParserError r2 = new SqlAnalyser( alterV ).ParseStatement( out sqlObject );
            Assert.That( !r2.IsError );
            ISqlServerAlterOrCreateStatement st2 = sqlObject as ISqlServerAlterOrCreateStatement;
            string alter2V = st2.ToFullString();
            Assert.That( alter2V, Is.EqualTo( "alter " + text ) );
            ISqlServerAlterOrCreateStatement stC = st2.ToggleAlterKeyword();
            string alterC = stC.ToFullString();
            Assert.That( alterC, Is.EqualTo( "create " + text ) );
        }

    }

}
