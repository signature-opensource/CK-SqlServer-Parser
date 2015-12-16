#region Proprietary License
/*----------------------------------------------------------------------------
* This file (CK.SqlServer.Parser\Parser\Expr\Select\SelectOrderByOffset.cs) is part of CK-Database. 
* Copyright © 2007-2014, Invenietis <http://www.invenietis.com>. All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Parser
{
    public class SelectOrderByOffset : SqlItem
    {
        public SelectOrderByOffset( SqlTokenIdentifier offsetToken, SqlExpr offsetExpr, SqlTokenIdentifier rowsToken )
            : base( null, CreateArray<SqlNode>( offsetToken, offsetExpr, rowsToken ), null )
        {
        }

        public SelectOrderByOffset( SqlTokenIdentifier offsetToken, SqlExpr offsetExpr, SqlTokenIdentifier rowsToken,
                                    SqlTokenIdentifier fetchToken, SqlTokenIdentifier firstOrNextToken, SqlExpr fetchExpr, SqlTokenIdentifier fetchRowsToken, SqlTokenIdentifier onlyToken )
            : base( null, CreateArray<SqlNode>( offsetToken, offsetExpr, rowsToken, fetchToken, firstOrNextToken, fetchExpr, fetchRowsToken, onlyToken ), null )
        {
        }

        internal SelectOrderByOffset( ImmutableList<SqlTrivia> leading, ISqlNode[] items, ImmutableList<SqlTrivia> trailing )
            : base( leading, items, trailing )
        {
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IReadOnlyList<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SelectOrderByOffset( leading, EnsureArray( children ), trailing );
        }


        public SqlTokenIdentifier OffsetT { get { return (SqlTokenIdentifier)Slots[0]; } }

        public SqlExpr OffsetExpression { get { return (SqlExpr)Slots[1]; } }

        public SqlTokenIdentifier RowsT { get { return (SqlTokenIdentifier)Slots[2]; } }
    
        public bool HasFetchClause { get { return Slots.Length > 3; } }

        public SqlTokenIdentifier FetchT { get { return HasFetchClause ? (SqlTokenIdentifier)Slots[3] : null; } }

        public SqlTokenIdentifier FetchFirstOrNextT { get { return HasFetchClause ? (SqlTokenIdentifier)Slots[4] : null; } }

        public SqlExpr FetchExpression { get { return HasFetchClause ? (SqlExpr)Slots[5] : null; } }

        public SqlTokenIdentifier FetchRowsT { get { return HasFetchClause ? (SqlTokenIdentifier)Slots[6] : null; } }

        public SqlTokenIdentifier FetchOnlyT { get { return HasFetchClause ? (SqlTokenIdentifier)Slots[7] : null; } }

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor )
        {
            return visitor.Visit( this );
        }
    }
}