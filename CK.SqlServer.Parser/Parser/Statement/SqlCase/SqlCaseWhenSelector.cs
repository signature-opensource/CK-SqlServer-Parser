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
    using CNode = SNode<SqlTokenIdentifier, ISqlNode, SqlTokenIdentifier, ISqlNode>;

    /// <summary>
    /// Defines "when Expression then Value" items of <see cref="SqlCase"/> expression.
    /// </summary>
    public sealed class SqlCaseWhenSelector : SqlNonToken
    {
        readonly CNode _content;

        public SqlCaseWhenSelector( SqlTokenIdentifier whenT, ISqlNode expression, SqlTokenIdentifier thenT, ISqlNode value )
            : base( null, null )
        {
            _content = new CNode( whenT, expression, thenT, value );
            CheckContent();
        }

        void CheckContent()
        {
            Helper.CheckToken( WhenT, nameof( WhenT ), SqlTokenType.When );
            Helper.CheckNotNull( Expression, nameof( Expression ) );
            Helper.CheckToken( ThenT, nameof( ThenT ), SqlTokenType.Then );
            Helper.CheckNotNull( Value, nameof( Value ) );
        }

        SqlCaseWhenSelector( SqlCaseWhenSelector o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _content = o._content;
            else
            {
                _content = new CNode( items );
                CheckContent();
            }
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlCaseWhenSelector( this, leading, content, trailing );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

        public SqlTokenIdentifier WhenT => _content.V1;

        public ISqlNode Expression => _content.V2;

        public SqlTokenIdentifier ThenT => _content.V3;

        public ISqlNode Value => _content.V4;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

    }


}
