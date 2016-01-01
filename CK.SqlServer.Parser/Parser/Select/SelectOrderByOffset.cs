using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Parser
{
    public sealed class SelectOrderByOffset : SqlNode
    {
        readonly SNode<
                    SqlTokenIdentifier,
                    ISqlNode,
                    SqlTokenIdentifier,
                    SqlTokenIdentifier,
                    SqlTokenIdentifier,
                    ISqlNode,
                    SqlTokenIdentifier,
                    SqlTokenIdentifier> _content;

        public SelectOrderByOffset( SqlTokenIdentifier offsetToken, ISqlNode offsetExpr, SqlTokenIdentifier rowsToken )
            : base( null, null )
        {
            _content = new SNode<SqlTokenIdentifier, ISqlNode, SqlTokenIdentifier, SqlTokenIdentifier, SqlTokenIdentifier, ISqlNode, SqlTokenIdentifier, SqlTokenIdentifier>(
                offsetToken,
                offsetExpr,
                rowsToken,
                null,
                null,
                null,
                null,
                null
                );
            CheckContent();
        }

        public SelectOrderByOffset( SqlTokenIdentifier offsetToken, ISqlNode offsetExpr, SqlTokenIdentifier rowsToken,
                                    SqlTokenIdentifier fetchToken, SqlTokenIdentifier firstOrNextToken, ISqlNode fetchExpr, SqlTokenIdentifier fetchRowsToken, SqlTokenIdentifier onlyToken )
            : base( null, null )
        {
            _content = new SNode<SqlTokenIdentifier, ISqlNode, SqlTokenIdentifier, SqlTokenIdentifier, SqlTokenIdentifier, ISqlNode, SqlTokenIdentifier, SqlTokenIdentifier>(
                offsetToken, 
                offsetExpr, 
                rowsToken, 
                fetchToken, 
                firstOrNextToken, 
                fetchExpr, 
                fetchRowsToken, 
                onlyToken );
            CheckContent();
        }

        void CheckContent()
        {
            SNode.CheckToken( OffsetT, nameof( OffsetT ), SqlTokenType.Offset );
            SNode.CheckNotNull( OffsetExpression, nameof( OffsetExpression ) );
            SNode.CheckToken( RowsT, nameof( RowsT ), SqlTokenType.Rows );
            SNode.CheckNullableToken( FetchT, nameof( RowsT ), SqlTokenType.Fetch );
            if( FetchT != null )
            {
                SNode.CheckToken( FetchFirstOrNextT, nameof( FetchFirstOrNextT ), SqlTokenType.First, SqlTokenType.Next );
                SNode.CheckNotNull( FetchExpression, nameof( FetchExpression ) );
                SNode.CheckToken( FetchRowsT, nameof( FetchRowsT ), SqlTokenType.Rows );
                SNode.CheckToken( FetchOnlyT, nameof( FetchOnlyT ), SqlTokenType.Only );

            }
            else
            {
                SNode.CheckNull( FetchFirstOrNextT, nameof( FetchFirstOrNextT ) );
                SNode.CheckNull( FetchExpression, nameof( FetchExpression ) );
                SNode.CheckNull( FetchRowsT, nameof( FetchRowsT ) );
                SNode.CheckNull( FetchOnlyT, nameof( FetchOnlyT ) );
            }
        }

        SelectOrderByOffset( SelectOrderByOffset o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _content = o._content;
            else
            {
                _content = new SNode<SqlTokenIdentifier, ISqlNode, SqlTokenIdentifier, SqlTokenIdentifier, SqlTokenIdentifier, ISqlNode, SqlTokenIdentifier, SqlTokenIdentifier>( items );
                CheckContent();
            }
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SelectOrderByOffset( this, leading, children, trailing );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public SqlTokenIdentifier OffsetT => _content.V1;

        public ISqlNode OffsetExpression => _content.V2;

        public SqlTokenIdentifier RowsT => _content.V3;
    
        public bool HasFetchClause => _content.V4 != null;

        public SqlTokenIdentifier FetchT => _content.V4;

        public SqlTokenIdentifier FetchFirstOrNextT => _content.V5;

        public ISqlNode FetchExpression => _content.V6;

        public SqlTokenIdentifier FetchRowsT => _content.V7;

        public SqlTokenIdentifier FetchOnlyT => _content.V8;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor ) => visitor.Visit( this );

    }
}