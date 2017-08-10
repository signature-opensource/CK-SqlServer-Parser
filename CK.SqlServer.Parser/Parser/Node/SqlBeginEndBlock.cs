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
    using CNode = SNode<SqlTokenIdentifier, SqlStatementList, SqlTokenIdentifier, SqlTokenTerminal>;

    /// <summary>
    /// A block is defined by begin...end enclosing keywords.
    /// </summary>
    public sealed class SqlBeginEndBlock : SqlNonTokenAutoWidth, ISqlNamedStatement
    {
        readonly CNode _content;

        public SqlBeginEndBlock( SqlTokenIdentifier begin, SqlStatementList body, SqlTokenIdentifier end, SqlTokenTerminal statementTerminator = null )
            : base( null, null )
        {
            _content = new CNode( begin, body, end, statementTerminator );
            CheckContent();
        }

        SqlBeginEndBlock( ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            _content = new CNode( items );
            CheckContent();
        }

        void CheckContent()
        {
            Helper.CheckToken( BeginT, nameof( BeginT ), SqlTokenType.Begin );
            Helper.CheckNotNull( Body, nameof( Body ) );
            Helper.CheckToken( EndT, nameof( EndT ), SqlTokenType.End );
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
