using CK.Core;
using CK.SqlServer.Parser;
using CK.SqlServer.UtilTests;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using static CK.Testing.SqlTransformTestHelper;

namespace CK.SqlServer.Transform.Tests
{
    [TestFixture]
    public class LocationRangeTests
    {

        [Test]
        public void Basic_range_intersect_union_except_operations()
        {
            SqlTransformHost t = new SqlTransformHost( new SqlAnalyser( "select A, B from T where 1 = 0;" ).Parse() );

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

            var r0 = add( "∅", t.BuildRange( TestHelper.Monitor, new SqlNodeScopeBreadthPredicate( n => false ) ) );
            var r = add( "[0,11[", t.BuildRange( TestHelper.Monitor, new SqlNodeScopeBreadthPredicate( n => true ) ) );
            var r1 = add( "[0,10[", t.BuildRange( TestHelper.Monitor, new SqlNodeScopeBreadthPredicate( n => n is SelectSpec ) ) );
            var r2 = add( "[10,11[", t.BuildRange( TestHelper.Monitor, new SqlNodeScopeBreadthPredicate( n => n.IsToken( SqlTokenType.SemiColon ) ) ) );
            var r11 = add( "[0,1[", t.BuildRange( TestHelper.Monitor, new SqlNodeScopeBreadthPredicate( n => n.IsToken( SqlTokenType.Select ) ) ) );
            var r12 = add( "[1,4[", t.BuildRange( TestHelper.Monitor, new SqlNodeScopeBreadthPredicate( n => n is SelectColumnList ) ) );
            var r121 = add( "[1,2[", t.BuildRange( TestHelper.Monitor, new SqlNodeScopeBreadthPredicate( n => n.ToString() == "A" ) ) );
            var r122 = add( "[2,3[", t.BuildRange( TestHelper.Monitor, new SqlNodeScopeBreadthPredicate( n => n.IsToken( SqlTokenType.Comma ) ) ) );
            var r123 = add( "[3,4[", t.BuildRange( TestHelper.Monitor, new SqlNodeScopeBreadthPredicate( n => n.ToString() == "B" ) ) );
            var r13 = add( "[4,6[", t.BuildRange( TestHelper.Monitor, new SqlNodeScopeBreadthPredicate( n => n is SelectFrom ) ) );
            var r131 = add( "[4,5[", t.BuildRange( TestHelper.Monitor, new SqlNodeScopeBreadthPredicate( n => n.IsToken( SqlTokenType.From ) ) ) );
            var r132 = add( "[5,6[", t.BuildRange( TestHelper.Monitor, new SqlNodeScopeBreadthPredicate( n => n.ToString() == "T" ) ) );
            var r14 = add( "[6,7[", t.BuildRange( TestHelper.Monitor, new SqlNodeScopeBreadthPredicate( n => n.ToString() == "where" ) ) );
            var r15 = add( "[7,10[", t.BuildRange( TestHelper.Monitor, new SqlNodeScopeBreadthPredicate( n => n is SqlBinaryOperator ) ) );
            var r151 = add( "[7,8[", t.BuildRange( TestHelper.Monitor, new SqlNodeScopeBreadthPredicate( n => n.ToString() == "1" ) ) );
            var r152 = add( "[8,9[", t.BuildRange( TestHelper.Monitor, new SqlNodeScopeBreadthPredicate( n => n.IsToken( SqlTokenType.Equal ) ) ) );
            var r153 = add( "[9,10[", t.BuildRange( TestHelper.Monitor, new SqlNodeScopeBreadthPredicate( n => n.ToString() == "0" ) ) );
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
            var t = new SqlTransformHost( new SqlAnalyser( text ).Parse() );
            Assert.That( t.BuildRange( TestHelper.Monitor, p ).ToString(), Is.EqualTo( result ) );
        }

