#region Proprietary License
/*----------------------------------------------------------------------------
* This file (CK.SqlServer.Parser\Parser\Expr\SqlExprBaseMonoToken.cs) is part of CK-Database. 
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
    public abstract class SqlExprBaseMonoToken<T> : SqlExpr 
        where T : SqlToken 
    {
        protected SqlExprBaseMonoToken( T t )
            : this( null, CreateArray<SqlNode>( SqlToken.EmptyOpenPar, t, SqlToken.EmptyClosePar ), null )
        {
        }

        protected SqlExprBaseMonoToken( ImmutableList<SqlTrivia> leading, ISqlNode[] items, ImmutableList<SqlTrivia> trailing )
            : base( leading, items, trailing )
        {
        }

        public T Token { get { return (T)Slots[1]; } }

    }


}
