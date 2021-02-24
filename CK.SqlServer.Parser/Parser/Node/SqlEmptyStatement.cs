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
    using CNode = SNode<SqlTokenTerminal>;

    /// <summary>
    /// An isolated statement terminator ; is valid.
    /// This is also the "empty node" since the <see cref="StatementTerminator"/> can be null.
    /// </summary>
    public sealed class SqlEmptyStatement : SqlNonTokenAutoWidth, ISqlNamedStatement
    {
        readonly SqlTokenTerminal[] _content;

        public SqlEmptyStatement( SqlTokenTerminal statementTerminator, ImmutableList<SqlTrivia> leading = null, ImmutableList<SqlTrivia> trailing = null )
            : base( leading, trailing )
        {
            _content = statementTerminator != null ? new[] { statementTerminator } : Array.Empty<SqlTokenTerminal>();
            Helper.CheckNullableToken( StatementTerminator, nameof( StatementTerminator ), SqlTokenType.SemiColon );
        }

        public StatementKnownName StatementKnownName => StatementKnownName.Empty;

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public override IList<ISqlNode> GetRawContent() => new[] { StatementTerminator };

        /// <summary>
        /// Gets the optional terminator. 
        /// </summary>
        public SqlTokenTerminal StatementTerminator => _content.Length == 0 ? null : _content[0];

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlEmptyStatement( content == null ? StatementTerminator : (SqlTokenTerminal)content[0], leading, trailing );
        }

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

    }


}