        [TestCase( "select 1; yo;", "∅" )]
        [TestCase( "yo; select 1, yo;", "[5,6[" )]
        [TestCase( "select 1, yo; select yo, 2; yo;", "[3,4[-[6,7[" )]
        public void range_intersection_between_select_specification_and_yo( string text, string result )
        {
            var pS = new SqlNodeScopeBreadthPredicate( n => n is SelectSpec );
            var pY = new SqlNodeScopeBreadthPredicate( n => n.IsToken( SqlTokenType.IdentifierStandard ) && n.ToString() == "yo" );
            ISqlNode node = new SqlAnalyser( text ).Parse();
            var t = new SqlTransformHost( node );

            var p = new SqlNodeScopeIntersect( pS, pY );
            Assert.That( t.BuildRange( TestHelper.Monitor, p ).ToString(), Is.EqualTo( result ) );

            var pI = new SqlNodeScopeIntersect( pY, pS );
            Assert.That( t.BuildRange( TestHelper.Monitor, pI ).ToString(), Is.EqualTo( result ) );
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
            var t = new SqlTransformHost( node );

            SqlNodeScopeUnion p = new SqlNodeScopeUnion( pS, pY );
            Assert.That( t.BuildRange( TestHelper.Monitor, p ).ToString(), Is.EqualTo( result ) );

            SqlNodeScopeUnion pI = new SqlNodeScopeUnion( pY, pS );
            Assert.That( t.BuildRange( TestHelper.Monitor, pI ).ToString(), Is.EqualTo( result ) );
        }

        [Test]
        public void depth_versus_breadth_node_predicate()
        {
            string text = @"select * from (select * from (select * from sys.tables) t) t";
            ISqlNode node = new SqlAnalyser( text ).Parse();
            var t = new SqlTransformHost( node );

            var pD = new SqlNodeScopeDepthPredicate( n => n.AllTokens.FirstOrDefault()?.TokenType == SqlTokenType.Select, false );
            var rD = t.BuildRange( TestHelper.Monitor, pD );
            Assert.That( rD.ToString(), Is.EqualTo( "[0,1[-[4,5[-[8,9[" ) );

            var pB = new SqlNodeScopeBreadthPredicate( n => n.AllTokens.FirstOrDefault()?.TokenType == SqlTokenType.Select );
            var rB = t.BuildRange( TestHelper.Monitor, pB );
            Assert.That( rB.ToString(), Is.EqualTo( "[0,18[" ) );
        }

        class TriviaInjecter : SqlNodeLocationVisitor
        {
            public TriviaInjecter( IActivityMonitor monitor )
                : base( monitor )
            {
            }

            protected override ISqlNode AfterVisitItem( ISqlNode e )
            {
                if( VisitContext.RangeFilterStatus.IsIncludedInFilteredRange() )
                    return e.SetTrivias( e.LeadingTrivias.Add( new SqlTrivia( SqlTokenType.None, $"[<{e.GetType().Name}>" ) ), e.TrailingTrivias.Insert( 0, new SqlTrivia( SqlTokenType.None, "]" ) ) );
                return e;
            }
        }

        [TestCase( "A", "[0,1[", "[<SqlTokenIdentifier>A] B C" )]
        [TestCase( "B", "[1,2[", "A [<SqlTokenIdentifier>B] C" )]
        [TestCase( "C", "[2,3[", "A B [<SqlTokenIdentifier>C]" )]
        public void range_trivia_injecter( string item, string range, string result )
        {
            string text = @"A B C";
            ISqlNode node = new SqlAnalyser( text ).Parse();
            var t = new SqlTransformHost( node );

            var pA = new SqlNodeScopeDepthPredicate( n => n.ToString() == item );
            var rA = t.BuildRange( TestHelper.Monitor, pA );
            Assert.That( rA.ToString(), Is.EqualTo( range ) );

            Assert.That( t.Visit( new TriviaInjecter( TestHelper.Monitor ), rA ) );
            Assert.That( t.Node.ToString( true, true ), Is.EqualTo( result ) );
        }

        [Test]
        public void extrema_in_action_on_selects()
        {
            string text = @"select * from (select * from (select * from sys.tables) t) t";
            ISqlNode node = new SqlAnalyser( text ).Parse();
            var t = new SqlTransformHost( node );

            var selects = new SqlNodeScopeDepthPredicate( n => n.AllTokens.FirstOrDefault()?.TokenType == SqlTokenType.Select, false );

            var extrema = new SqlNodeScopeExtrema( selects, SqlNodeScopeExtrema.Option.None );
            t.BuildRange( TestHelper.Monitor, extrema ).ToString().Should().Be( "[0,9[" );

            var after = new SqlNodeScopeExtrema( selects, SqlNodeScopeExtrema.Option.After );
            t.BuildRange( TestHelper.Monitor, after ).ToString().Should().Be( "[9,18[" );

            var afterIncluded = new SqlNodeScopeExtrema( selects, SqlNodeScopeExtrema.Option.AfterIncluded );
            t.BuildRange( TestHelper.Monitor, afterIncluded ).ToString().Should().Be( "[0,18[" );

            var before = new SqlNodeScopeExtrema( selects, SqlNodeScopeExtrema.Option.Before );
            t.BuildRange( TestHelper.Monitor, before ).ToString().Should().Be( "∅" );

            var beforeIncluded = new SqlNodeScopeExtrema( selects, SqlNodeScopeExtrema.Option.BeforeIncluded );
            t.BuildRange( TestHelper.Monitor, beforeIncluded ).ToString().Should().Be( "[0,9[" );
        }

