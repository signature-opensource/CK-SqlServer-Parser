#region Proprietary License
/*----------------------------------------------------------------------------
* This file (CK.SqlServer.Parser\Parser\Expr\SqlExprBinaryOperator.cs) is part of CK-Database. 
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
    public class SqlExprBinaryOperator : SqlExprBaseBinary
    {
        public SqlExprBinaryOperator( SqlExpr left, SqlToken op, SqlExpr right )
            : base( left, op, right )
        {
            if( !IsValidOperator( op.TokenType ) ) throw new ArgumentException();
        }

        protected SqlExprBinaryOperator( ImmutableList<SqlTrivia> leading, SqlNode[] items, ImmutableList<SqlTrivia> trailing )
            : base( leading, items, trailing )
        {
            Debug.Assert( IsValidOperator( Middle.TokenType ) );
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IReadOnlyList<SqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlExprBinaryOperator( leading, EnsureArray( children ), trailing );
        }

        static public bool IsValidOperator( SqlTokenType op )
        {
            if( op > 0 )
            {
                if( (op & SqlTokenType.IsCompareOperator) != 0 ) return true;
                if( (op & SqlTokenType.IsBasicOperator) != 0 )
                {
                    if( op != SqlTokenType.BitwiseNot && op != SqlTokenType.Is) return true;
                }
                else if( op == SqlTokenType.And || op == SqlTokenType.Or ) return true;
            }
            return false;
        }

        public new SqlToken Middle { get { return (SqlToken)base.Middle; } }

        public SqlToken Operator { get { return (SqlToken)base.Middle; } }

        [DebuggerStepThrough]
        internal protected override T Accept<T>( ISqlItemVisitor<T> visitor )
        {
            return visitor.Visit( this );
        }
    }
}
