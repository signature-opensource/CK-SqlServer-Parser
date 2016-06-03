using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using CK.Core;

namespace CK.SqlServer.Parser
{
    /// <summary>
    /// Possibly empty list of comma separated <see cref="SqlCallParameter"/>
    /// </summary>
    public class SqlCallParameterList : ASqlNodeSeparatedList<SqlCallParameter, SqlTokenComma>
    {
        public SqlCallParameterList( IEnumerable<ISqlNode> items )
            : base( null, 0, null, items, null )
        {
        }

        SqlCallParameterList( SqlCallParameterList o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( o, 0, leading, items, trailing )
        {
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlCallParameterList( this, leading, content, trailing );
        }

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

    }
}
