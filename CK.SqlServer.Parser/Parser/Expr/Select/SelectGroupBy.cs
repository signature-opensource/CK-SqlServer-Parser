#region Proprietary License
/*----------------------------------------------------------------------------
* This file (CK.SqlServer.Parser\Parser\Expr\Select\SelectGroupBy.cs) is part of CK-Database. 
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
    /// Captures the optional "Group by ... having ..." select part.
    /// Even if it seems that "having" can exist without "group by" clause, I have not found any use of it: I decided to subordinate the "having" to the "group by".
    /// </summary>
    public class SelectGroupBy : SqlItem
    {
        public SelectGroupBy( SqlTokenIdentifier groupToken, SqlTokenIdentifier byT, SqlExpr groupContent, SqlTokenIdentifier havingT = null, SqlExpr havingExpression = null )
            : this( null, Build( groupToken, byT, groupContent, havingT, havingExpression ), null )
        {
        }

        static ISqlNode[] Build( SqlTokenIdentifier groupToken, SqlTokenIdentifier byT, SqlExpr groupContent, SqlTokenIdentifier havingT = null, SqlExpr havingExpression = null )
        {
            if( havingT != null )
            {
                if( havingExpression == null ) throw new ArgumentNullException( "havingExpression" );
                return CreateArray<SqlNode>( groupToken, byT, groupContent, havingT, havingExpression );
            }
            return CreateArray<SqlNode>( groupToken, byT, groupContent );
        }

        internal SelectGroupBy( ImmutableList<SqlTrivia> leading, ISqlNode[] items, ImmutableList<SqlTrivia> trailing )
            : base( leading, items, trailing )
        {
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IReadOnlyList<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SelectGroupBy( leading, EnsureArray( children ), trailing );
        }

        public SqlExpr GroupExpression { get { return (SqlExpr)Slots[2]; } }

        public SqlExpr HavingExpression { get { return Slots.Length > 3 ? (SqlExpr)Slots[4] : null; } }

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor )
        {
            return visitor.Visit( this );
        }
    }


}
