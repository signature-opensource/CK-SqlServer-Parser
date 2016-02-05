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

    public sealed class SqlMultiIdentifier : ASqlNodeSeparatedList<ISqlIdentifier, ISqlTokenIdentifierSeparator>, ISqlIdentifier
    {
        /// <summary>
        /// Initializes a new <see cref="SqlMultiIdentifier"/>.
        /// </summary>
        /// <param name="tokens">Identifiers and separator tokens.</param>
        public SqlMultiIdentifier( IEnumerable<ISqlNode> tokens )
            : this( null, null, tokens, null )
        {
        }

        SqlMultiIdentifier( SqlMultiIdentifier o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( o, 1, leading, items, trailing )
        {
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlMultiIdentifier( this, leading, content, trailing );
        }
        public bool IsVariable => this[0].IsVariable;

        public bool IsOpenDataSouce => this[0].IsOpenDataSouce;

        IReadOnlyList<ISqlIdentifier> ISqlIdentifier.Identifiers => this;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

    }

}
