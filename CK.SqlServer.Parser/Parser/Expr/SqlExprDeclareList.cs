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
    public class SqlExprDeclareList : SqlExprBaseExprList<SqlExprDeclare>
    {
        /// <summary>
        /// Initializes a new list of variable declarations.
        /// </summary>
        /// <param name="content">Comma separated list of <see cref="SqlExprDeclare"/> (must not be empty).</param>
        public SqlExprDeclareList( IList<ISqlNode> content )
            : base( content, false )
        {
        }

        internal SqlExprDeclareList( ImmutableList<SqlTrivia> leading, ISqlNode[] items, ImmutableList<SqlTrivia> trailing )
            : base( leading, items, trailing )
        {
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IReadOnlyList<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlExprDeclareList( leading, EnsureArray( children ), trailing );
        }

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor )
        {
            return visitor.Visit( this );
        }

    }

}
