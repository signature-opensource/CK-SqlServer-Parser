using NUnit.Framework;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Immutable;
using CK.Core;
using System.Diagnostics;

namespace CK.SqlServer.Parser.Tests;

[TestFixture]
public sealed class SqlNodeTests
{
    public class TestNode : SqlNodeExternal
    {
        readonly string _name;
        readonly ISqlNode[] _content;
        readonly SqlTokenType _tokenLike;

        public TestNode( string name, IEnumerable<ISqlNode> content = null, ImmutableList<SqlTrivia> leading = null, ImmutableList<SqlTrivia> trailing = null )
            : base( leading, trailing )
        {
            _tokenLike = SqlTokenType.IdentifierStandard;
            _name = name;
            _content = content != null ? content.ToArray() : Array.Empty<ISqlNode>();
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public override IList<ISqlNode> GetRawContent() => _content.ToList();


        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return new TestNode( _name, content ?? _content, leading, trailing );
        }

        public override void WriteWithoutTrivias( ISqlTextWriter w )
        {
            w.Write( _tokenLike, _name );
            base.WriteWithoutTrivias( w );
        }
    }

    [Test]
    public void SqlNode_trivias_can_be_lifted()
    {
        ISqlNode n = new TestNode( "N" );
        n = n.AddLeadingTrivia( new SqlTrivia( SqlTokenType.StarComment, "<<" ) )
                .AddTrailingTrivia( new SqlTrivia( SqlTokenType.StarComment, ">>" ) );
        ISqlNode n1 = new TestNode( "N1" );
        n1 = n1.AddLeadingTrivia( new SqlTrivia( SqlTokenType.None, "[a1[" ) )
                    .AddLeadingTrivia( new SqlTrivia( SqlTokenType.None, "[b1[" ) )
                    .AddTrailingTrivia( new SqlTrivia( SqlTokenType.None, "]a1]" ) )
                    .AddTrailingTrivia( new SqlTrivia( SqlTokenType.None, "]b1]" ) );
        ISqlNode n2 = new TestNode( "N2" );
        n2 = n2.AddLeadingTrivia( new SqlTrivia( SqlTokenType.None, "[a2[" ) )
                    .AddLeadingTrivia( new SqlTrivia( SqlTokenType.None, "[b2[" ) )
                    .AddTrailingTrivia( new SqlTrivia( SqlTokenType.None, "]a2]" ) )
                    .AddTrailingTrivia( new SqlTrivia( SqlTokenType.None, "]b2]" ) );
        n = n.StuffRawContent( 0, 0, new[] { n1, n2 } );

        n.ToString( false ).ShouldBe( "N[b1[[a1[N1]a1]]b1][b2[[a2[N2]a2]]b2]" );
        n.ToString( true ).ShouldBe( "/*<<*/N[b1[[a1[N1]a1]]b1][b2[[a2[N2]a2]]b2]/*>>*/" );

        ISqlNode nLeftLift = n.LiftLeadingTrivias();
        nLeftLift.LeadingTrivias.Count.ShouldBe( 3 );
        nLeftLift.ChildrenNodes[0].LeadingTrivias.ShouldBeEmpty();
        nLeftLift.ToString( true ).ShouldBe( "/*<<*/[b1[[a1[N N1]a1]]b1][b2[[a2[N2]a2]]b2]/*>>*/" );

        ISqlNode nRightLift = n.LiftTrailingTrivias();
        nRightLift.TrailingTrivias.Count.ShouldBe( 3 );
        nRightLift.ChildrenNodes[1].TrailingTrivias.ShouldBeEmpty();
        nRightLift.ToString( true ).ShouldBe( "/*<<*/N[b1[[a1[N1]a1]]b1][b2[[a2[N2]a2]]b2]/*>>*/" );
        nRightLift.ToString( false ).ShouldBe( "N[b1[[a1[N1]a1]]b1][b2[[a2[N2" );

        ISqlNode nLift = n.LiftBothTrivias();
        nLift.LeadingTrivias.Count.ShouldBe( 3 );
        nLift.TrailingTrivias.Count.ShouldBe( 3 );
        nLift.ChildrenNodes[0].LeadingTrivias.ShouldBeEmpty();
        nLift.ChildrenNodes[1].TrailingTrivias.ShouldBeEmpty();
        nLift.ToString( true ).ShouldBe( "/*<<*/[b1[[a1[N N1]a1]]b1][b2[[a2[N2]a2]]b2]/*>>*/" );
        nLift.ToString( false ).ShouldBe( "N N1]a1]]b1][b2[[a2[N2" );
    }

    [Test]
    public void SqlNode_write_with_trivias()
    {
        ISqlNode n = new TestNode( "X" )
                        .AddLeadingTrivia( new SqlTrivia( SqlTokenType.None, Environment.NewLine + " 1 " + Environment.NewLine ) )
                        .AddTrailingTrivia( new SqlTrivia( SqlTokenType.None, Environment.NewLine + " 2 " + Environment.NewLine ) );
        ISqlNode n2 = new TestNode( "Y" )
                        .AddLeadingTrivia( new SqlTrivia( SqlTokenType.None, Environment.NewLine + " 3 " + Environment.NewLine ) )
                        .AddTrailingTrivia( new SqlTrivia( SqlTokenType.None, Environment.NewLine + " 4 " + Environment.NewLine ) );
        n = n2.StuffRawContent( 0, 0, new[] { n } );

        n.ToString( true ).ShouldBe( Environment.NewLine + " 3 " + Environment.NewLine
                + "Y"
                    + Environment.NewLine + " 1 " + Environment.NewLine
                    + "X"
                    + Environment.NewLine + " 2 " + Environment.NewLine
            + Environment.NewLine + " 4 " + Environment.NewLine );

        n.ToString().ShouldBe( "Y X" );
    }

    [TestCase( "A /*1*/ \t |B /*2*/ |C /*3*/ ", "A /*1*/| \t B /*2*/ |C /*3*/ " )]
    public void moving_white_space_between_tokens( string before, string after )
    {
        ISqlNode list;
        SqlAnalyser.Parse( out list, ParseMode.AnyExpression, before.Replace( "|", "" ) ).IsError.ShouldBeFalse();
        SqlToken[] all = list.ChildrenNodes.Cast<SqlToken>().ToArray();
        string.Join( "|", all.Select( t => t.ToString( true ) ) ).ShouldBe( before );

        var allOnB = SqlTrivia.WhiteSpaceToMiddle( all[0], all[1], all[2] );
        SqlToken[] all2 = new SqlToken[] { allOnB.Item1, allOnB.Item2, allOnB.Item3 };
        string.Join( "|", all2.Select( t => t.ToString( true ) ) ).ShouldBe( after );
    }

}
