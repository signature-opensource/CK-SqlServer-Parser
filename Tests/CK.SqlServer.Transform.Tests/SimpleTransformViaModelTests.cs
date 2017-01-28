using CK.SqlServer.Parser;
using CK.SqlServer.UtilTests;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Transform.Tests.Transform
{
    [TestFixture]
    public class SimpleTransformViaModelTests
    {
        [TestCase( "procedure test( @i int ) as begin select 0; end" )]
        [TestCase( "function fTest( @i int ) returns int begin return 0; end" )]
        [TestCase( "function fTestMultiStatement( @i int ) returns @T table (Id int) begin return; end" )]
        [TestCase( "function fTestITVF( @i int ) returns table return select 1;" )]
        public void alter_or_create_can_be_toggled( string text )
        {
            ISqlServerObject sqlObject;
            var r = new SqlAnalyser( "create " + text ).ParseStatement( out sqlObject );
            Assert.That( !r.IsError );
            ISqlServerAlterOrCreateStatement st = sqlObject as ISqlServerAlterOrCreateStatement;
            Assert.That( st, Is.Not.Null );
            Assert.That( st.IsAlterKeyword, Is.False );
            ISqlServerAlterOrCreateStatement stA = st.ToggleAlterKeyword();
            Assert.That( stA.IsAlterKeyword, Is.True );
            string alterV = stA.ToFullString();
            Assert.That( alterV, Is.EqualTo( "alter " + text ) );

            var r2 = new SqlAnalyser( alterV ).ParseStatement( out sqlObject );
            Assert.That( !r2.IsError );
            ISqlServerAlterOrCreateStatement st2 = sqlObject as ISqlServerAlterOrCreateStatement;
            string alter2V = st2.ToFullString();
            Assert.That( alter2V, Is.EqualTo( "alter " + text ) );
            ISqlServerAlterOrCreateStatement stC = st2.ToggleAlterKeyword();
            string alterC = stC.ToFullString();
            Assert.That( alterC, Is.EqualTo( "create " + text ) );
        }

        [TestCase( "create procedure X.test( @i int ) as begin select 0; end", " $ ", "create procedure [ $ ].test" )]
        [TestCase( "alter function fTest( @i int ) returns int begin return 0; end", "PPP", "alter function PPP.fTest" )]
        [TestCase( "create function /*c1*/[X]/*c2*/./*c3*/fTestMultiStatement( @i int ) returns @T table (Id int) begin return; end", "S", "create function /*c1*/S/*c2*/./*c3*/fTestMultiStatement" )]
        [TestCase( "alter function [a schema].fTestITVF( @i int ) returns table return select 1;", null, "alter function fTestITVF" )]
        public void schema_can_be_set( string text, string schema, string resultStart )
        {
            ISqlServerObject sqlObject;
            var r = new SqlAnalyser( text ).ParseStatement( out sqlObject );
            Assert.That( !r.IsError );
            ISqlServerObject o2 = sqlObject.SetSchema( schema );
            Assert.That( o2.ToFullString(), Does.StartWith( resultStart ) );
        }

        [TestCase( "One", 1, "Two", "Two" )]
        [TestCase( "One", 2, "Two", "Two.One" )]
        [TestCase( "One", 3, "Two", "ArgumentException" )]
        [TestCase( "One.Two", 1, "[3]", "One.[[3]]]" )]
        [TestCase( "One.Two", 2, "[3]", "[[3]]].Two" )]
        [TestCase( "One.Two", 3, "[3]", "[[3]]].One.Two" )]
        [TestCase( "One.Two", 1, null, "One" )]
        [TestCase( "One.Two", 2, null, "Two" )]
        [TestCase( "One.Two", 3, null, "ArgumentException" )]
        [TestCase( "One", 1, null, "InvalidOperationException" )]
        [TestCase( "One", 2, null, "InvalidOperationException" )]
        [TestCase( "One.Two.Three.Four", 5, null, "ArgumentException" )]
        [TestCase( "One.Two.Three.Four", 4, null, "Two.Three.Four" )]
        [TestCase( "One.Two.Three.Four", 3, null, "One.Three.Four" )]
        [TestCase( "One.Two.Three.Four", 2, null, "One.Two.Four" )]
        [TestCase( "One.Two.Three.Four", 1, null, "One.Two.Three" )]
        [TestCase( "One.Two.Three.Four", 0, null, "ArgumentException" )]
        public void setting_identifier_parts_via_SetPartName( string id, int idxPart, string name, string result )
        {
            ISqlIdentifier t = (ISqlIdentifier)new SqlAnalyser( id ).IsOneExpression( true );
            if( result == "ArgumentException" )
                Assert.Throws<ArgumentException>( () => t.SetPartName( idxPart, name ) );
            else if( result == "InvalidOperationException" )
                Assert.Throws<InvalidOperationException>( () => t.SetPartName( idxPart, name ) );
            else
            {
                var r = t.SetPartName( idxPart, name );
                Assert.That( r.ToString(), Is.EqualTo( result ) );
                if( name != null )
                {
                    Assert.That( r.GetPartName( idxPart ), Is.EqualTo( name ) );
                }
            }
        }


        [TestCase( "select X;", "create transformer as begin replace first range {X} with 'Y'; end", "select Y;" )]
        [TestCase( "set @V = 3;", "create transformer as begin replace first range {3} with '4'; end", "set @V = 4;" )]
        public void using_transform_model_method( string original, string transform, string final )
        {
            SqlServerParser p = new SqlServerParser();
            ISqlServerParsedText o = p.Parse( original ).Result;
            Assert.That( o, Is.Not.Null );
            ISqlServerTransformer t = p.ParseTransformer( transform ).Result;
            Assert.That( t, Is.Not.Null );
            ISqlServerParsedText oT = t.SafeTransform( TestHelper.ConsoleMonitor, o );
            Assert.That( oT, Is.Not.Null );
            Assert.That( oT.ToFullString(), Is.EqualTo( final ) );
        }
    }

}
