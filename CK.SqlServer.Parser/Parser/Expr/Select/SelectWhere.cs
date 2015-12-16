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
    /// Captures the optional "Where ..." select part.
    /// </summary>
    public class SelectWhere : SqlItem
    {
        public SelectWhere( SqlTokenIdentifier whereT, SqlExpr expression )
            : this( null, CreateArray<SqlNode>( whereT, expression ), null )
        {
        }

        internal SelectWhere( ImmutableList<SqlTrivia> leading, ISqlNode[] items, ImmutableList<SqlTrivia> trailing )
            : base( leading, items, trailing )
        {
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IReadOnlyList<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SelectWhere( leading, EnsureArray( children ), trailing );
        }

        public SqlTokenIdentifier WhereT { get { return (SqlTokenIdentifier)Slots[0]; } }
        
        public SqlExpr Expression { get { return (SqlExpr)Slots[1]; } }


        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor )
        {
            return visitor.Visit( this );
        }
    }


}
