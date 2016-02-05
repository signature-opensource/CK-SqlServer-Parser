using CK.Core;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Parser
{
    /// <summary>
    /// Abstract class that can be used to extend the model with any type of nodes.
    /// </summary>
    public abstract class SqlNodeExternal : SqlNode
    {
        protected SqlNodeExternal( ImmutableList<SqlTrivia> leading = null, ImmutableList<SqlTrivia> trailing = null )
            : base( leading, trailing )
        {
        }

        [DebuggerStepThrough]
        internal protected override sealed ISqlNode Accept( SqlNodeVisitor visitor )
        {
            return visitor.Visit( this );
        }
    }
}