        [Test]
        public void extrema_in_action_on_from()
        {
            string text = @"select * from (select * from (select * from sys.tables) t) t";
            ISqlNode node = new SqlAnalyser( text ).Parse();
            var t = new SqlTransformHost( node );

            var froms = new SqlNodeScopeDepthPredicate( n => n.AllTokens.FirstOrDefault()?.TokenType == SqlTokenType.From, false );

            var extrema = new SqlNodeScopeExtrema( froms, SqlNodeScopeExtrema.Option.None );
            t.BuildRange( TestHelper.Monitor, extrema ).ToString().Should().Be( "[2,11[" );

            var after = new SqlNodeScopeExtrema( froms, SqlNodeScopeExtrema.Option.After );
            t.BuildRange( TestHelper.Monitor, after ).ToString().Should().Be( "[11,18[" );

            var afterIncluded = new SqlNodeScopeExtrema( froms, SqlNodeScopeExtrema.Option.AfterIncluded );
            t.BuildRange( TestHelper.Monitor, afterIncluded ).ToString().Should().Be( "[2,18[" );

            var before = new SqlNodeScopeExtrema( froms, SqlNodeScopeExtrema.Option.Before );
            t.BuildRange( TestHelper.Monitor, before ).ToString().Should().Be( "[0,2[" );

            var beforeIncluded = new SqlNodeScopeExtrema( froms, SqlNodeScopeExtrema.Option.BeforeIncluded );
            t.BuildRange( TestHelper.Monitor, beforeIncluded ).ToString().Should().Be( "[0,11[" );
        }

        [Test]
        public void extrema_in_action_on_t()
        {
            string text = @"select * from (select * from (select * from sys.tables) t) t";
            ISqlNode node = new SqlAnalyser( text ).Parse();
            var t = new SqlTransformHost( node );

            var ts = new SqlNodeScopeDepthPredicate( n => n.IsToken(SqlTokenType.IdentifierStandard) && n.ToStringHyperCompact() == "t", false );

            var extrema = new SqlNodeScopeExtrema( ts, SqlNodeScopeExtrema.Option.None );
            t.BuildRange( TestHelper.Monitor, extrema ).ToString().Should().Be( "[15,18[" );

            var after = new SqlNodeScopeExtrema( ts, SqlNodeScopeExtrema.Option.After );
            t.BuildRange( TestHelper.Monitor, after ).ToString().Should().Be( "∅" );

            var afterIncluded = new SqlNodeScopeExtrema( ts, SqlNodeScopeExtrema.Option.AfterIncluded );
            t.BuildRange( TestHelper.Monitor, afterIncluded ).ToString().Should().Be( "[15,18[" );

            var before = new SqlNodeScopeExtrema( ts, SqlNodeScopeExtrema.Option.Before );
            t.BuildRange( TestHelper.Monitor, before ).ToString().Should().Be( "[0,15[" );

            var beforeIncluded = new SqlNodeScopeExtrema( ts, SqlNodeScopeExtrema.Option.BeforeIncluded );
            t.BuildRange( TestHelper.Monitor, beforeIncluded ).ToString().Should().Be( "[0,18[" );
        }


        [Test]
        public void SqlNodeScopeFromTriviaMatcher_on_flat_tokens()
        {
            string text = @"/*0*/A/*1*/B/*2*/C/*3*/";
            var input = new SqlAnalyser( text ).Parse();
            var h = new SqlTransformHost( input );

            var after0 = new SqlNodeScopeFromTriviaMatcher( true, t => t.Text == "0", "After 0" );
            h.BuildRange( TestHelper.Monitor, after0 ).ToString().Should().Be( "[0,1[" );

            var before0 = new SqlNodeScopeFromTriviaMatcher( false, t => t.Text == "0", "Before 0" );
            h.BuildRange( TestHelper.Monitor, before0 ).ToString().Should().Be( "]0[" );

            var bothAAndB = new SqlNodeScopeFromTriviaMatcher( true, t => t.Text == "0" || t.Text == "1", "A and B." );
            h.BuildRange( TestHelper.Monitor, bothAAndB ).ToString().Should().Be( "[0,1[-[1,2[" );

            var allAfter = new SqlNodeScopeFromTriviaMatcher( true, t => true, "AllAfter." );
            h.BuildRange( TestHelper.Monitor, allAfter ).ToString().Should().Be( "[0,1[-[1,2[-[2,3[" );

            var allBefore = new SqlNodeScopeFromTriviaMatcher( false, t => true, "AllBefore." );
            h.BuildRange( TestHelper.Monitor, allBefore ).ToString().Should().Be( "]0[-[0,1[-[1,2[-[2,3[" );

        }

