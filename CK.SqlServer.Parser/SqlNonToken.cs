using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Parser
{
    public abstract class SqlNonToken : SqlNode
    {
        int _width;

        protected SqlNonToken( ImmutableList<SqlTrivia> leading = null, ImmutableList<SqlTrivia> trailing = null )
            : base( leading, trailing )
        {
        }

        public override sealed bool IsToken( SqlTokenType t ) => false;

        public override sealed IEnumerable<ISqlNode> LeadingNodes
        {
            get
            {
                ISqlNode n = this;
                for( ;;)
                {
                    yield return n;
                    if( n.ChildrenNodes.Count == 0 ) yield break;
                    n = n.ChildrenNodes[0];
                }
            }
        }

        public override sealed IEnumerable<ISqlNode> TrailingNodes
        {
            get
            {
                ISqlNode n = this;
                for( ;;)
                {
                    yield return n;
                    if( n.ChildrenNodes.Count == 0 ) yield break;
                    n = n.ChildrenNodes[n.ChildrenNodes.Count - 1];
                }
            }
        }

        public override sealed IEnumerable<SqlTrivia> FullLeadingTrivias => LeadingNodes.SelectMany( n => n.LeadingTrivias );

        public override sealed IEnumerable<SqlTrivia> FullTrailingTrivias => TrailingNodes.Reverse().SelectMany( n => n.TrailingTrivias );

        public override sealed IEnumerable<SqlToken> AllTokens => ChildrenNodes.ToTokens();

        public override sealed int Width => _width == 0 ? (_width = ChildrenNodes.Select( c => c.Width ).Sum()) : _width;

    }
}
