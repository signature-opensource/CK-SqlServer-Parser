#region Proprietary License
/*----------------------------------------------------------------------------
* This file (CK.SqlServer.Parser\Parser\Expr\SqlNoExprOverClause.cs) is part of CK-Database. 
* Copyright © 2007-2014, Invenietis <http://www.invenietis.com>. All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

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

        internal SqlNoExprOverClause( ImmutableList<SqlTrivia> leading, SqlNode[] slots, ImmutableList<SqlTrivia> trailing )
            : base( leading, slots, trailing )
        {
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IReadOnlyList<SqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlNoExprOverClause( leading, children, trailing );
        }

        public SqlTokenIdentifier OverT { get { return (SqlTokenIdentifier)Slots[0]; } }

        public SqlExpr OverExpression { get { return (SqlExpr)Slots[1]; } }

        [DebuggerStepThrough]
        internal protected override T Accept<T>( ISqlItemVisitor<T> visitor )
        {
            return visitor.Visit( this );
        }
    }


}
