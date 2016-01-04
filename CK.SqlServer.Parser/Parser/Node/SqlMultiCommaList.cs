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
    /// Comma separated list of <see cref="SqlEnclosedCommaList"/>. Possibly empty.
    /// </summary>
    public sealed class SqlMultiCommaList : ASqlNodeSeparatedList<SqlEnclosedCommaList,SqlTokenComma>
    {
        /// <summary>
        /// Initializes a new <see cref="SqlMultiCommaList"/>.
        /// </summary>
        /// <param name="content">Items and comma tokens.</param>
        public SqlMultiCommaList( IEnumerable<ISqlNode> content )
            : base( null, 0, null, content, null )
        {
        }

        SqlMultiCommaList( SqlMultiCommaList o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( o, 0, leading, items, trailing )
        {
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlMultiCommaList( this, leading, children, trailing );
        }
        
        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor ) => visitor.Visit( this );

    }

}
