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
    public sealed class SqlExecuteAs : SqlNode
    {
        readonly SNode<SqlTokenIdentifier, SqlTokenIdentifier, SqlToken> _content;

        public SqlExecuteAs( SqlTokenIdentifier execToken, SqlTokenIdentifier asToken, SqlToken userSpec )
            : base( null, null )
        {
            _content = new SNode<SqlTokenIdentifier, SqlTokenIdentifier, SqlToken>( execToken, asToken, userSpec );
            CheckContent();
        }

        void CheckContent()
        {
            SNode.CheckToken( ExecT, nameof( ExecT ), SqlTokenType.Execute );
            SNode.CheckToken( AsT, nameof( AsT ), SqlTokenType.As );
            SNode.CheckNotNull( UserSpec, nameof( UserSpec ) );
        }

        SqlExecuteAs( SqlExecuteAs o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _content = o._content;
            else
            {
                _content = new SNode<SqlTokenIdentifier, SqlTokenIdentifier, SqlToken>( items );
                CheckContent();
            }
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IReadOnlyList<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlExecuteAs( this, leading, children, trailing );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public SqlTokenIdentifier ExecT => _content.V1;

        public SqlTokenIdentifier AsT => _content.V2;

        public SqlToken UserSpec => _content.V3;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor ) => visitor.Visit( this );

    }

}
