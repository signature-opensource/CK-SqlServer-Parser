using CK.Core;
using CK.SqlServer.Parser;
using CK.Testing;
using Shouldly;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

using static CK.Testing.SqlTransformTestHelper;

namespace CK.SqlServer.Transform.Tests;

[TestFixture]
public class LocationTests
{
    class AllLocations : SqlNodeLocationVisitor
    {
        public readonly List<SqlNodeLocation> Collector = new List<SqlNodeLocation>();
        public readonly List<SqlNodeLocation> AfterCollector = new List<SqlNodeLocation>();

        public AllLocations( IActivityMonitor monitor )
            : base( monitor )
        {
        }

        protected override ISqlNode VisitStandard( ISqlNode e ) => VisitStandardReadOnly( e );

        protected override bool BeforeVisitItem()
        {
            Collector.Add( VisitContext.GetCurrentLocation() );
            return true;
        }

        protected override ISqlNode AfterVisitItem( ISqlNode visitResult )
        {
            AfterCollector.Add( VisitContext.GetCurrentLocation() );
            return visitResult;
        }

        static public List<SqlNodeLocation> GetAllLocations( string text,
                                                             ParseMode mode = ParseMode.OneOrMoreStatements )
        {
            List<SqlNodeLocation> locs;
            ISqlNode n = new SqlAnalyser( text ).Parse( mode );
            using( TestHelper.Monitor.OpenInfo( "GetAllLocations " + text ) )
            {
                var c = new AllLocations( TestHelper.Monitor );
                c.VisitRoot( n );
                locs = c.Collector;
                var afterLocs = c.AfterCollector;
                if( TestHelper.LogToConsole )
                {
                    int i = 0;
                    foreach( var l in locs )
                    {
                        TestHelper.Monitor.Trace( "[" + i++ + "] " + l.ToString() );
                    }
                }
                locs.Select( l => l.ToString() ).ShouldBe( afterLocs.Select( l => l.ToString() ), ignoreOrder: true );
            }
            return locs;
        }
    }

    [Test]
    public void creating_all_locations()
    {
        List<SqlNodeLocation> locs = AllLocations.GetAllLocations( "select W as A, C = Z;", ParseMode.Statement );
        locs.Count.ShouldBe( 15 );
        locs[8].Node.ToString().ShouldBe( "A" );
        locs[9].Node.IsToken( SqlTokenType.Comma ).ShouldBeTrue();
        locs[10].Node.ToString().ShouldBe( "C=Z" );
    }

    [Test]
    public void multi_statements_locations()
    {
        List<SqlNodeLocation> locs = AllLocations.GetAllLocations( "break; select 1; continue; select 2;", ParseMode.OneOrMoreStatements );
        locs.Count.ShouldBe( 25 );
        locs[5].Node.ToString().ShouldBe( "select 1;" );
        locs[6].Node.ToString().ShouldBe( "select 1" );
        locs[7].Node.ToString().ShouldBe( "select" );
        locs[8].Node.ToString().ShouldBe( "select" );
        locs[9].Node.ShouldBeAssignableTo<SelectColumnList>().ShouldNotBeNull();
        locs[10].Node.ShouldBeAssignableTo<SelectColumn>().ShouldNotBeNull();
        locs[11].Node.ShouldBeAssignableTo<SqlTokenLiteralInteger>().ShouldNotBeNull();
        locs[12].Node.IsToken( SqlTokenType.SemiColon ).ShouldBeTrue();
    }

    [Test]
    public void mono_token_with_beg_and_end_markers()
    {
        List<SqlNodeLocation> locs = AllLocations.GetAllLocations( "A", ParseMode.OneExpression );
        locs.Count.ShouldBe( 1 );

        locs[0].IsBegMarker.ShouldBeFalse();
        locs[0].IsEndMarker.ShouldBeFalse();
        locs[0].Node.IsToken( SqlTokenType.IdentifierStandard ).ShouldBeTrue();
        locs[0].Position.ShouldBe( 0 );
        locs[0].Root.ShouldBeSameAs( locs[0] );

        var beg = locs[0].Predecessor();
        beg.IsBegMarker.ShouldBeTrue();
        beg.IsEndMarker.ShouldBeFalse();
        beg.Position.ShouldBe( -1 );
        beg.Node.ShouldBeSameAs( SqlKeyword.BegOfInput );
        beg.Predecessor().ShouldBeNull();
        beg.Successor().ShouldBeSameAs( locs[0] );

        var end = locs[0].Successor();
        end.IsBegMarker.ShouldBeFalse();
        end.IsEndMarker.ShouldBeTrue();
        end.Position.ShouldBe( 1 );
        end.Node.ShouldBeSameAs( SqlKeyword.EndOfInput );
        end.Successor().ShouldBeNull();
        end.Predecessor().Position.ShouldBe( 0 );
    }

    [Test]
    public void flat_successors_and_predecessors()
    {
        List<SqlNodeLocation> locs = AllLocations.GetAllLocations( "A B C D E F", ParseMode.ExtendedExpression );
        locs.Count.ShouldBe( 7 );
        locs.Select( l => l.Position ).SequenceEqual( new[] { 0, 0, 1, 2, 3, 4, 5 } ).ShouldBeTrue();
        var prec = locs.Select( l => l.Predecessor() ).ToArray();
        prec[0].IsBegMarker.ShouldBeTrue();
        prec[1].IsBegMarker.ShouldBeTrue();
        prec.Select( l => l.Position ).SequenceEqual( new[] { -1, -1, 0, 1, 2, 3, 4 } ).ShouldBeTrue();

        var succ = locs.Select( l => l.Successor() ).ToArray();
        succ.Select( l => l.Position ).SequenceEqual( new[] { 1, 1, 2, 3, 4, 5, 6 } ).ShouldBeTrue();
    }


}
