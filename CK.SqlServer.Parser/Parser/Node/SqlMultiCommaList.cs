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
        /// <param name="content"><see cref="SqlEnclosedCommaList"/> and <see cref="SqlTokenComma"/>.</param>
        /// <param name="leading">Optional leading trivias.</param>
        /// <param name="trailing">Optional trailing trivias.</param>
        public SqlMultiCommaList( IEnumerable<ISqlNode> content, ImmutableList<SqlTrivia> leading = null, ImmutableList<SqlTrivia> trailing = null )
            : base( null, 0, leading, content, trailing )
        {
        }

        /// <summary>
        /// Initializes a new <see cref="SqlMultiCommaList"/> with one or zero <see cref="SqlEnclosedCommaList"/> in it.
        /// </summary>
        /// <param name="item">An optional <see cref="SqlEnclosedCommaList"/>.</param>
        /// <param name="leading">Optional leading trivias.</param>
        /// <param name="trailing">Optional trailing trivias.</param>
        public SqlMultiCommaList( SqlEnclosedCommaList item = null, ImmutableList<SqlTrivia> leading = null, ImmutableList<SqlTrivia> trailing = null )
            : base( null, 0, leading, new[] { item }, trailing )
        {
        }

        SqlMultiCommaList( SqlMultiCommaList o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( o, 0, leading, items, trailing )
        {
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlMultiCommaList( this, leading, content, trailing );
        }
        public SqlMultiCommaList AppendValue( ISqlNode expression )
        {
            int count = Count;
            if( count == 0 ) return new SqlMultiCommaList( new SqlEnclosedCommaList( expression ), LeadingTrivias, TrailingTrivias );
            return (SqlMultiCommaList)DoReplaceItems( ( idx, item ) => item.InsertAt( item.Count, expression ) );
        }
        
        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

    }

}
