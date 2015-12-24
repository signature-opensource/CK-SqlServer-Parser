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
    /// Select "For" operator.
    /// </summary>
    public sealed class SelectFor : SqlNode, ISelectSpecification
    {
        readonly SNode<ISelectSpecification, SqlTokenIdentifier, ISqlNode> _content;

        public SelectFor( ISelectSpecification select, SqlTokenIdentifier forToken, ISqlNode forExpression )
            : base( null, null )
        {
            _content = new SNode<ISelectSpecification, SqlTokenIdentifier, ISqlNode>( select, forToken, forExpression );
            CheckContent();
        }

        void CheckContent()
        {
            SNode.CheckNotNull( Select, nameof( Select ) );
            SNode.CheckToken( ForT, nameof( ForT ), SqlTokenType.For );
            SNode.CheckNotNull( ForExpression, nameof( ForExpression ) );
        }

        SelectFor( SelectFor o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _content = o._content;
            else
            {
                _content = new SNode<ISelectSpecification, SqlTokenIdentifier, ISqlNode>( items );
                CheckContent();
            }

        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IReadOnlyList<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SelectFor( this, leading, children, trailing );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public ISelectSpecification Select => _content.V1;

        public SqlTokenIdentifier ForT => _content.V2;

        public ISqlNode ForExpression => _content.V3;

        public SqlTokenType CombinationKind => SqlTokenType.For;

        public SelectColumnList Columns => Select.Columns; 

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor ) => visitor.Visit( this );

    }


}