        [Test]
        public void SqlNodeScopeFromTriviaMatcher_on_structure()
        {
            string text = @"/*0*/
                            ( /*1*/
                               A, /*2*/
                               ( /*3*/
                                 B /*4*/
                               ) /*5*/
                            )/*6*/";
            var input = new SqlAnalyser( text ).Parse();
            var h = new SqlTransformHost( input );

            var after0 = new SqlNodeScopeFromTriviaMatcher( true, t => t.Text == "0", "After 0" );
            h.BuildRange( TestHelper.Monitor, after0 ).ToString().Should().Be( "[0,1[" );

            var before6 = new SqlNodeScopeFromTriviaMatcher( false, t => t.Text == "6", "Before 6" );
            h.BuildRange( TestHelper.Monitor, before6 ).ToString().Should().Be( "[6,7[" );

            var after6 = new SqlNodeScopeFromTriviaMatcher( true, t => t.Text == "6", "After 6" );
            h.BuildRange( TestHelper.Monitor, after6 ).ToString().Should().Be( "∅" );

            var allAfter = new SqlNodeScopeFromTriviaMatcher( true, t => true, "AllAfter." );
            h.BuildRange( TestHelper.Monitor, allAfter ).ToString().Should().Be( "[0,1[-[1,2[-[3,4[-[4,5[-[5,6[-[6,7[" );

            var allBefore = new SqlNodeScopeFromTriviaMatcher( false, t => true, "AllBefore." );
            h.BuildRange( TestHelper.Monitor, allBefore ).ToString().Should().Be( "]0[-[0,1[-[2,3[-[3,4[-[4,5[-[5,6[-[6,7[" );

        }

