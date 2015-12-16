#region Proprietary License
/*----------------------------------------------------------------------------
* This file (CK.SqlServer.Parser\Parser\Expr\SqlExprNull.cs) is part of CK-Database. 
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
    public class SqlExprNull : SqlExprBaseMonoToken<SqlTokenIdentifier>
    {
        public SqlExprNull( SqlTokenIdentifier nullT )
            : base( nullT )
        {
            if( nullT.TokenType != SqlTokenType.Null
                || String.Compare( nullT.Name, "null", StringComparison.OrdinalIgnoreCase ) != 0 )
            {
                throw new ArgumentException( "Invalid null token.", "nullT" );
            }
        }

        internal SqlExprNull( ImmutableList<SqlTrivia> leading, SqlNode[] items, ImmutableList<SqlTrivia> trailing )
            : base( leading, items, trailing )
        {
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IReadOnlyList<SqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlExprNull( leading, EnsureArray( children ), trailing );
        }

        [DebuggerStepThrough]
        internal protected override SqlNode Accept( SqlItemVisitor visitor )
        {
            return visitor.Visit( this );
        }
    }


}
