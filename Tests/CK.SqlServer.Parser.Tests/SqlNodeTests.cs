using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Immutable;
using CK.Core;
using System.Diagnostics;

namespace CK.SqlServer.Parser.Tests
{
    [TestFixture]
    public class SqlNodeTests
    {
        public class TestNode : SqlNodeExternal
        {
            readonly string _name;
            readonly SqlNode[] _content;

            public TestNode( string name, SqlNode[] content = null, ImmutableList<SqlTrivia> leading = null, ImmutableList<SqlTrivia> trailing = null )
                : base( leading, trailing )
            {
                _name = name;
                _content = content ?? Util.EmptyArray<SqlNode>.Empty;
            }

            public override IReadOnlyList<SqlNode> ChildrenNodes => _content;


            protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IReadOnlyList<SqlNode> children, ImmutableList<SqlTrivia> trailing )
            {
                return new TestNode( _name, children.ToArray(), leading, trailing );
            }

            public override void WriteWithoutTrivias( ISqlTextWriter w )
            {
                w.Write( _name );
                base.WriteWithoutTrivias( w );
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
            n = n.StuffChildren( 0, 0, new[] { n1, n2 } );

            Assert.That( n.ToString( false ), Is.EqualTo( "N[b1[[a1[N1]a1]]b1][b2[[a2[N2]a2]]b2]" ) );
            Assert.That( n.ToString( true ), Is.EqualTo( "/*<<*/N[b1[[a1[N1]a1]]b1][b2[[a2[N2]a2]]b2]/*>>*/" ) );

            SqlNode nLeftLift = n.LiftLeadingTrivias();
            Assert.That( nLeftLift.LeadingTrivias.Count, Is.EqualTo( 3 ) );
            Assert.That( nLeftLift.ChildrenNodes[0].LeadingTrivias, Is.Empty );
            Assert.That( nLeftLift.ToString( true ), Is.EqualTo( "/*<<*/[b1[[a1[NN1]a1]]b1][b2[[a2[N2]a2]]b2]/*>>*/" ) );

            SqlNode nRightLift = n.LiftTrailingTrivias();
            Assert.That( nRightLift.TrailingTrivias.Count, Is.EqualTo( 3 ) );
            Assert.That( nRightLift.ChildrenNodes[1].TrailingTrivias, Is.Empty );
            Assert.That( nRightLift.ToString( true ), Is.EqualTo( "/*<<*/N[b1[[a1[N1]a1]]b1][b2[[a2[N2]a2]]b2]/*>>*/" ) );
            Assert.That( nRightLift.ToString( false ), Is.EqualTo( "N[b1[[a1[N1]a1]]b1][b2[[a2[N2" ) );

            SqlNode nLift = n.LiftBothTrivias();
            Assert.That( nLift.LeadingTrivias.Count, Is.EqualTo( 3 ) );
            Assert.That( nLift.TrailingTrivias.Count, Is.EqualTo( 3 ) );
            Assert.That( nLift.ChildrenNodes[0].LeadingTrivias, Is.Empty );
            Assert.That( nLift.ChildrenNodes[1].TrailingTrivias, Is.Empty );
            Assert.That( nLift.ToString( true ), Is.EqualTo( "/*<<*/[b1[[a1[NN1]a1]]b1][b2[[a2[N2]a2]]b2]/*>>*/" ) );
            Assert.That( nLift.ToString( false ), Is.EqualTo( "NN1]a1]]b1][b2[[a2[N2" ) );
        }

        [Test]
        public void SqlNode_write_with_trivias()
        {
            SqlNode n = new TestNode( "X" )
                            .AddLeadingTrivia( new SqlTrivia( SqlTokenType.None, Environment.NewLine + " 1 " + Environment.NewLine ) )
                            .AddTrailingTrivia( new SqlTrivia( SqlTokenType.None, Environment.NewLine + " 2 " + Environment.NewLine ) );
            SqlNode n2 = new TestNode( "Y" )
                            .AddLeadingTrivia( new SqlTrivia( SqlTokenType.None, Environment.NewLine + " 3 " + Environment.NewLine ) )
                            .AddTrailingTrivia( new SqlTrivia( SqlTokenType.None, Environment.NewLine + " 4 " + Environment.NewLine ) );
            n = n2.StuffChildren( 0, 0, new[] { n } );

            Assert.That( n.ToString( true ), Is.EqualTo(
                Environment.NewLine + " 3 " + Environment.NewLine
                    + "Y"
                        + Environment.NewLine + " 1 " + Environment.NewLine 
                        + "X" 
                        + Environment.NewLine + " 2 " + Environment.NewLine
                + Environment.NewLine + " 4 " + Environment.NewLine ) );

            Assert.That( n.ToString(), Is.EqualTo( "Y X" ) );
        }

    }
}