        [Test]
        public void in_between_two_trivias()
        {
            var inScope = (SqlTInScope)new SqlAnalyser( @"in between single ""-- Preconditions"" and single ""-- Actions"" begin end" ).Parse( ParseMode.TransformStatement );

            SqlTRangeLocationFinder r = (SqlTRangeLocationFinder)inScope.Location;
            LocationInfo locFirstMatch = r.FirstLocation.GetFinderInfo( isAfterContext: true );
            LocationInfo locSecondMatch = r.SecondLocation.GetFinderInfo( isAfterContext: false );

            var wholeScope = new SqlNodeScopeBreadthPredicate( n => true, "are any nodes" );

            string text = @"header0
                            -- Preconditions
                            body1 
                            -- Actions
                            action2
                            footer3";
            var input = new SqlAnalyser( text ).Parse( ParseMode.AnyExpression );
            var h = new SqlTransformHost( input );

            h.BuildRange( TestHelper.Monitor, wholeScope ).ToString().Should().Be( "[0,4[" );

            // Creating the intersection with the whole scope around the "between".
            // This is not how it should work since the cardinalities would apply to the input instead of being
            // restricted to the scope.
            {
                var matchBeg = locFirstMatch.CreateScopeBuilder();
                var matchEnd = locSecondMatch.CreateScopeBuilder();

                h.BuildRange( TestHelper.Monitor, matchBeg ).ToString().Should().Be( "[1,2[" );
                h.BuildRange( TestHelper.Monitor, matchEnd ).ToString().Should().Be( "[1,2[" );

                var matchBegExtrema = new SqlNodeScopeExtrema( matchBeg, SqlNodeScopeExtrema.Option.AfterIncluded );
                var matchEndExtrema = new SqlNodeScopeExtrema( matchEnd, SqlNodeScopeExtrema.Option.BeforeIncluded );

                h.BuildRange( TestHelper.Monitor, matchBegExtrema ).ToString().Should().Be( "[1,4[" );
                h.BuildRange( TestHelper.Monitor, matchEndExtrema ).ToString().Should().Be( "[0,2[" );

                using( TestHelper.Monitor.OpenInfo( "Without scope intersection." ) )
                {
                    var betweenScopeWithoutSingle = new SqlNodeScopeIntersect( matchBegExtrema, matchEndExtrema );
                    h.BuildRange( TestHelper.Monitor, betweenScopeWithoutSingle ).ToString().Should().Be( "[1,2[" );
                }
                var matchBegSingle = new SqlNodeScopeCardinalityFilter( matchBeg, locFirstMatch.Card );
                var matchEndSingle = new SqlNodeScopeCardinalityFilter( matchEnd, locSecondMatch.Card );

                h.BuildRange( TestHelper.Monitor, matchBegSingle ).ToString().Should().Be( "[1,2[" );
                h.BuildRange( TestHelper.Monitor, matchEndSingle ).ToString().Should().Be( "[1,2[" );

                var matchBegSingleExtrema = new SqlNodeScopeExtrema( matchBegSingle, SqlNodeScopeExtrema.Option.AfterIncluded );
                var matchEndSingleExtrema = new SqlNodeScopeExtrema( matchEndSingle, SqlNodeScopeExtrema.Option.BeforeIncluded );

                h.BuildRange( TestHelper.Monitor, matchBegSingleExtrema ).ToString().Should().Be( "[1,4[" );
                h.BuildRange( TestHelper.Monitor, matchEndSingleExtrema ).ToString().Should().Be( "[0,2[" );

                var betweenScope = new SqlNodeScopeIntersect( matchBegSingleExtrema, matchEndSingleExtrema );
                h.BuildRange( TestHelper.Monitor, betweenScope ).ToString().Should().Be( "[1,2[" );

                var final = new SqlNodeScopeIntersect( wholeScope, betweenScope );
                h.BuildRange( TestHelper.Monitor, final ).ToString().Should().Be( "[1,2[" );
            }

            // Creating the intersection with whole the whole scope at the leaf level.
            // This is how it works.
            {
                var matchBeg = locFirstMatch.CreateScopeBuilder();
                var matchEnd = locSecondMatch.CreateScopeBuilder();
                matchBeg = new SqlNodeScopeIntersect( wholeScope, matchBeg );
                matchEnd = new SqlNodeScopeIntersect( wholeScope, matchEnd );

                h.BuildRange( TestHelper.Monitor, matchBeg ).ToString().Should().Be( "[1,2[" );
                h.BuildRange( TestHelper.Monitor, matchEnd ).ToString().Should().Be( "[1,2[" );

                var matchBegExtrema = new SqlNodeScopeExtrema( matchBeg, SqlNodeScopeExtrema.Option.AfterIncluded );
                var matchEndExtrema = new SqlNodeScopeExtrema( matchEnd, SqlNodeScopeExtrema.Option.BeforeIncluded );

                h.BuildRange( TestHelper.Monitor, matchBegExtrema ).ToString().Should().Be( "[1,4[" );
                h.BuildRange( TestHelper.Monitor, matchEndExtrema ).ToString().Should().Be( "[0,2[" );

                using( TestHelper.Monitor.OpenInfo( "With scope intersection." ) )
                {
                    var betweenScopeWithoutSingle = new SqlNodeScopeIntersect( matchBegExtrema, matchEndExtrema );
                    h.BuildRange( TestHelper.Monitor, betweenScopeWithoutSingle ).ToString().Should().Be( "[1,2[" );
                }
                var matchBegSingle = new SqlNodeScopeCardinalityFilter( matchBeg, locFirstMatch.Card );
                var matchEndSingle = new SqlNodeScopeCardinalityFilter( matchEnd, locSecondMatch.Card );

                h.BuildRange( TestHelper.Monitor, matchBegSingle ).ToString().Should().Be( "[1,2[" );
                h.BuildRange( TestHelper.Monitor, matchEndSingle ).ToString().Should().Be( "[1,2[" );

                var matchBegSingleExtrema = new SqlNodeScopeExtrema( matchBegSingle, SqlNodeScopeExtrema.Option.AfterIncluded );
                var matchEndSingleExtrema = new SqlNodeScopeExtrema( matchEndSingle, SqlNodeScopeExtrema.Option.BeforeIncluded );

                h.BuildRange( TestHelper.Monitor, matchBegSingleExtrema ).ToString().Should().Be( "[1,4[" );
                h.BuildRange( TestHelper.Monitor, matchEndSingleExtrema ).ToString().Should().Be( "[0,2[" );

                var betweenScopeWithSingle = new SqlNodeScopeIntersect( matchBegSingleExtrema, matchEndSingleExtrema );
                h.BuildRange( TestHelper.Monitor, betweenScopeWithSingle ).ToString().Should().Be( "[1,2[" );
            }


        }

    }
}
