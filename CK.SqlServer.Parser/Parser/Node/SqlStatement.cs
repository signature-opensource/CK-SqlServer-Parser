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
    /// Captures any statement: it is a <see cref="Name"/> and a non empty <see cref="Content"/> 
    /// (a <see cref="SqlNodeList"/>).
    /// </summary>
    public sealed class SqlStatement : SqlNode, ISqlNamedStatement
    {
        readonly SNode<ISqlIdentifier, ISqlNode, SqlTokenTerminal> _content;

        public SqlStatement( ISqlIdentifier name, ISqlNode content, SqlTokenTerminal statementTerminator = null )
            : base( null, null )
        {
            _content = new SNode<ISqlIdentifier, ISqlNode, SqlTokenTerminal>( name, content, statementTerminator );
            CheckContent();
        }

        void CheckContent()
        {
            SNode.CheckNotNull( Name, nameof( Name ) );
        }

        SqlStatement( SqlStatement o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _content = o._content;
            else
            {
                _content = new SNode<ISqlIdentifier, ISqlNode, SqlTokenTerminal>( items );
                CheckContent();
            }
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IReadOnlyList<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlStatement( this, leading, children, trailing );
        }

        public StatementName StatementName => StatementName.Statement;

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public ISqlIdentifier Name => _content.V1;

        public ISqlNode Content => _content.V2;

        public SqlTokenTerminal StatementTerminator => _content.V3;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor ) => visitor.Visit( this );

    }

}
