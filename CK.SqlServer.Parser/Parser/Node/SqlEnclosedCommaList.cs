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
    public sealed class SqlEnclosedCommaList : ASqlNodeEnclosedSeparatedList<SqlTokenOpenPar,ISqlNode,SqlTokenComma,SqlTokenClosePar>
    {
        /// <summary>
        /// Initializes a new <see cref="SqlEnclosedCommaList"/>.
        /// </summary>
        /// <param name="content">Items and comma tokens.</param>
        public SqlEnclosedCommaList( SqlTokenOpenPar openPar, IEnumerable<ISqlNode> content, SqlTokenClosePar closePar )
            : base( 0, true, openPar, content, closePar )
        {
        }

        SqlEnclosedCommaList( SqlEnclosedCommaList o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( o, 0, true, leading, items, trailing )
        {
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IReadOnlyList<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlEnclosedCommaList( this, leading, children, trailing );
        }
        
        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor ) => visitor.Visit( this );

    }

}
