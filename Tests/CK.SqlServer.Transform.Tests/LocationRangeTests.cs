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

        [Test]
        public void Basic_range_intersect_union_except_operations()
        {
            SqlTransformHost t = new SqlTransformHost( new SqlAnalyser( "select A, B from T where 1 = 0;" ).Parse(), TestHelper.ConsoleMonitor );

            List<SqlNodeLocationRange> all = new List<SqlNodeLocationRange>();
            Dictionary<string, SqlNodeLocationRange> s = new Dictionary<string, SqlNodeLocationRange>();
            Func<string,ISqlNodeLocationRange,SqlNodeLocationRange> add = ( check, range ) => 
            {
                Assert.That( range != null );
                Assert.That( range is SqlNodeLocationRange );
                Assert.That( range.ToString(), Is.EqualTo( check ) );
                all.Add( (SqlNodeLocationRange)range);
                s.Add( check, (SqlNodeLocationRange)range );
                return (SqlNodeLocationRange)range;
            };

            var r0 = add( "∅", t.BuildRange( new SqlNodeScopeBreadthPredicate( n => false ) ) );
            var r = add( "[0,11[", t.BuildRange( new SqlNodeScopeBreadthPredicate( n => true ) ) );
            var r1 = add( "[0,10[", t.BuildRange( new SqlNodeScopeBreadthPredicate( n => n is SelectSpec ) ) );
            var r2 = add( "[10,11[", t.BuildRange( new SqlNodeScopeBreadthPredicate( n => n.IsToken( SqlTokenType.SemiColon ) ) ) );
            var r11 = add( "[0,1[", t.BuildRange( new SqlNodeScopeBreadthPredicate( n => n.IsToken( SqlTokenType.Select ) ) ) );
            var r12 = add( "[1,4[", t.BuildRange( new SqlNodeScopeBreadthPredicate( n => n is SelectColumnList ) ) );
            var r121 = add( "[1,2[", t.BuildRange( new SqlNodeScopeBreadthPredicate( n => n.ToString() == "A" ) ) );
            var r122 = add( "[2,3[", t.BuildRange( new SqlNodeScopeBreadthPredicate( n => n.IsToken( SqlTokenType.Comma ) ) ) );
            var r123 = add( "[3,4[", t.BuildRange( new SqlNodeScopeBreadthPredicate( n => n.ToString() == "B" ) ) );
            var r13 = add( "[4,6[", t.BuildRange( new SqlNodeScopeBreadthPredicate( n => n is SelectFrom ) ) );
            var r131 = add( "[4,5[", t.BuildRange( new SqlNodeScopeBreadthPredicate( n => n.IsToken( SqlTokenType.From ) ) ) );
            var r132 = add( "[5,6[", t.BuildRange( new SqlNodeScopeBreadthPredicate( n => n.ToString() == "T" ) ) );
            var r14 = add( "[6,7[", t.BuildRange( new SqlNodeScopeBreadthPredicate( n => n.ToString() == "where" ) ) );
            var r15 = add( "[7,10[", t.BuildRange( new SqlNodeScopeBreadthPredicate( n => n is SqlBinaryOperator ) ) );
            var r151 = add( "[7,8[", t.BuildRange( new SqlNodeScopeBreadthPredicate( n => n.ToString() == "1" ) ) );
            var r152 = add( "[8,9[", t.BuildRange( new SqlNodeScopeBreadthPredicate( n => n.IsToken( SqlTokenType.Equal ) ) ) );
            var r153 = add( "[9,10[", t.BuildRange( new SqlNodeScopeBreadthPredicate( n => n.ToString() == "0" ) ) );
            var rEnd = add( "[7,11[", r15.Union( r2 ) );
            var rFront = add( "[0,9[", r1.Except( r153 ) );
            var rMid1 = add( "[1,9[", rFront.Except( r11 ) );
            var rMid2 = add( "[4,9[", rMid1.Except( r12 ) );

            Assert.That( all.Count, Is.EqualTo( 21 ) );

            Assert.That( all.All( x => r.Intersect( x ).ToString() == x.ToString() ) );
            Assert.That( all.All( x => x.Intersect( r ).ToString() == x.ToString() ) );
            Assert.That( all.All( x => x.Intersect( r0 ).ToString() == r0.ToString() ) );
            Assert.That( all.All( x => r.Union( x ).ToString() == r.ToString() ) );
            Assert.That( all.All( x => x.Union( r ).ToString() == r.ToString() ) );
            Assert.That( all.All( x => x.Union( r0 ).ToString() == x.ToString() ) );
            Assert.That( all.All( x => x.Except( r ).ToString() == r0.ToString() ) );
            Assert.That( all.All( x => x.Except( x ).ToString() == r0.ToString() ) );

            Action<string, string, string> except = ( left, right, result ) =>
              {
                  SqlNodeLocationRange rL = s[left];
                  SqlNodeLocationRange rR = s[right];

                  Assert.That( rL.Except( rR ).ToString(), Is.EqualTo( result ) );
                  Assert.That( ((ISqlNodeLocationRange)rL).Except( rR ).ToString(), Is.EqualTo( result ) );
              };
            except( "[0,11[", "[0,11[", "∅" );
            except( "[0,11[", "[0,10[", "[10,11[" );
            except( "[0,11[", "[0,1[", "[1,11[" );
            except( "[0,11[", "[7,10[", "[0,7[-[10,11[" );
            except( "[0,11[", "[10,11[", "[0,10[" );
            except( "[7,10[", "[0,11[", "∅" );
            except( "[0,11[", "[7,11[", "[0,7[" );
            except( "[1,9[", "[7,11[", "[1,7[" );
            except( "[7,11[", "[1,9[", "[9,11[" );
            except( "[7,10[", "[9,10[", "[7,9[" );
            except( "[7,10[", "[8,9[", "[7,8[-[9,10[" );

            var rM124 = s["[0,1["].Union( s["[2,3["] ).Union( s["[4,5["] );
            Assert.That( rM124.ToString(), Is.EqualTo( "[0,1[-[2,3[-[4,5[" ) );
            Assert.That( rM124.Except( s["[1,2["] ).ToString(), Is.EqualTo( "[0,1[-[2,3[-[4,5[" ) );
            Assert.That( rM124.Except( s["[1,2["].Union( s["[2,3["] ) ).ToString(), Is.EqualTo( "[0,1[-[4,5[" ) );
            Assert.That( rM124.Except( s["[1,2["].Union( s["[2,3["] ).Union( s["[4,5["] ) ).ToString(), Is.EqualTo( "[0,1[" ) );
            Assert.That( rM124.Except( s["[0,1["].Union( s["[4,6["] ) ).ToString(), Is.EqualTo( "[2,3[" ) );
            Assert.That( rM124.Except( s["[0,1["].Union( s["[1,2["] ) ).ToString(), Is.EqualTo( "[2,3[-[4,5[" ) );
            Assert.That( rM124.Except( s["[0,1["].Union( s["[1,2["] ).Union( s["[2,3["] ) ).ToString(), Is.EqualTo( "[4,5[" ) );
            Assert.That( rM124.Except( s["[0,1["].Union( s["[1,2["] ).Union( s["[2,3["] ).Union( s["[3,4["] ) ).ToString(), Is.EqualTo( "[4,5[" ) );
        }

        [TestCase( "break;", "∅" )]
        [TestCase( "select 1;", "[0,2[" )]
        [TestCase( "break; select 1;", "[2,4[" )]
        [TestCase( "select 1; break; select 2, yo;", "[0,2[-[5,9[" )]
        [TestCase( "select 1; select 2", "[0,2[-[3,5[" )]
        public void simple_ScopePredicate_on_select_specification( string text, string result )
        {
            var p = new SqlNodeScopeBreadthPredicate( n => n is SelectSpec );
            var t = new SqlTransformHost( new SqlAnalyser( text ).Parse(), TestHelper.ConsoleMonitor );
            Assert.That( t.BuildRange( p ).ToString(), Is.EqualTo( result ) );
        }

        [TestCase( "select 1; yo;", "∅" )]
        [TestCase( "yo; select 1, yo;", "[5,6[" )]
        [TestCase( "select 1, yo; select yo, 2; yo;", "[3,4[-[6,7[" )]
        public void range_intersection_between_select_specification_and_yo( string text, string result )
        {
            var pS = new SqlNodeScopeBreadthPredicate( n => n is SelectSpec );
            var pY = new SqlNodeScopeBreadthPredicate( n => n.IsToken( SqlTokenType.IdentifierStandard ) && n.ToString() == "yo" );
            ISqlNode node = new SqlAnalyser( text ).Parse();
            var t = new SqlTransformHost( node, TestHelper.ConsoleMonitor );

            var p = new SqlNodeScopeIntersect( pS, pY );
            Assert.That( t.BuildRange( p ).ToString(), Is.EqualTo( result ) );

            var pI = new SqlNodeScopeIntersect( pY, pS );
            Assert.That( t.BuildRange( pI ).ToString(), Is.EqualTo( result ) );
        }


        [TestCase( "yo", "[0,1[" )]
        [TestCase( "break; yotinue;", "∅" )]
        [TestCase( "yo; select 1; yo;", "[0,1[-[2,4[-[5,6[" )]
        [TestCase( "yo; select 1, yo;", "[0,1[-[2,6[" )]
        [TestCase( "select 1, yo; select yo, 2; yo;", "[0,4[-[5,9[-[10,11[" )]
        public void range_union_between_select_specification_and_yo( string text, string result )
        {
            var pS = new SqlNodeScopeBreadthPredicate( n => n is SelectSpec );
            var pY = new SqlNodeScopeBreadthPredicate( n => n.IsToken( SqlTokenType.IdentifierStandard ) && n.ToString() == "yo" );
            ISqlNode node = new SqlAnalyser( text ).Parse();
            var t = new SqlTransformHost( node, TestHelper.ConsoleMonitor );

            SqlNodeScopeUnion p = new SqlNodeScopeUnion( pS, pY );
            Assert.That( t.BuildRange( p ).ToString(), Is.EqualTo( result ) );

            SqlNodeScopeUnion pI = new SqlNodeScopeUnion( pY, pS );
            Assert.That( t.BuildRange( pI ).ToString(), Is.EqualTo( result ) );
        }

        [TestCase( true )]
        [TestCase( false )]
        public void depth_versus_breadth_node_predicate( bool useQualifiedLocationNodeBuilder )
        {
            string text = @"select * from (select * from (select * from sys.tables) t) t";
            ISqlNode node = new SqlAnalyser( text ).Parse();
            var t = new SqlTransformHost( node, TestHelper.ConsoleMonitor ) { BuildQualifiedNodeLocations = useQualifiedLocationNodeBuilder };

            var pD = new SqlNodeScopeDepthPredicate( n => n.AllTokens.FirstOrDefault()?.TokenType == SqlTokenType.Select, false );
            var rD = t.BuildRange( pD );
            Assert.That( rD.ToString(), Is.EqualTo( "[0,1[-[4,5[-[8,9[" ) );

            var pB = new SqlNodeScopeBreadthPredicate( n => n.AllTokens.FirstOrDefault()?.TokenType == SqlTokenType.Select );
            var rB = t.BuildRange( pB );
            Assert.That( rB.ToString(), Is.EqualTo( "[0,18[" ) );
        }

        class TriviaInjecter : SqlNodeLocationVisitor
        {
            protected override ISqlNode AfterVisitItem( ISqlNode e )
            {
                if( VisitContext.RangeFilterStatus.IsIncludedInFilteredRange() )
                    return e.SetTrivias( e.LeadingTrivias.Add( new SqlTrivia( SqlTokenType.None, $"[<{e.GetType().Name}>" ) ), e.TrailingTrivias.Insert( 0, new SqlTrivia( SqlTokenType.None, "]" ) ) );
                return e;
            }
        }

        [TestCase( true, "A", "[0,1[", "[<SqlTokenIdentifier>A] B C" )]
        [TestCase( false, "A", "[0,1[", "[<SqlTokenIdentifier>A] B C" )]
        [TestCase( true, "B", "[1,2[", "A [<SqlTokenIdentifier>B] C" )]
        [TestCase( false, "B", "[1,2[", "A [<SqlTokenIdentifier>B] C" )]
        [TestCase( true, "C", "[2,3[", "A B [<SqlTokenIdentifier>C]" )]
        [TestCase( false, "C", "[2,3[", "A B [<SqlTokenIdentifier>C]" )]
        public void range_trivia_injecter( bool useQualifiedLocationNodeBuilder, string item, string range, string result )
        {
            string text = @"A B C";
            ISqlNode node = new SqlAnalyser( text ).Parse();
            var t = new SqlTransformHost( node, TestHelper.ConsoleMonitor ) { BuildQualifiedNodeLocations = useQualifiedLocationNodeBuilder };

            var pA = new SqlNodeScopeDepthPredicate( n => n.ToString() == item );
            var rA = t.BuildRange( pA );
            Assert.That( rA.ToString(), Is.EqualTo( range ) );

            Assert.That( t.Visit( new TriviaInjecter(), rA ) );
            Assert.That( t.Node.ToString( true, true ), Is.EqualTo( result ) );
        }

    }
}
