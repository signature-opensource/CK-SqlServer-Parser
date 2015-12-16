using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;

namespace CK.SqlServer.Parser
{
    /// <summary>
    /// Captures a select column definition. 
    /// </summary>
    public class SqlNoExprOverClause : SqlItem
    {
        public SqlNoExprOverClause( SqlTokenIdentifier overT, SqlExpr overExpression )
            : this( null, CreateArray<SqlNode>( overT, overExpression ), null )
        {
        }

        internal SqlNoExprOverClause( ImmutableList<SqlTrivia> leading, ISqlNode[] slots, ImmutableList<SqlTrivia> trailing )
            : base( leading, slots, trailing )
        {
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IReadOnlyList<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlNoExprOverClause( leading, EnsureArray( children ), trailing );
        }

        public SqlTokenIdentifier OverT { get { return (SqlTokenIdentifier)Slots[0]; } }

        public SqlExpr OverExpression { get { return (SqlExpr)Slots[1]; } }

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor )
        {
            return visitor.Visit( this );
        }
    }


}
