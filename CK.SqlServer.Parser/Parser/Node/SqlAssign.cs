using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;

namespace CK.SqlServer.Parser
{
    public sealed class SqlAssign : SqlNode
    {
        readonly SNode<ISqlNode, SqlTokenTerminal, ISqlNode> _content;

        public SqlAssign( ISqlNode left, SqlTokenTerminal operatorT, ISqlNode right )
            : base( null, null )
        {
            _content = new SNode<ISqlNode, SqlTokenTerminal, ISqlNode>( left, operatorT, right );
            CheckContent();
        }

        void CheckContent()
        {
            SNode.CheckNotNull( Left, nameof( Left ) );
            SNode.CheckToken( Operator, nameof( Operator ), IsValidAssignOperator );
            SNode.CheckNotNull( Right, nameof( Right ) );
        }

        static bool IsValidAssignOperator( SqlTokenType tokenType )
        {
            return (tokenType & SqlTokenType.IsAssignOperator) != 0;
        }

        SqlAssign( SqlAssign o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _content = o._content;
            else
            {
                _content = new SNode<ISqlNode, SqlTokenTerminal, ISqlNode>( items );
                CheckContent();
            }
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlAssign( this, leading, children, trailing );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public ISqlNode Left => _content.V1;

        public SqlTokenTerminal Operator => _content.V2;

        public ISqlNode Right => _content.V3;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor ) => visitor.Visit( this );

    }
}
