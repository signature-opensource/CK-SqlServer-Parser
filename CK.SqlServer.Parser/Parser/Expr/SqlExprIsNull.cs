#region Proprietary License
/*----------------------------------------------------------------------------
* This file (CK.SqlServer.Parser\Parser\Expr\SqlExprIsNull.cs) is part of CK-Database. 
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
    public class SqlExprIsNull : SqlExpr
    {
        public SqlExprIsNull( SqlItem left, SqlTokenIdentifier isT, SqlTokenIdentifier notT, SqlTokenIdentifier nullT )
            : this( null, Build( left, isT, notT, nullT ), null )
        {
        }

        static SqlNode[] Build( SqlItem left, SqlTokenIdentifier isT, SqlTokenIdentifier notT, SqlTokenIdentifier nullT )
        {
            return notT != null 
                        ? CreateArray<SqlNode>( SqlTokenList<SqlTokenOpenPar>.Empty, left, isT, notT, nullT, SqlTokenList<SqlTokenClosePar>.Empty )
                        : CreateArray<SqlNode>( SqlTokenList<SqlTokenOpenPar>.Empty, left, isT, nullT, SqlTokenList<SqlTokenClosePar>.Empty );
        }

        internal SqlExprIsNull( ImmutableList<SqlTrivia> leading, SqlNode[] items, ImmutableList<SqlTrivia> trailing )
            : base( leading, items, trailing )
        {
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IReadOnlyList<SqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlExprIsNull( leading, EnsureArray( children ), trailing );
        }


        public SqlItem Left { get { return (SqlItem)Slots[1]; } }

        public SqlTokenIdentifier IsT { get { return (SqlTokenIdentifier)Slots[2]; } }

        public bool IsNotNull { get { return Slots.Length == 6; } }

        public SqlTokenIdentifier NotT { get { return IsNotNull ? (SqlTokenIdentifier)Slots[3] : null; } }

        public SqlTokenIdentifier NullT { get { return (SqlTokenIdentifier)Slots[IsNotNull ? 4 : 3]; } }

        [DebuggerStepThrough]
        internal protected override T Accept<T>( ISqlItemVisitor<T> visitor )
        {
            return visitor.Visit( this );
        }


    }


}
