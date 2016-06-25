using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using CK.Core;
using System.Collections.Immutable;

namespace CK.SqlServer.Parser
{
    public sealed class SqlUnaryOperator : SqlNonToken
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
            Helper.CheckToken( Operator, nameof( Operator ), 
                SqlTokenType.Not, 
                SqlTokenType.BitwiseNot, 
                SqlTokenType.Plus, 
                SqlTokenType.Minus );
            Helper.CheckNotNull( Right, nameof( Right ) );
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

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlUnaryOperator( this, leading, content, trailing );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

        public SqlToken Operator => _content.V1;

        public ISqlNode Right => _content.V2;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );
    }
}
