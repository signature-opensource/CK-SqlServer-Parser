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
        readonly SNode<ISqlIdentifier, SqlTokenTerminal, ISqlNode> _content;

        public SqlAssign( ISqlIdentifier identifier, SqlTokenTerminal assignT, ISqlNode right )
            : base( null, null )
        {
            _content = new SNode<ISqlIdentifier, SqlTokenTerminal, ISqlNode>( identifier, assignT, right );
            CheckContent();
        }

        void CheckContent()
        {
            SNode.CheckNotNull( Identifier, nameof( Identifier ) );
            SNode.CheckToken( AssignT, nameof( AssignT ), IsValidAssignOperator );
            SNode.CheckNotNull( Right, nameof( Right ) );
        }

        static bool IsValidAssignOperator( SqlTokenType tokenType )
        {
            return (tokenType & SqlTokenType.IsAssignOperator) == 0;
        }

        SqlAssign( SqlAssign o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _content = o._content;
            else
            {
                _content = new SNode<ISqlIdentifier, SqlTokenTerminal, ISqlNode>( items );
                CheckContent();
            }
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IReadOnlyList<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlAssign( this, leading, children, trailing );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public ISqlIdentifier Identifier => _content.V1;

        public SqlTokenTerminal AssignT => _content.V2;

        public ISqlNode Right => _content.V3;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor ) => visitor.Visit( this );

    }
}
