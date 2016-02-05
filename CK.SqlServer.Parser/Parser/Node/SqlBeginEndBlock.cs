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
    /// A block is defined by begin...end enclosing keywords.
    /// </summary>
    public sealed class SqlBeginEndBlock : SqlNode, ISqlNamedStatement
    {
        readonly SNode<SqlTokenIdentifier, SqlStatementList, SqlTokenIdentifier, SqlTokenTerminal> _content;

        public SqlBeginEndBlock( SqlTokenIdentifier begin, SqlStatementList body, SqlTokenIdentifier end, SqlTokenTerminal statementTerminator = null )
            : base( null, null )
        {
            _content = new SNode<SqlTokenIdentifier, SqlStatementList, SqlTokenIdentifier, SqlTokenTerminal>( begin, body, end, statementTerminator );
            CheckContent();
        }

        SqlBeginEndBlock( ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            _content = new SNode<SqlTokenIdentifier, SqlStatementList, SqlTokenIdentifier, SqlTokenTerminal>( items );
            CheckContent();
        }

        void CheckContent()
        {
            if( BeginT == null || BeginT.TokenType != SqlTokenType.Begin ) throw new ArgumentException( nameof( BeginT ) );
            if( Body == null ) throw new ArgumentException( nameof( Body ) );
            if( EndT == null || EndT.TokenType != SqlTokenType.End ) throw new ArgumentException( nameof( EndT ) );
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlBeginEndBlock( leading, content, trailing );
        }

        public StatementKnownName StatementKnownName => StatementKnownName.BeginEndBlock;

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

        public SqlTokenIdentifier BeginT => _content.V1;

        public SqlStatementList Body => _content.V2;

        public SqlTokenIdentifier EndT => _content.V3;

        public SqlTokenTerminal StatementTerminator => _content.V4;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

    }


}
