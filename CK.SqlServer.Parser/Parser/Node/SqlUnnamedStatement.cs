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
    /// Captures an unnamed statement: it is only a <see cref="Content"/>.
    /// </summary>
    public sealed class SqlUnnamedStatement : SqlNonTokenAutoWidth, ISqlStatement
    {
        readonly SNode<ISqlNode, SqlTokenTerminal> _content;

        public SqlUnnamedStatement( ISqlNode content, SqlTokenTerminal statementTerminator = null )
            : base( null, null )
        {
            _content = new SNode<ISqlNode, SqlTokenTerminal>( content, statementTerminator );
            CheckContent();
        }

        void CheckContent()
        {
            Helper.CheckNotNull( Content, nameof( Content ) );
        }

        SqlUnnamedStatement( SqlUnnamedStatement o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _content = o._content;
            else
            {
                _content = new SNode<ISqlNode, SqlTokenTerminal>( items );
                CheckContent();
            }
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlUnnamedStatement( this, leading, content, trailing );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

        public ISqlNode Content => _content.V1;

        public SqlTokenTerminal StatementTerminator => _content.V2;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

    }

}
