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
    /// This can be also be used with a null terminator: this becomes the "empty node".
    /// </summary>
    public sealed class SqlEmptyStatement : SqlNonToken, ISqlNamedStatement
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
            Helper.CheckNotNull( StatementTerminator, nameof( StatementTerminator ) );
        }

        public StatementKnownName StatementKnownName => StatementKnownName.Empty;

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

        public SqlTokenTerminal StatementTerminator => _content.V;

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlEmptyStatement( this, leading, content, trailing );
        }

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

    }


}
