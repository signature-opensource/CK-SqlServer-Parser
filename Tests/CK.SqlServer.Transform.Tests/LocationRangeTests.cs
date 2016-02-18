using CK.Core;
using CK.SqlServer.Parser;
using CK.SqlServer.UtilTests;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Transform.Tests
{
    [TestFixture]
    public class LocationRangeTests
    {
        [TestCase( "select 1;", "[0,3[" )]
        [TestCase( "break; select 1;", "[2,5[" )]
        [TestCase( "select 1; break; select 2, yo;", "[0,3[, [5,10[" )]
        public void simple_ScopePredicate_on_select_specification( string text, string result )
        {
            SqlNodeScopePredicate p = new SqlNodeScopePredicate( n => n is SelectSpec );
            SqlNodeTransformer t = new SqlNodeTransformer( new SqlAnalyser( text ).Parse() );
            Assert.That( t.BuildRange( p ).ToString(), Is.EqualTo( result ) );
        }


        [TestCase( "select 1; yo;", "∅" )]
        [TestCase( "yo; select 1, yo;", "[5,6[" )]
        [TestCase( "select 1, yo; select yo, 2; yo;", "[3,4[, [6,7[" )]
        public void range_intersection_between_select_specification_and_yo( string text, string result )
        {
            SqlNodeScopePredicate pS = new SqlNodeScopePredicate( n => n is SelectSpec );
            SqlNodeScopePredicate pY = new SqlNodeScopePredicate( n => n.IsToken( SqlTokenType.IdentifierStandard ) && n.ToString() == "yo" );
            SqlNodeScopeIntersect p = new SqlNodeScopeIntersect( pS, pY );
            SqlNodeTransformer t = new SqlNodeTransformer( new SqlAnalyser( text ).Parse() );
            Assert.That( t.BuildRange( p ).ToString(), Is.EqualTo( result ) );
        }


    }
}
