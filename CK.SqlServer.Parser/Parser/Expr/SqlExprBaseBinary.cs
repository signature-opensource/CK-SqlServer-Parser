#region Proprietary License
/*----------------------------------------------------------------------------
* This file (CK.SqlServer.Parser\Parser\Expr\SqlExprBaseBinary.cs) is part of CK-Database. 
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
    public abstract class SqlExprBaseBinary : SqlExpr
    {
        protected SqlExprBaseBinary( SqlExpr left, SqlNode middle, SqlExpr right )
            : this( null, Build( left, middle, right ), null )
        {
        }

        static ISqlNode[] Build( SqlItem left, SqlNode middle, SqlItem right )
        {
            if( left == null ) throw new ArgumentNullException( "left" );
            if( middle == null ) throw new ArgumentNullException( "middle" );
            if( right == null ) throw new ArgumentNullException( "right" );
            return CreateArray( SqlToken.EmptyOpenPar, left, middle, right, SqlToken.EmptyClosePar );
        }

        protected SqlExprBaseBinary( ImmutableList<SqlTrivia> leading, ISqlNode[] items, ImmutableList<SqlTrivia> trailing )
            : base( leading, items, trailing )
        {
        }

        public SqlExpr Left { get { return (SqlExpr)Slots[1]; } }

        protected ISqlNode Middle { get { return Slots[2]; } }

        public SqlExpr Right { get { return (SqlExpr)Slots[3]; } }

    }

}
