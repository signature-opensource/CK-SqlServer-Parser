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
    using CNode = SNode<ISqlIdentifier, ISqlNode, SqlTokenTerminal>;

    /// <summary>
    /// Captures any statement: it is a <see cref="Name"/> and a non empty <see cref="Content"/> 
    /// (a <see cref="SqlNodeList"/>).
    /// </summary>
    public sealed class SqlStatement : SqlNonTokenAutoWidth, ISqlNamedStatement
    {
        readonly CNode _content;

        public SqlStatement( ISqlIdentifier name, ISqlNode content, SqlTokenTerminal statementTerminator = null )
            : base( null, null )
        {
            _content = new CNode( name, content, statementTerminator );
            CheckContent();
        }

        void CheckContent()
        {
            Helper.CheckNotNull( Name, nameof( Name ) );
        }

        SqlStatement( SqlStatement o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
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
            return new SqlStatement( this, leading, content, trailing );
        }

        public StatementKnownName StatementKnownName => StatementKnownName.Unknown;

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

        public ISqlIdentifier Name => _content.V1;

        public ISqlNode Content => _content.V2;

        public SqlTokenTerminal StatementTerminator => _content.V3;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

    }

}
