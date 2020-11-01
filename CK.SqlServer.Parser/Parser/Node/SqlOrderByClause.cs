using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Parser
{
    using CNode = SNode<SqlTokenIdentifier,
                        SqlTokenIdentifier,
                        SqlOrderByList>;

    /// <summary>
    /// This is the simple order by clause without "offset_fetch" extension.
    /// See https://docs.microsoft.com/en-us/sql/t-sql/queries/select-order-by-clause-transact-sql#syntax
    /// and https://docs.microsoft.com/en-us/sql/t-sql/queries/select-over-clause-transact-sql#syntax.
    /// The <see cref="SelectDecorator"/> uses the <see cref="SelectOrderBy"/> (that handles the "offset_fetch" extension)
    /// and the <see cref="SqlOverClause"/> directly uses this.
    /// </summary>
    public sealed class SqlOrderByClause : SqlNonTokenAutoWidth
    {
        readonly CNode _content;

        public SqlOrderByClause(
            SqlTokenIdentifier orderT, SqlTokenIdentifier byT, SqlOrderByList orderByList )
            : base( null, null )
        {
            _content = new CNode( orderT, byT, orderByList );
            CheckContent();
        }

        void CheckContent()
        {
            Helper.CheckToken( OrderT, nameof( OrderT ), SqlTokenType.Order );
            Helper.CheckToken( ByT, nameof( ByT ), SqlTokenType.By );
            Helper.CheckNotNull( OrderByColumns, nameof( OrderByColumns ) );
        }

        SqlOrderByClause( SqlOrderByClause o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
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
            return new SqlOrderByClause( this, leading, content, trailing );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

        public SqlTokenIdentifier OrderT => _content.V1;

        public SqlTokenIdentifier ByT => _content.V2;

        public SqlOrderByList OrderByColumns => _content.V3;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

    }
}
