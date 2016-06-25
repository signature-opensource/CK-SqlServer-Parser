using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;

namespace CK.SqlServer.Parser
{
    public sealed class SqlAssign : SqlNonToken
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
            Helper.CheckNotNull( Left, nameof( Left ) );
            Helper.CheckToken( Operator, nameof( Operator ), IsValidAssignOperator );
            Helper.CheckNotNull( Right, nameof( Right ) );
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

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlAssign( this, leading, content, trailing );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

        public ISqlNode Left => _content.V1;

        public SqlTokenTerminal Operator => _content.V2;

        public ISqlNode Right => _content.V3;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

    }
}
