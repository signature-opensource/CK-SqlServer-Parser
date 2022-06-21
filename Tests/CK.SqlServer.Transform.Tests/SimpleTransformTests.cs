using CK.Core;
using CK.SqlServer.Parser;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using CK.SqlServer.UtilTests;
using CK.SqlServer.Transform.Transformers;
using FluentAssertions;

namespace CK.SqlServer.Transform.Tests
{
    [TestFixture]
    public class SimpleTransformTests
    {
        [TestCase( "( @i int )", "@P int ", "( @i int, @P int )", null, null )]
        [TestCase( "( @i int )", "@P int ", "( @P int, @i int )", null, "@i" )]
        [TestCase( "", "@P int", "(@P int)", null, null )]
        [TestCase( " @i int", "@P int = 0 output", " @i int, @P int = 0 output", null, null )]
        [TestCase( " @i int, @j float = 0.2", "@P int = 0 output", " @i int, @j float = 0.2, @P int = 0 output", null, null )]
        [TestCase( " @i int, @j float = 0.2", "@P int", " @i int, @j float = 0.2, @P int", "@j", null )]
        [TestCase( "(@i int, @j float = 0.2)", "@P int", "(@i int, @P int, @j float = 0.2)", "@i", null )]
        [TestCase( "(@i int, @j float = 0.2)", "@P int", "(@P int, @i int, @j float = 0.2)", null, "@i" )]
        [TestCase( "(@i int, @j float = 0.2)", "@P int", "(@i int, @P int, @j float = 0.2)", null, "@j" )]
        [TestCase( @"
    (
        @i int, 
        @j float = 0.2
    )
", "@P int", @"
    (
        @i int, 
        @P int, 
        @j float = 0.2
    )
", null, "@j" )]
        public void adding_parameters( string paramList, string parameter, string result, string beforeName, string afterName )
        {
            SqlAnalyser a = new SqlAnalyser();
            a.Reset( parameter );
            SqlParameter pS = a.IsParameter( true );
            Assert.That( pS != null );
            a.Reset( "create procedure t" + paramList + " as begin select 0; end" );
            ISqlParameterListHolder p;
            Assert.That( a.ParseStatement( out p ).IsError, Is.False );
            ISqlParameterListHolder p2 = p.InsertParameter( pS, beforeName, afterName );
            result = "create procedure t" + result + " as begin select 0; end";
            CheckRenderResult( result, a, p2 );
        }

        [TestCase( "select W as A, X B, C = Z", 
                   "select A = W, B=X, C = Z" )]
        [TestCase( "select W as A, X B, C=Z", 
                   "select A = W, B=X, C=Z" )]
        [TestCase( "select W /*1*/as /*2*/A, X/*3*/ B/*4*/, /*5*/Z/*6*/as/*7*/C/*8*/",
                   "select /*2*/A = W /*1*/, B/*4*/ = X/*3*/, /*7*/C/*8*/ = /*5*/Z/*6*/" )]
        public void select_columns_can_be_rewritten_to_use_equal( string s, string result )
        {
            var a = new SqlAnalyser( s );
            SqlSelectStatement st;
            Assert.That( a.ParseStatement( out st ).IsError, Is.False );
            ISqlNode transformed = new SetSelectColumnAsOrAssign( TestHelper.ConsoleMonitor, true ).VisitRoot( st );
            CheckRenderResult( result, a, transformed );
        }

        [TestCase( "select A = W, X B, C = Z", "select W as A, X as B, Z as C" )]
        [TestCase( "select A = W, X B, C=Z", "select W as A, X as B, Z as C" )]
        public void select_columns_can_be_rewritten_to_use_as( string s, string result )
        {
            var a = new SqlAnalyser( s );
            SqlSelectStatement st;
            Assert.That( a.ParseStatement( out st ).IsError, Is.False );
            SqlSelectStatement transformed = (SqlSelectStatement)new SetSelectColumnAsOrAssign( TestHelper.ConsoleMonitor, false ).VisitRoot( st );
            CheckRenderResult( result, a, transformed );
        }

        [TestCase( "insert into T default values", "insert into T (NewCol) values (NewVal)" )]
        [TestCase( "insert into T (C) values (@P)", "insert into T (C, NewCol) values (@P, NewVal)" )]
        [TestCase( "insert into T (C1, C2) values (@P1, @P2)", "insert into T (C1, C2, NewCol) values (@P1, @P2, NewVal)" )]
        [TestCase( "insert into T (C1, C2) values (@P1, @P2), (@P3, @P4)", "insert into T (C1, C2, NewCol) values (@P1, @P2, NewVal), (@P3, @P4, NewVal)" )]
        [TestCase( "insert into T (C1, C2) select A, B from T", "insert into T (C1, C2, NewCol) select A, B, NewVal from T" )]
        [TestCase( "insert into T (C) select A from T1 union select B from T2", "insert into T (C, NewCol) select A, NewVal from T1 union select B, NewVal from T2" )]
        [TestCase(
            "insert into T (C) select A = (select S1 from TZ) from T",
            "insert into T (C, NewCol) select A = (select S1 from TZ), NewVal from T" )]
        [TestCase(
            "insert into T (C) select A = (select S1 from TZ) from (select X) as X",
            "insert into T (C, NewCol) select A = (select S1 from TZ), NewVal from (select X) as X" )]
        public void add_insert_into_values( string s, string result )
        {
            var a = new SqlAnalyser( s );
            SqlInsertStatement st;
            Assert.That( a.ParseStatement( out st ).IsError, Is.False );
            SqlTokenIdentifier colName = new SqlTokenIdentifier( SqlTokenType.IdentifierStandard, "NewCol" );
            ISqlNode newVal = new SqlTokenIdentifier( SqlTokenType.IdentifierStandard, "NewVal" );
            SqlInsertStatement transformed = st.AddSimpleColumn( colName, newVal );
            CheckRenderResult( result, a, transformed );
        }

        [TestCase( "first {a}", "a", 0 )]
        [TestCase( "first {a}", "b", -1 )]
        [TestCase( "first {a}", "X|a", 1 )]
        [TestCase( "first {a}", "X|Y|a|Z", 2 )]
        [TestCase( "first {a}", "d|a|b|a|c|a", 1 )]
        [TestCase( "first+1 {a}", "d|a|b|a|c|a", 3 )]
        [TestCase( "first+2 {a}", "d|a|b|a|c|a", 5 )]
        [TestCase( "first+3 {a}", "d|a|b|a|c|a", -1 )]

        [TestCase( "last {a}", "a", 0 )]
        [TestCase( "last {a}", "b", -1 )]
        [TestCase( "last {a}", "X|a", 1 )]
        [TestCase( "last {a}", "X|Y|a|Z", 2 )]
        [TestCase( "last {a}", "d|a|b|a|c|a", 5 )]
        [TestCase( "last-1 {a}", "d|a|b|a|c|a", 3 )]
        [TestCase( "last-2 {a}", "d|a|b|a|c|a", 1 )]
        [TestCase( "last-3 {a}", "d|a|b|a|c|a", -1 )]
        public void checking_cardinality_filter( string cardinality, string text, int resultIndex )
        {
            var filterInfo = new SqlAnalyser( cardinality ).IsISqlTLocationFinder( true );
            var info = filterInfo.GetCardinality();
            var matcher = new SqlNodeScopePatternRange( filterInfo.Pattern.AllTokens.Skip(1).Take(1).ToArray() );
            var filter = new SqlNodeScopeCardinalityFilter( matcher, info );
            ISqlNode nodes = new SqlNodeList( text.Split('|').Select( t => SqlTokenIdentifier.Create( t ) ) );
            var host = new SqlTransformHost( nodes );
            var ranges = host.BuildRange( TestHelper.ConsoleMonitor, filter );
            if( resultIndex >= 0 )
            {
                Assert.That( ranges.Count, Is.EqualTo( 1 ) );
                Assert.That( ranges.First.Beg.Position, Is.EqualTo( resultIndex ) );
            }
            else
            {
                Assert.That( ranges, Is.SameAs( SqlNodeLocationRange.EmptySet ) );
            }
        }

        static void CheckRenderResult( string result, SqlAnalyser a, ISqlNode transformed, [CallerMemberName]string caller = null  )
        {
            a.Reset( result );
            string compactResult = a.IsExtendedStatement( true ).ToStringHyperCompact();
            Assert.That( transformed.ToStringHyperCompact(), Is.EqualTo( compactResult ) );

            if( transformed.ToString( true ) != result )
            {
                using( TestHelper.ConsoleMonitor.OpenWarn( caller ) )
                {
                    using( TestHelper.ConsoleMonitor.OpenInfo( "Expected" ) )
                    {
                        TestHelper.ConsoleMonitor.Info( result );
                    }
                    using( TestHelper.ConsoleMonitor.OpenInfo( "Actual" ) )
                    {
                        TestHelper.ConsoleMonitor.Info( transformed.ToString( true ) );
                    }
                }
                Assume.That( false, "Rendering is not perfect..." );
            }
        }

        [TestCase( "/*1*/create/*this will be lost*/or alter/*2*/procedure p as begin select 0; end",
                   CreateOrAlterStatementPrefix.Create,
                   "/*1*/create/*2*/procedure p as begin select 0; end" )]
        [TestCase( "/*1*/create or alter procedure p as begin select 0; end",
                   CreateOrAlterStatementPrefix.Alter,
                   "/*1*/alter procedure p as begin select 0; end" )]
        [TestCase( "/*1*/alter/*2*/procedure p as begin select 0; end",
                   CreateOrAlterStatementPrefix.CreateOrAlter,
                   "/*1*/create or alter/*2*/procedure p as begin select 0; end" )]
        public void create_or_alter_mutations( string source, CreateOrAlterStatementPrefix target, string result )
        {
            var src = (ISqlServerAlterOrCreateStatement)new SqlAnalyser( source ).IsNamedStatement( true );
            src = src.WithStatementPrefix( target );
            src.ToFullString().Should().Be( result );
        }

        [Test]
        public void create_or_alter_mutations()
        {
            const string source = "/*1*/create/*this will be lost*/or alter/*2*/procedure p as begin select 0; end";
            var src = (ISqlServerAlterOrCreateStatement)new SqlAnalyser( source ).IsNamedStatement( true );
            src.StatementPrefix.Should().Be( CreateOrAlterStatementPrefix.CreateOrAlter );
            var create = src.WithStatementPrefix( CreateOrAlterStatementPrefix.Create );
            create.StatementPrefix.Should().Be( CreateOrAlterStatementPrefix.Create );
            create.ToFullString().Should().StartWith( "/*1*/create/*2*/procedure" );
            var alter = src.WithStatementPrefix( CreateOrAlterStatementPrefix.Alter );
            alter.StatementPrefix.Should().Be( CreateOrAlterStatementPrefix.Alter );
            alter.ToFullString().Should().StartWith( "/*1*/alter/*2*/procedure" );
        }

    }
}
