using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;

namespace CK.SqlServer.Parser
{
    using CNode = SNode<SqlTokenIdentifier, SqlNodeList>;

    /// <summary>
    /// Captures the ROW or RANGE part of the OVER clause.
    /// See https://docs.microsoft.com/en-us/sql/t-sql/queries/select-over-clause-transact-sql#syntax
    /// </summary>
    public sealed class SqlOverClauseRowOrRange : SqlNonTokenAutoWidth
    {
        readonly CNode _content;

        public SqlOverClauseRowOrRange( SqlTokenIdentifier rowOrRangeT, SqlNodeList windowFrame )
            : base( null, null )
        {
            _content = new CNode( rowOrRangeT, windowFrame );
            CheckContent();
        }

        void CheckContent()
        {
            Helper.CheckToken( RowOrRangeT, nameof( RowOrRangeT ), SqlTokenType.Rows, SqlTokenType.Range );
            Helper.CheckNotNull( WindowFrame, nameof( WindowFrame ) );
        }

        SqlOverClauseRowOrRange( SqlOverClauseRowOrRange o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
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
            return new SqlOverClauseRowOrRange( this, leading, content, trailing );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

        public SqlTokenIdentifier RowOrRangeT => _content.V1;

        /// <summary>
        /// Gets the BETWEEN or [UNBOUNDED]PRECEDING/FOLLOWING window frame as a raw token list. 
        /// </summary>
        public SqlNodeList WindowFrame => _content.V2;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

    }

}
