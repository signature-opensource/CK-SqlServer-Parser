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
    /// Mono identifier statements are "continue" or "break".
    /// </summary>
    public sealed class SqlMonoStatement : SqlNode, ISqlStatement
    {
        readonly SNode<SqlTokenIdentifier, SqlTokenTerminal> _content;

        public SqlMonoStatement( SqlTokenIdentifier id, SqlTokenTerminal statementTerminator = null )
            : base( null, null )
        {
            _content = new SNode<SqlTokenIdentifier, SqlTokenTerminal>( id, statementTerminator );
            CheckContent();
        }

        void CheckContent()
        {
            SNode.CheckNotNull( IdentifierT, nameof( IdentifierT ) );
        }

        SqlMonoStatement( SqlMonoStatement o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _content = o._content;
            else
            {
                _content = new SNode<SqlTokenIdentifier, SqlTokenTerminal>( items );
                CheckContent();
            }
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IReadOnlyList<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlMonoStatement( this, leading, children, trailing );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public SqlTokenIdentifier IdentifierT => _content.V1;

        public SqlTokenTerminal StatementTerminator => _content.V2;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor ) => visitor.Visit( this );

    }


}
