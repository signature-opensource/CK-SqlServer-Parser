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
    /// List of comma separated <see cref="SelectOrderByColumn"/>
    /// </summary>
    public sealed class SelectOrderByColumnList : ASqlNodeSeparatedList<SelectOrderByColumn,SqlTokenComma>
    {
        public SelectOrderByColumnList( IEnumerable<ISqlNode> items )
            : base( null, 1, null, items, null )
        {
        }

        SelectOrderByColumnList( SelectOrderByColumnList o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( o, 1, leading, items, trailing )
        {
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IReadOnlyList<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SelectOrderByColumnList( this, leading, children, trailing );
        }

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor ) => visitor.Visit( this );

    }


}
