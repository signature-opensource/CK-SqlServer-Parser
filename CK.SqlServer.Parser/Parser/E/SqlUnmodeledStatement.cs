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
    /// Captures any statement: it can be any <see cref="ISqlNode"/>.
    /// </summary>
    public sealed class SqlUnmodeledStatement : SqlNode, ISqlStatement
    {
        readonly SNode<ISqlNode, SqlTokenTerminal> _content;

        public SqlUnmodeledStatement( ISqlNode content, SqlTokenTerminal statementTerminator = null )
            : base( null, null )
        {
            _content = new SNode<ISqlNode, SqlTokenTerminal>( content, statementTerminator );
            CheckContent();
        }

        void CheckContent()
        {
            SNode.CheckNotNull( Content, nameof( Content ) );
        }

        SqlUnmodeledStatement( SqlUnmodeledStatement o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _content = o._content;
            else
            {
                _content = new SNode<ISqlNode, SqlTokenTerminal>( items );
                CheckContent();
            }
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IReadOnlyList<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlUnmodeledStatement( this, leading, children, trailing );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public ISqlNode Content => _content.V1;

        public SqlTokenTerminal StatementTerminator => _content.V2;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor ) => visitor.Visit( this );

    }

}
