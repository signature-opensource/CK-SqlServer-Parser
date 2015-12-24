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
    /// Comma separated list of <see cref="ISqlNode"/>. Possibly empty.
    /// </summary>
    public class SqlCommaList : ASqlNodeSeparatedList<ISqlNode,SqlTokenComma>
    {
        /// <summary>
        /// Initializes a new <see cref="SqlCommaList"/>.
        /// </summary>
        /// <param name="tokens">Items and comma tokens.</param>
        public SqlCommaList( IEnumerable<ISqlNode> tokens )
            : this( null, null, tokens, null )
        {
        }

        SqlCommaList( SqlCommaList o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( o, 0, leading, items, trailing )
        {
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IReadOnlyList<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlCommaList( this, leading, children, trailing );
        }
        
        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor ) => visitor.Visit( this );

    }

}
