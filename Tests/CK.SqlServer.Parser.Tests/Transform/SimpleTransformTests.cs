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
        public void ToggleAlterOrCreate( string text )
        {
            ISqlServerObject sqlObject;
            ISqlServerParserError r = SqlAnalyser.ParseStatement( out sqlObject, "create " + text );
            Assert.That( !r.IsError );
            ISqlServerAlterOrCreateStatement st = sqlObject as ISqlServerAlterOrCreateStatement;
            Assert.That( st, Is.Not.Null );
            Assert.That( st.IsAlterKeyword, Is.False );
            ISqlServerAlterOrCreateStatement stA = st.ToggleAlterKeyword();
            Assert.That( stA.IsAlterKeyword, Is.True );
            string alterV = stA.ToFullString();
            Assert.That( alterV, Is.EqualTo( "alter " + text ) );

            ISqlServerParserError r2 = SqlAnalyser.ParseStatement( out sqlObject, alterV );
            Assert.That( !r2.IsError );
            ISqlServerAlterOrCreateStatement st2 = sqlObject as ISqlServerAlterOrCreateStatement;
            string alter2V = st2.ToFullString();
            Assert.That( alter2V, Is.EqualTo( "alter " + text ) );
            ISqlServerAlterOrCreateStatement stC = st2.ToggleAlterKeyword();
            string alterC = stC.ToFullString();
            Assert.That( alterC, Is.EqualTo( "create " + text ) );
        }
    }

    static class SqlServerExtension
    {
        static public string ToFullString( this ISqlServerObject @this )
        {
            StringBuilder b = new StringBuilder();
            @this.Write( b );
            return b.ToString();
        }
    }
}
