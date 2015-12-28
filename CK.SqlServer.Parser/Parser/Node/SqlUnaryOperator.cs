using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using CK.Core;
using System.Collections.Immutable;

namespace CK.SqlServer.Parser
{
    public sealed class SqlUnaryOperator : SqlNode
    {
        readonly SNode<SqlToken, ISqlNode> _content;

        public SqlUnaryOperator( SqlToken op, ISqlNode right )
            : base( null, null )
        {
            _content = new SNode<SqlToken, ISqlNode>( op, right );
            CheckContent();
        }

        void CheckContent()
        {
            SNode.CheckToken( Operator, nameof( Operator ), 
                SqlTokenType.Not, 
                SqlTokenType.BitwiseNot, 
                SqlTokenType.Plus, 
                SqlTokenType.Minus );
            SNode.CheckNotNull( Right, nameof( Right ) );
        }

        SqlUnaryOperator( SqlUnaryOperator o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _content = o._content;
            else
            {
                _content = new SNode<SqlToken, ISqlNode>( items );
                CheckContent();
            }
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IReadOnlyList<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlUnaryOperator( this, leading, children, trailing );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public SqlToken Operator => _content.V1;

        public ISqlNode Right => _content.V2;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor ) => visitor.Visit( this );
    }
}
