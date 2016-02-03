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
    /// Defines a non empty list of <see cref="SqlCaseWhenSelector"/> ("when Expression then Value") items 
    /// of a <see cref="SqlCase"/> expression.
    /// </summary>
    public sealed class SqlCaseWhenList : ASqlNodeList<SqlCaseWhenSelector>
    {
        public SqlCaseWhenList( IEnumerable<SqlCaseWhenSelector> items )
            : base( 1, items )
        {
        }

        SqlCaseWhenList( SqlCaseWhenList o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( o, 1, leading, items, trailing )
        {
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlCaseWhenList( this, leading, content, trailing );
        }

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

    }


}
