using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace CK.SqlServer.Parser
{
    /// <summary>
    /// Captures 'into', 'from', 'where' and 'group by' clauses. The <see cref="SelectDecorator"/> and <see cref="SelectFor"/>
    /// as well as the <see cref="SelectCombine"/> are operators that wraps other <see cref="ISelectSpecification"/>.
    /// </summary>
    public sealed class SelectSpec : SqlNonToken, ISelectSpecification, ISqlStatementPart
    {
        readonly SNode<SelectHeader, SelectColumnList, SelectInto, SelectFrom, SqlTokenIdentifier, ISqlNode, SelectGroupBy> _content;

        public SelectSpec( SelectHeader header, SelectColumnList columns, SelectInto into = null, SelectFrom from = null, SqlTokenIdentifier whereT = null, ISqlNode whereExpression = null, SelectGroupBy groupBy = null )
            : base( null, null )
        {
            SqlTrivia.WhiteSpaceToLeft( ref header, ref columns );
            _content = new SNode<SelectHeader, SelectColumnList, SelectInto, SelectFrom, SqlTokenIdentifier, ISqlNode, SelectGroupBy>(
                header, 
                columns, 
                into, 
                from, 
                whereT,
                whereExpression, 
                groupBy );
            CheckContent();
        }

        void CheckContent()
        {
            Helper.CheckNotNull( Header, nameof( Header ) );
            Helper.CheckNotNull( Columns, nameof( Columns ) );
            Helper.CheckNullableToken( WhereT, nameof( WhereT ), SqlTokenType.Where );
            Helper.CheckBothNullOrNot( WhereT, nameof( WhereT ), WhereExpression, nameof( WhereExpression ) );
        }

        SelectSpec( SelectSpec o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _content = o._content;
            else
            {
                _content = new SNode<SelectHeader, SelectColumnList, SelectInto, SelectFrom, SqlTokenIdentifier, ISqlNode, SelectGroupBy>( items );
                CheckContent();
            }
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return new SelectSpec( this, leading, content, trailing );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

        /// <summary>
        /// Gets the operator token type: it is <see cref="SelectOperatorKind.None"/> since this is an 
        /// actual select specification and not an operator like a <see cref="SelectCombine"/> or 
        /// a <see cref="SelectDecorator"/>.
        /// </summary>
        public SelectOperatorKind SelectOperator => SelectOperatorKind.None;

        public SelectHeader Header => _content.V1;

        public SelectColumnList Columns => _content.V2;

        public SelectSpec InsertColumn( int idx, SelectColumn column ) => this.ReplaceContentNode( 1, Columns.InsertAt( idx, column ) );

        public SelectSpec InsertColumn( int idx, ISqlNode definition, SqlTokenIdentifier alias = null )
        {
            if( definition == null ) throw new ArgumentNullException( nameof( definition ) );
            if( alias == null ) return InsertColumn( idx, new SelectColumn( definition ) );
            SqlToken tSep = Columns.Where( col => col.AsOrEqualT != null ).Select( col => col.AsOrEqualT ).FirstOrDefault()
                            ?? SqlKeyword.Assign;
            return InsertColumn( idx, tSep.IsToken( SqlTokenType.Assign ) 
                                        ? new SelectColumn( alias, (SqlTokenTerminal)tSep, definition )
                                        : new SelectColumn( definition, (SqlTokenIdentifier)tSep, alias ) );
        }

        public SelectInto IntoClause => _content.V3;

        public SelectFrom FromClause => _content.V4;

        public SqlTokenIdentifier WhereT => _content.V5;

        public ISqlNode WhereExpression => _content.V6;

        public SelectGroupBy GroupByClause => _content.V7;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

    }
}
