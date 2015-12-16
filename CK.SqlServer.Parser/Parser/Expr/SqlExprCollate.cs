#region Proprietary License
/*----------------------------------------------------------------------------
* This file (CK.SqlServer.Parser\Parser\Expr\SqlExprCollate.cs) is part of CK-Database. 
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
    public class SqlExprCollate : SqlExpr
    {
        public SqlExprCollate( SqlExpr left, SqlTokenIdentifier collateT, SqlTokenIdentifier nameT )
            : this( null, CreateArray<SqlNode>( SqlToken.EmptyOpenPar, left, collateT, nameT, SqlToken.EmptyClosePar ), null )
        {
        }

        protected SqlExprCollate( ImmutableList<SqlTrivia> leading, SqlNode[] items, ImmutableList<SqlTrivia> trailing )
            : base( leading, items, trailing )
        {
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IReadOnlyList<SqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlExprCollate( leading, EnsureArray( children ), trailing );
        }

        [DebuggerStepThrough]
        internal protected override SqlNode Accept( SqlItemVisitor visitor )
        {
            return visitor.Visit( this );
        }
    }


}
