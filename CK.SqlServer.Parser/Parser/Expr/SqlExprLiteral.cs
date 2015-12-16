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
    /// Literal numbers (including 0x... literal binary values) and strings (either N'unicode' or 'one-byte-char').
    /// See <see cref="SqlTokenBaseLiteral"/>.
    /// </summary>
    public class SqlExprLiteral : SqlExprBaseMonoToken<SqlTokenBaseLiteral>
    {
        public SqlExprLiteral( SqlTokenBaseLiteral t )
            : base( t )
        {
        }

        internal SqlExprLiteral( ImmutableList<SqlTrivia> leading, ISqlNode[] items, ImmutableList<SqlTrivia> trailing )
            : base( leading, items, trailing )
        {
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IReadOnlyList<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlExprLiteral( leading, EnsureArray( children ), trailing );
        }


        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor )
        {
            return visitor.Visit( this );
        }


    }


}
