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
    /// <summary>
    ///  "Order by" operator.
    /// </summary>
    public class SelectOrderBy : SqlNode, ISelectSpecification
    {
        readonly SNode<ISelectSpecification, SqlTokenIdentifier, SqlTokenIdentifier, SqlOrderByList, SelectOrderByOffset> _content;

        public SelectOrderBy( ISelectSpecification select, SqlTokenIdentifier orderT, SqlTokenIdentifier byT, SqlOrderByList columns, SelectOrderByOffset offset = null )
            : base( null, null )
        {
            _content = new SNode<ISelectSpecification, SqlTokenIdentifier, SqlTokenIdentifier, SqlOrderByList, SelectOrderByOffset>(
                select, 
                orderT, 
                byT, 
                columns, 
                offset );
            CheckContent();
        }

        void CheckContent()
        {
            SNode.CheckNotNull( Select, nameof( Select ) );
            SNode.CheckToken( OrderT, nameof( OrderT ), SqlTokenType.Order );
            SNode.CheckToken( ByT, nameof( ByT ), SqlTokenType.By );
            SNode.CheckNotNull( Columns, nameof( Columns ) );
        }

        SelectOrderBy( SelectOrderBy o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _content = o._content;
            else
            {
                _content = new SNode<ISelectSpecification, SqlTokenIdentifier, SqlTokenIdentifier, SqlOrderByList, SelectOrderByOffset>( items );
                CheckContent();
            }
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IReadOnlyList<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SelectOrderBy( this, leading, children, trailing );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public ISelectSpecification Select => _content.V1;

        public SqlTokenIdentifier OrderT => _content.V2;

        public SqlTokenIdentifier ByT => _content.V3;

        public SqlOrderByList OrderByColumns => _content.V4;

        public SelectOrderByOffset OffsetClause => _content.V5;

        public SqlTokenType CombinationKind => SqlTokenType.Order; 

        public SelectColumnList Columns => Select.Columns; 

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor ) => visitor.Visit( this );

    }


}
