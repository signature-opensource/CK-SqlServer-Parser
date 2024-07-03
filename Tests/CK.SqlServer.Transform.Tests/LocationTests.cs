using CK.Core;
using CK.SqlServer.Parser;
using CK.Testing;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

using static CK.Testing.SqlTransformTestHelper;

namespace CK.SqlServer.Transform.Tests
{
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
                    locs.Select( l => l.ToString() ).Should().BeEquivalentTo( afterLocs.Select( l => l.ToString() ) );
                }
                return locs;
            }
        }

        [Test]
        public void creating_all_locations()
        {
            List<SqlNodeLocation> locs = AllLocations.GetAllLocations( "select W as A, C = Z;", ParseMode.Statement );
            Assert.That( locs.Count, Is.EqualTo( 15 ) );
            Assert.That( locs[8].Node.ToString(), Is.EqualTo( "A" ) );
            Assert.That( locs[9].Node.IsToken( SqlTokenType.Comma ) );
            Assert.That( locs[10].Node.ToString(), Is.EqualTo( "C=Z" ) );
        }

        [Test]
        public void multi_statements_locations()
        {
            List<SqlNodeLocation> locs = AllLocations.GetAllLocations( "break; select 1; continue; select 2;", ParseMode.OneOrMoreStatements );
            Assert.That( locs.Count, Is.EqualTo( 25 ) );
            Assert.That( locs[5].Node.ToString(), Is.EqualTo( "select 1;" ) );
            Assert.That( locs[6].Node.ToString(), Is.EqualTo( "select 1" ) );
            Assert.That( locs[7].Node.ToString(), Is.EqualTo( "select" ) );
            Assert.That( locs[8].Node.ToString(), Is.EqualTo( "select" ) );
            Assert.That( locs[9].Node, Is.InstanceOf<SelectColumnList>() );
            Assert.That( locs[10].Node, Is.InstanceOf<SelectColumn>() );
            Assert.That( locs[11].Node, Is.InstanceOf<SqlTokenLiteralInteger>() );
            Assert.That( locs[12].Node.IsToken( SqlTokenType.SemiColon ) );
        }

        [Test]
        public void mono_token_with_beg_and_end_markers()
        {
            List<SqlNodeLocation> locs = AllLocations.GetAllLocations( "A", ParseMode.OneExpression );
            Assert.That( locs.Count, Is.EqualTo( 1 ) );

            Assert.That( locs[0].IsBegMarker, Is.False );
            Assert.That( locs[0].IsEndMarker, Is.False );
            Assert.That( locs[0].Node.IsToken( SqlTokenType.IdentifierStandard ) );
            Assert.That( locs[0].Position, Is.EqualTo( 0 ) );
            Assert.That( locs[0].Root, Is.SameAs( locs[0] ) );

            var beg = locs[0].Predecessor();
            Assert.That( beg.IsBegMarker );
            Assert.That( beg.IsEndMarker, Is.False );
            Assert.That( beg.Position, Is.EqualTo( -1 ) );
            Assert.That( beg.Node, Is.SameAs( SqlKeyword.BegOfInput ) );
            Assert.That( beg.Predecessor(), Is.Null );
            Assert.That( beg.Successor(), Is.SameAs( locs[0] ) );

            var end = locs[0].Successor();
            Assert.That( end.IsBegMarker, Is.False );
            Assert.That( end.IsEndMarker );
            Assert.That( end.Position, Is.EqualTo( 1 ) );
            Assert.That( end.Node, Is.SameAs( SqlKeyword.EndOfInput ) );
            Assert.That( end.Successor(), Is.Null );
            Assert.That( end.Predecessor().Position, Is.EqualTo( 0 ) );
        }

        [Test]
        public void flat_successors_and_predecessors()
        {
            List<SqlNodeLocation> locs = AllLocations.GetAllLocations( "A B C D E F", ParseMode.ExtendedExpression );
            Assert.That( locs.Count, Is.EqualTo( 7 ) );
            Assert.That( locs.Select( l => l.Position ).SequenceEqual( new[] { 0, 0, 1, 2, 3, 4, 5 } ) );
            var prec = locs.Select( l => l.Predecessor() ).ToArray();
            Assert.That( prec[0].IsBegMarker );
            Assert.That( prec[1].IsBegMarker );
            Assert.That( prec.Select( l => l.Position ).SequenceEqual( new[] { -1, -1, 0, 1, 2, 3, 4 } ) );

            var succ = locs.Select( l => l.Successor() ).ToArray();
            Assert.That( succ.Select( l => l.Position ).SequenceEqual( new[] { 1, 1, 2, 3, 4, 5, 6 } ) );
        }


    }
}
