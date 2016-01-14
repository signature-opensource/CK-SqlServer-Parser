using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Parser
{
    public sealed class SelectOrderBy : SqlNode
    {
        readonly SNode<
                    SqlTokenIdentifier,
                    SqlTokenIdentifier,
                    SqlOrderByList,
                    SqlTokenIdentifier,
                    ISqlNode,
                    SqlTokenIdentifier,
                    SqlTokenIdentifier,
                    SqlTokenIdentifier,
                    ISqlNode,
                    SqlTokenIdentifier,
                    SqlTokenIdentifier> _content;

        public SelectOrderBy(
            SqlTokenIdentifier orderT, SqlTokenIdentifier byT, SqlOrderByList orderByList,
            SqlTokenIdentifier offsetToken, ISqlNode offsetExpr, SqlTokenIdentifier rowsToken )
            : base( null, null )
        {
            _content = new SNode<SqlTokenIdentifier, SqlTokenIdentifier, SqlOrderByList, SqlTokenIdentifier, ISqlNode, SqlTokenIdentifier, SqlTokenIdentifier, SqlTokenIdentifier, ISqlNode, SqlTokenIdentifier, SqlTokenIdentifier>(
                orderT, byT, orderByList,
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

        public SelectOrderBy( SqlTokenIdentifier orderT, SqlTokenIdentifier byT, SqlOrderByList orderByList, 
                                    SqlTokenIdentifier offsetToken, ISqlNode offsetExpr, SqlTokenIdentifier rowsToken,
                                    SqlTokenIdentifier fetchToken, SqlTokenIdentifier firstOrNextToken, ISqlNode fetchExpr, SqlTokenIdentifier fetchRowsToken, SqlTokenIdentifier onlyToken )
            : base( null, null )
        {
            _content = new SNode<SqlTokenIdentifier, SqlTokenIdentifier, SqlOrderByList, SqlTokenIdentifier, ISqlNode, SqlTokenIdentifier, SqlTokenIdentifier, SqlTokenIdentifier, ISqlNode, SqlTokenIdentifier, SqlTokenIdentifier>(
                orderT, byT, orderByList,
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
            SNode.CheckToken( OrderT, nameof( OrderT ), SqlTokenType.Order );
            SNode.CheckToken( ByT, nameof( ByT ), SqlTokenType.By );
            SNode.CheckNotNull( OrderByColumns, nameof( OrderByColumns ) );

            SNode.CheckNullableToken( OffsetT, nameof( OffsetT ), SqlTokenType.Offset );
            SNode.CheckBothNullOrNot( OffsetT, nameof( OffsetT ), OffsetExpression, nameof( OffsetExpression ) );
            SNode.CheckNullableToken( RowsT, nameof( RowsT ), SqlTokenType.Rows );
            SNode.CheckBothNullOrNot( OffsetT, nameof( OffsetT ), RowsT, nameof( RowsT ) );
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

        SelectOrderBy( SelectOrderBy o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _content = o._content;
            else
            {
                _content = new SNode<SqlTokenIdentifier, SqlTokenIdentifier, SqlOrderByList, SqlTokenIdentifier, ISqlNode, SqlTokenIdentifier, SqlTokenIdentifier, SqlTokenIdentifier, ISqlNode, SqlTokenIdentifier, SqlTokenIdentifier>( items );
                CheckContent();
            }
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SelectOrderBy( this, leading, children, trailing );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public SqlTokenIdentifier OrderT => _content.V1;

        public SqlTokenIdentifier ByT => _content.V2;

        public SqlOrderByList OrderByColumns => _content.V3;

        public bool HasOffset => _content.V4 != null;

        public SqlTokenIdentifier OffsetT => _content.V4;

        public ISqlNode OffsetExpression => _content.V5;

        public SqlTokenIdentifier RowsT => _content.V6;
    
        public bool HasFetch => _content.V7 != null;

        public SqlTokenIdentifier FetchT => _content.V7;

        public SqlTokenIdentifier FetchFirstOrNextT => _content.V8;

        public ISqlNode FetchExpression => _content.V9;

        public SqlTokenIdentifier FetchRowsT => _content.V10;

        public SqlTokenIdentifier FetchOnlyT => _content.V11;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor ) => visitor.Visit( this );

    }
}