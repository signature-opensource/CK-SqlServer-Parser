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
    /// The "Order by" operator is a <see cref="ISelectSpecification"/>.
    /// </summary>
    public class SelectOrderBy : SqlNode, ISelectSpecification
    {
        readonly SNode<ISqlNode, SqlTokenIdentifier, SqlTokenIdentifier, SqlOrderByList, SelectOrderByOffset> _content;

        public SelectOrderBy( ISqlNode selectNode, SqlTokenIdentifier orderT, SqlTokenIdentifier byT, SqlOrderByList orderByList, SelectOrderByOffset offset = null )
            : base( null, null )
        {
            _content = new SNode<ISqlNode, SqlTokenIdentifier, SqlTokenIdentifier, SqlOrderByList, SelectOrderByOffset>(
                selectNode, 
                orderT, 
                byT, 
                orderByList, 
                offset );
            CheckContent();
        }

        void CheckContent()
        {
            SNode.CheckUnPar<ISelectSpecification>( SelectNode, nameof( SelectNode ) );
            SNode.CheckToken( OrderT, nameof( OrderT ), SqlTokenType.Order );
            SNode.CheckToken( ByT, nameof( ByT ), SqlTokenType.By );
            SNode.CheckNotNull( OrderByColumns, nameof( OrderByColumns ) );
        }

        SelectOrderBy( SelectOrderBy o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _content = o._content;
            else
            {
                _content = new SNode<ISqlNode, SqlTokenIdentifier, SqlTokenIdentifier, SqlOrderByList, SelectOrderByOffset>( items );
                CheckContent();
            }
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SelectOrderBy( this, leading, children, trailing );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public ISqlNode SelectNode => _content.V1;

        public ISelectSpecification Select => (ISelectSpecification)_content.V1.UnPar;

        public SqlTokenIdentifier OrderT => _content.V2;

        public SqlTokenIdentifier ByT => _content.V3;

        public SqlOrderByList OrderByColumns => _content.V4;

        public SelectOrderByOffset OffsetClause => _content.V5;

        SelectOperatorKind ISelectSpecification.SelectOperator => SelectOperatorKind.OrderBy; 

        SelectColumnList ISelectSpecification.Columns => Select.Columns; 

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor ) => visitor.Visit( this );

    }


}
