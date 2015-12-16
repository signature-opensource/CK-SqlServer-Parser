#region Proprietary License
/*----------------------------------------------------------------------------
* This file (CK.SqlServer.Parser\Parser\Expr\SqlExprUnaryOperator.cs) is part of CK-Database. 
* Copyright © 2007-2014, Invenietis <http://www.invenietis.com>. All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using CK.Core;
using System.Collections.Immutable;

namespace CK.SqlServer.Parser
{
    public class SqlExprUnaryOperator : SqlExpr
    {
        public SqlExprUnaryOperator( SqlToken op, SqlExpr rightExpr )
            : this( null, Build( op, rightExpr ), null )
        {
        }

        static ISqlNode[] Build( SqlToken op, SqlExpr rightExpr )
        {
            if( op == null ) throw new ArgumentNullException( "op" );
            if( rightExpr == null ) throw new ArgumentNullException( "rightExpr" );
            return CreateArray<SqlNode>( SqlToken.EmptyOpenPar, op, rightExpr, SqlToken.EmptyClosePar );
        }

        SqlExprUnaryOperator( ImmutableList<SqlTrivia> leading, ISqlNode[] items, ImmutableList<SqlTrivia> trailing )
            : base( leading, items, trailing )
        {
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IReadOnlyList<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlExprUnaryOperator( leading, EnsureArray( children ), trailing );
        }

        public SqlToken OperatorT { get { return (SqlToken)Slots[1]; } }

        public SqlExpr Expression { get { return (SqlExpr)Slots[2]; } }

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor )
        {
            return visitor.Visit( this );
        }
    }
}
