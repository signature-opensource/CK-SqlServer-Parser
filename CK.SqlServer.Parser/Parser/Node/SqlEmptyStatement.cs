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
    /// An isolated statement terminator ; is valid.
    /// </summary>
    public sealed class SqlEmptyStatement : SqlNode, ISqlNamedStatement
    {
        readonly SNode<SqlTokenTerminal> _content;

        public SqlEmptyStatement( SqlTokenTerminal statementTerminator )
            : base( null, null )
        {
            _content = new SNode<SqlTokenTerminal>( statementTerminator );
            CheckContent();
        }

        SqlEmptyStatement( SqlEmptyStatement o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _content = o._content;
            else
            {
                _content = new SNode<SqlTokenTerminal>( items );
                CheckContent();
            }
        }

        void CheckContent()
        {
            SNode.CheckNotNull( StatementTerminator, nameof( StatementTerminator ) );
        }

        public StatementName StatementName => StatementName.None;

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public SqlTokenTerminal StatementTerminator => _content.V;

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IReadOnlyList<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlEmptyStatement( this, leading, children, trailing );
        }

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor ) => visitor.Visit( this );

    }


}
