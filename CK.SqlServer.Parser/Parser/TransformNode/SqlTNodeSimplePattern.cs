using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using CK.Core;
using System.Collections.Immutable;

namespace CK.SqlServer.Parser
{
    using CNode = SNode<
            SqlTokenIdentifier,
            SqlTokenIdentifier,
            SqlTokenIdentifier,
            SqlTRawNodeList>;

    /// <summary>
    /// insert (raw|statement) I
    /// </summary>
    public sealed class SqlTNodeSimplePattern : SqlNonToken, ISqlTNodeMatcher
    {
        readonly CNode _content;

        public SqlTNodeSimplePattern( SqlTokenIdentifier largestOrDeepestT, SqlTokenIdentifier nodesT, SqlTokenIdentifier likeT, SqlTRawNodeList nodeList )
            : base( null, null )
        {
            _content = new CNode( largestOrDeepestT, nodesT, likeT, nodeList );
            CheckContent();
        }

        void CheckContent()
        {
            Helper.CheckNullableToken( LargestOrDeepestT, nameof( LargestOrDeepestT ), SqlTokenType.Largest, SqlTokenType.Deepest );
            Helper.CheckNullableToken( NodesT, nameof( NodesT ), SqlTokenType.Nodes );

            Helper.CheckNullableToken( NodesT, nameof( NodesT ), SqlTokenType.Nodes );
            Helper.CheckNullableToken( LikeT, nameof( LikeT ), SqlTokenType.Like );
            Helper.CheckBothNullOrNot( NodesT, nameof( NodesT ), LikeT, nameof( LikeT ) );

            Helper.CheckNotNull( RawList, nameof( RawList ) );
        }

        SqlTNodeSimplePattern( SqlTNodeSimplePattern o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _content = o._content;
            else
            {
                _content = new CNode( items );
                CheckContent();
            }
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlTNodeSimplePattern( this, leading, content, trailing );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

        /// <summary>
        /// Gets the optional 'largest' or 'deepest' token.
        /// When null, <see cref="IsLargest"/> is false: the deafult is to match the deepest
        /// patterns. 
        /// </summary>
        public SqlTokenIdentifier LargestOrDeepestT => _content.V1;

        /// <summary>
        /// Gets whether the largest or the deepest patterns must match.
        /// Default to false.
        /// </summary>
        public bool IsLargest => _content.V1 != null && _content.V1.TokenType == SqlTokenType.Largest;

        public SqlTokenIdentifier NodesT => _content.V2;

        public SqlTokenIdentifier LikeT => _content.V3;

        public SqlTRawNodeList RawList => _content.V4;

        public bool Match( ISqlNode n )
        {
            var tokens = n.AllTokens.GetEnumerator();
            var patterns = RawList.AllTokens.Skip(1).Take( RawList.Width-2 ).GetEnumerator();
            try
            {
                if( !tokens.MoveNext() || !patterns.MoveNext() ) return false;

                for(;;)
                {
                    if( patterns.Current.TokenType == SqlTokenType.QuestionMark
                        || tokens.Current.Equals( patterns.Current ) )
                    {
                        if( !patterns.MoveNext() ) return true;
                        if( !tokens.MoveNext() ) return false;
                    }
                    else return false;
                }
            }
            finally
            {
                tokens.Dispose();
                patterns.Dispose();
            }
        }

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

    }

}
