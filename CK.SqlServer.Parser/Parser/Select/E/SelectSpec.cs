using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace CK.SqlServer.Parser
{
    /// <summary>
    /// Captures 'into', 'from', 'where' and 'group by' clauses. The <see cref="SelectOrderBy"/> and <see cref="SelectFor"/>
    /// as well as the <see cref="SelectCombine"/> are operators that wraps other <see cref="ISelectSpecification"/>.
    /// </summary>
    public sealed class SelectSpec : SqlNode, ISelectSpecification
    {
        readonly SNode<SelectHeader, SelectColumnList, SelectInto, SelectFrom, SelectWhere, SelectGroupBy> _content;

        public SelectSpec( SelectHeader header, SelectColumnList columns, SelectInto into = null, SelectFrom from = null, SelectWhere where = null, SelectGroupBy groupBy = null )
            : base( null, null )
        {
            _content = new SNode<SelectHeader, SelectColumnList, SelectInto, SelectFrom, SelectWhere, SelectGroupBy>(
                header, 
                columns, 
                into, 
                from, 
                where, 
                groupBy );
            CheckContent();
        }

        void CheckContent()
        {
            SNode.CheckNotNull( Header, nameof( Header ) );
            SNode.CheckNotNull( Columns, nameof( Columns ) );
        }

        SelectSpec( SelectSpec o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _content = o._content;
            else
            {
                _content = new SNode<SelectHeader, SelectColumnList, SelectInto, SelectFrom, SelectWhere, SelectGroupBy>( items );
                CheckContent();
            }
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SelectSpec( this, leading, children, trailing );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        /// <summary>
        /// Gets the operator token type: it is <see cref="SelectOperatorKind.None"/> since this is an 
        /// actual select specification and not an operator like <see cref="SelectCombine"/>, 
        /// <see cref="SelectOrderBy"/> or <see cref="SelectFor"/>.
        /// </summary>
        public SelectOperatorKind SelectOperator => SelectOperatorKind.None;

        public SelectHeader Header => _content.V1;

        public SelectColumnList Columns => _content.V2;

        public SelectInto IntoClause => _content.V3;

        public SelectFrom FromClause => _content.V4;

        public SelectWhere WhereClause => _content.V5;

        public SelectGroupBy GroupByClause => _content.V6;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor ) => visitor.Visit( this );

    }
}
