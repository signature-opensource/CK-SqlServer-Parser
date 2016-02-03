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
    /// Non-enclosable, possibly empty, comma separated list of ony kind of node.
    /// </summary>
    public sealed class SqlCommaList : ASqlNodeSeparatedList<ISqlNode,SqlTokenComma>
    {
        public SqlCommaList( IEnumerable<ISqlNode> items )
            : base( null, 0, null, items, null )
        {
        }

        SqlCommaList( SqlCommaList o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( o, 0, leading, items, trailing )
        {
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlCommaList( this, leading, content, trailing );
        }

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

    }


}
