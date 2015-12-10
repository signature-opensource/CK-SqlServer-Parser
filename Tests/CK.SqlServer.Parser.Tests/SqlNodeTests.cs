using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Immutable;
using CK.Core;

namespace CK.SqlServer.Parser.Tests
{
    [TestFixture]
    public class SqlNodeTests
    {
        class TestNode : SqlNode
        {
            readonly IReadOnlyList<TestNode> _nodes;

            public TestNode( string name )
            {
                Name = name;
                _nodes = Util.EmptyArray<TestNode>.Empty;
            }

            TestNode( string name, ImmutableList<SqlTrivia> leading, IReadOnlyList<TestNode> children, ImmutableList<SqlTrivia> trailing )
                : base( leading, trailing )
            {
                _nodes = children;
                Name = name;
            }

            public string Name { get; }

            public override IReadOnlyList<SqlNode> ChildrenNodes => _nodes;

            protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IReadOnlyList<SqlNode> children, ImmutableList<SqlTrivia> trailing )
            {
                return new TestNode( Name, leading, children.Cast<TestNode>().ToReadOnlyList(), trailing );
            }

            protected override void DoWrite( StringBuilder b, SqlTriviaWriteOption option )
            {
                b.Append( Name );
                foreach( var c in _nodes ) c.Write( b, option );
            }
        }

        [Test]
        public void SqlNode_trivias_can_be_lifted()
        {
            SqlNode n = new TestNode( "N" );
            n = n.AddLeadingTrivia( new SqlTrivia( SqlTokenType.StarComment, "<<" ) )
                    .AddTrailingTrivia( new SqlTrivia( SqlTokenType.StarComment, ">>" ) );
            SqlNode n1 = new TestNode( "N1" );
            n1 = n1.AddLeadingTrivia( new SqlTrivia( SqlTokenType.None, "[a1[" ) )
                        .AddLeadingTrivia( new SqlTrivia( SqlTokenType.None, "[b1[" ) )
                        .AddTrailingTrivia( new SqlTrivia( SqlTokenType.None, "]a1]" ) )
                        .AddTrailingTrivia( new SqlTrivia( SqlTokenType.None, "]b1]" ) );
            SqlNode n2 = new TestNode( "N2" );
            n2 = n2.AddLeadingTrivia( new SqlTrivia( SqlTokenType.None, "[a2[" ) )
                        .AddLeadingTrivia( new SqlTrivia( SqlTokenType.None, "[b2[" ) )
                        .AddTrailingTrivia( new SqlTrivia( SqlTokenType.None, "]a2]" ) )
                        .AddTrailingTrivia( new SqlTrivia( SqlTokenType.None, "]b2]" ) );
            n = n.InsertChildNode( 0, n1 ).InsertChildNode( 1, n2 );

            Assert.That( n.FullLeadingTrivias.ToString( SqlTriviaWriteOption.None ), Is.EqualTo( "/*<<*/[b1[[a1[" ) );
            Assert.That( n.FullTrailingTrivias.ToString( SqlTriviaWriteOption.None ), Is.EqualTo( "]a2]]b2]/*>>*/" ) );
            Assert.That( n.ToString( false ), Is.EqualTo( "N[b1[[a1[N1]a1]]b1][b2[[a2[N2]a2]]b2]" ) );
            Assert.That( n.ToString( true ), Is.EqualTo( "/*<<*/N[b1[[a1[N1]a1]]b1][b2[[a2[N2]a2]]b2]/*>>*/" ) );

            SqlNode nLeftLift = n.LiftLeadingTrivias();
            Assert.That( nLeftLift.LeadingTrivias.Count, Is.EqualTo( 3 ) );
            Assert.That( nLeftLift.FullLeadingTrivias.ToString( SqlTriviaWriteOption.None ), Is.EqualTo( "/*<<*/[b1[[a1[" ) );
            Assert.That( nLeftLift.FullTrailingTrivias.ToString( SqlTriviaWriteOption.None ), Is.EqualTo( "]a2]]b2]/*>>*/" ) );
            Assert.That( nLeftLift.ChildrenNodes[0].LeadingTrivias, Is.Empty );
            Assert.That( nLeftLift.ToString( true ), Is.EqualTo( "/*<<*/[b1[[a1[NN1]a1]]b1][b2[[a2[N2]a2]]b2]/*>>*/" ) );

            SqlNode nRightLift = n.LiftTrailingTrivias();
            Assert.That( nRightLift.TrailingTrivias.Count, Is.EqualTo( 3 ) );
            Assert.That( nRightLift.FullLeadingTrivias.ToString( SqlTriviaWriteOption.None ), Is.EqualTo( "/*<<*/[b1[[a1[" ) );
            Assert.That( nRightLift.FullTrailingTrivias.ToString( SqlTriviaWriteOption.None ), Is.EqualTo( "]a2]]b2]/*>>*/" ) );
            Assert.That( nRightLift.ChildrenNodes[1].TrailingTrivias, Is.Empty );
            Assert.That( nRightLift.ToString( true ), Is.EqualTo( "/*<<*/N[b1[[a1[N1]a1]]b1][b2[[a2[N2]a2]]b2]/*>>*/" ) );
            Assert.That( nRightLift.ToString( false ), Is.EqualTo( "N[b1[[a1[N1]a1]]b1][b2[[a2[N2" ) );

            SqlNode nLift = n.LiftBothTrivias();
            Assert.That( nLift.LeadingTrivias.Count, Is.EqualTo( 3 ) );
            Assert.That( nLift.TrailingTrivias.Count, Is.EqualTo( 3 ) );
            Assert.That( nLift.FullLeadingTrivias.ToString( SqlTriviaWriteOption.None ), Is.EqualTo( "/*<<*/[b1[[a1[" ) );
            Assert.That( nLift.FullTrailingTrivias.ToString( SqlTriviaWriteOption.None ), Is.EqualTo( "]a2]]b2]/*>>*/" ) );
            Assert.That( nLift.ChildrenNodes[0].LeadingTrivias, Is.Empty );
            Assert.That( nLift.ChildrenNodes[1].TrailingTrivias, Is.Empty );
            Assert.That( nLift.ToString( true ), Is.EqualTo( "/*<<*/[b1[[a1[NN1]a1]]b1][b2[[a2[N2]a2]]b2]/*>>*/" ) );
            Assert.That( nLift.ToString( false ), Is.EqualTo( "NN1]a1]]b1][b2[[a2[N2" ) );
        }

    }
}
