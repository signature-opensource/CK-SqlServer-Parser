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
    /// Defines "when Expression then Value" items of <see cref="SqlCase"/> expression.
    /// </summary>
    public sealed class SqlCaseWhenSelector : SqlNode
    {
        readonly SNode<SqlTokenIdentifier, ISqlNode, SqlTokenIdentifier, ISqlNode> _content;

        public SqlCaseWhenSelector( SqlTokenIdentifier whenT, ISqlNode expression, SqlTokenIdentifier thenT, ISqlNode value )
            : base( null, null )
        {
            _content = new SNode<SqlTokenIdentifier, ISqlNode, SqlTokenIdentifier, ISqlNode>( whenT, expression, thenT, value );
            CheckContent();
        }

        void CheckContent()
        {
            SNode.CheckToken( WhenT, nameof( WhenT ), SqlTokenType.When );
            SNode.CheckNotNull( Expression, nameof( Expression ) );
            SNode.CheckToken( ThenT, nameof( ThenT ), SqlTokenType.Then );
            SNode.CheckNotNull( Value, nameof( Value ) );
        }

        SqlCaseWhenSelector( SqlCaseWhenSelector o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _content = o._content;
            else
            {
                _content = new SNode<SqlTokenIdentifier, ISqlNode, SqlTokenIdentifier, ISqlNode>( items );
                CheckContent();
            }
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlCaseWhenSelector( this, leading, children, trailing );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public SqlTokenIdentifier WhenT => _content.V1;

        public ISqlNode Expression => _content.V2;

        public SqlTokenIdentifier ThenT => _content.V3;

        public ISqlNode Value => _content.V4;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor ) => visitor.Visit( this );

    }


}
