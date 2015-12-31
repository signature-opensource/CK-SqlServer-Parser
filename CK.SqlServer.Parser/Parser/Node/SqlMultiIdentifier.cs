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

    public sealed class SqlMultiIdentifier : ASqlNodeSeparatedList<SqlTokenIdentifier,ISqlTokenIdentifierSeparator>, ISqlIdentifier
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

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlMultiIdentifier( this, leading, items, trailing );
        }
        public bool IsVariable => this[0].IsVariable;

        IReadOnlyList<SqlTokenIdentifier> ISqlIdentifier.Identifiers => this;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor ) => visitor.Visit( this );

    }

}
