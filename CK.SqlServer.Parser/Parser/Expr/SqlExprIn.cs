#region Proprietary License
/*----------------------------------------------------------------------------
* This file (CK.SqlServer.Parser\Parser\Expr\SqlExprIn.cs) is part of CK-Database. 
* Copyright © 2007-2014, Invenietis <http://www.invenietis.com>. All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

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
    /// 
    /// </summary>
    public class SqlExprIn : SqlExpr
    {
        public SqlExprIn( SqlExpr left, SqlTokenIdentifier notT, SqlTokenIdentifier inT, SqlExprCommaList values )
            : this( null, Build( left, notT, inT, values ), null )
        {
        }

        static ISqlNode[] Build( SqlExpr left, SqlTokenIdentifier notT, SqlTokenIdentifier inT, SqlExprCommaList values )
        {
            return notT != null
                            ? CreateArray<SqlNode>( SqlToken.EmptyOpenPar, left, notT, inT, values, SqlToken.EmptyClosePar )
                            : CreateArray<SqlNode>( SqlToken.EmptyOpenPar, left, inT, values, SqlToken.EmptyClosePar );
        }

        internal SqlExprIn( ImmutableList<SqlTrivia> leading, ISqlNode[] items, ImmutableList<SqlTrivia> trailing )
            : base( leading, items, trailing )
        {
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IReadOnlyList<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlExprIn( leading, EnsureArray( children ), trailing );
        }

        public SqlExpr Left { get { return (SqlExpr)Slots[1]; } }

        public bool IsNotIn { get { return Slots.Length == 6; } }

        public SqlTokenIdentifier NotT { get { return IsNotIn ? (SqlTokenIdentifier)Slots[2] : null; } }

        public SqlTokenIdentifier InT { get { return (SqlTokenIdentifier)Slots[IsNotIn ? 3 : 2]; } }

        public SqlExprCommaList Values { get { return (SqlExprCommaList)Slots[IsNotIn ? 4 : 3]; } }

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor )
        {
            return visitor.Visit( this );
        }

    }


}
