using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Parser
{
    using CNode = SNode<SqlOrderByClause,
                        SqlTokenIdentifier,
                        ISqlNode,
                        SqlTokenIdentifier,
                        SqlTokenIdentifier,
                        SqlTokenIdentifier,
                        ISqlNode,
                        SqlTokenIdentifier,
                        SqlTokenIdentifier>;

    /// <summary>
    /// See https://docs.microsoft.com/en-us/sql/t-sql/queries/select-order-by-clause-transact-sql#syntax
    /// This is a child of the <see cref="SelectDecorator"/> that handles order by clauses and "offset_fetch" extension.
    /// </summary>
    public sealed class SelectOrderBy : SqlNonTokenAutoWidth
    {
        readonly CNode _content;

        public SelectOrderBy( SqlOrderByClause orderBy,SqlTokenIdentifier offsetToken, ISqlNode offsetExpr, SqlTokenIdentifier rowsToken )
            : base( null, null )
        {
            _content = new CNode( orderBy,
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

        public SelectOrderBy( SqlOrderByClause orderBy, 
                              SqlTokenIdentifier offsetToken, ISqlNode offsetExpr, SqlTokenIdentifier rowsToken,
                              SqlTokenIdentifier fetchToken, SqlTokenIdentifier firstOrNextToken, ISqlNode fetchExpr, SqlTokenIdentifier fetchRowsToken, SqlTokenIdentifier onlyToken )
            : base( null, null )
        {
            _content = new CNode( orderBy,
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
            Helper.CheckNullableToken( OffsetT, nameof( OffsetT ), SqlTokenType.Offset );
            Helper.CheckBothNullOrNot( OffsetT, nameof( OffsetT ), OffsetExpression, nameof( OffsetExpression ) );
            Helper.CheckNullableToken( RowsT, nameof( RowsT ), SqlTokenType.Rows );
            Helper.CheckBothNullOrNot( OffsetT, nameof( OffsetT ), RowsT, nameof( RowsT ) );
            Helper.CheckNullableToken( FetchT, nameof( RowsT ), SqlTokenType.Fetch );
            if( FetchT != null )
            {
                Helper.CheckToken( FetchFirstOrNextT, nameof( FetchFirstOrNextT ), SqlTokenType.First, SqlTokenType.Next );
                Helper.CheckNotNull( FetchExpression, nameof( FetchExpression ) );
                Helper.CheckToken( FetchRowsT, nameof( FetchRowsT ), SqlTokenType.Rows );
                Helper.CheckToken( FetchOnlyT, nameof( FetchOnlyT ), SqlTokenType.Only );

            }
            else
            {
                Helper.CheckNull( FetchFirstOrNextT, nameof( FetchFirstOrNextT ) );
                Helper.CheckNull( FetchExpression, nameof( FetchExpression ) );
                Helper.CheckNull( FetchRowsT, nameof( FetchRowsT ) );
                Helper.CheckNull( FetchOnlyT, nameof( FetchOnlyT ) );
            }
        }

        SelectOrderBy( SelectOrderBy o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
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
            return new SelectOrderBy( this, leading, content, trailing );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

        public SqlOrderByClause OrderByClause => _content.V1;

        public bool HasOffset => _content.V2 != null;

        public SqlTokenIdentifier OffsetT => _content.V2;

        public ISqlNode OffsetExpression => _content.V3;

        public SqlTokenIdentifier RowsT => _content.V4;
    
        public bool HasFetch => _content.V5 != null;

        public SqlTokenIdentifier FetchT => _content.V5;

        public SqlTokenIdentifier FetchFirstOrNextT => _content.V6;

        public ISqlNode FetchExpression => _content.V7;

        public SqlTokenIdentifier FetchRowsT => _content.V8;

        public SqlTokenIdentifier FetchOnlyT => _content.V9;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

    }
}
