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
    /// List of comma separated <see cref="SqlCTEName"/> that can not be empty.
    /// </summary>
    public sealed class SqlCTENameList : ASqlNodeSeparatedList<SqlCTEName, SqlTokenComma>
    {
        public SqlCTENameList( IEnumerable<ISqlNode> items )
            : base( null, 1, null, items, null )
        {
        }

        SqlCTENameList( SqlCTENameList o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( o, 1, leading, items, trailing )
        {
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlCTENameList( this, leading, content, trailing );
        }

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

    }


}
