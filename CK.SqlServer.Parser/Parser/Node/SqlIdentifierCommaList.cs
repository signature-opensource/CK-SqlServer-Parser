using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CK.SqlServer.Parser
{
    /// <summary>
    /// Non-enclosable, non empty, comma separated list of <see cref="ISqlIdentifier"/>.
    /// </summary>
    public sealed class SqlIdentifierCommaList : ASqlNodeSeparatedList<ISqlIdentifier,SqlTokenComma>
    {
        public SqlIdentifierCommaList( IEnumerable<ISqlNode> items )
            : base( null, 1, null, items, null )
        {
        }

        SqlIdentifierCommaList( SqlIdentifierCommaList o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( o, 1, leading, items, trailing )
        {
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlIdentifierCommaList( this, leading, content, trailing );
        }

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

    }


}
