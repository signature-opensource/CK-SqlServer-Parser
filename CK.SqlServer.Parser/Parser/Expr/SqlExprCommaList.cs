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
    /// Comma separated list of <see cref="SqlExpr"/> (possibly empty).
    /// </summary>
    public class SqlExprCommaList : SqlExprBaseExprList<SqlExpr>
    {
        /// <summary>
        /// Initializes a new list of expressions with enclosing parenthesis.
        /// </summary>
        /// <param name="openPar">Opening parenthesis. Can not be null.</param>
        /// <param name="tokens">Comma separated list of <see cref="SqlExpr"/> (possibly empty).</param>
        /// <param name="closePar">Closing parenthesis. Can not be null.</param>
        public SqlExprCommaList( SqlTokenOpenPar openPar, IList<ISqlNode> tokens, SqlTokenClosePar closePar )
            : base( openPar, tokens, closePar, true )
        {
        }

        /// <summary>
        /// Initializes a new list of expressions without enclosing parenthesis.
        /// </summary>
        /// <param name="tokens">Comma separated list of <see cref="SqlExpr"/> (possibly empty).</param>
        public SqlExprCommaList( IList<ISqlNode> tokens )
            : base( tokens, true )
        {
        }

        protected SqlExprCommaList( ImmutableList<SqlTrivia> leading, ISqlNode[] items, ImmutableList<SqlTrivia> trailing )
            : base( leading, items, trailing )
        {
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IReadOnlyList<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlExprCommaList( leading, EnsureArray( children ), trailing );
        }

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor )
        {
            return visitor.Visit( this );
        }

    }

}
