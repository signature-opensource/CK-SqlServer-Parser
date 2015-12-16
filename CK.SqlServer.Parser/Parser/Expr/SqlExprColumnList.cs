#region Proprietary License
/*----------------------------------------------------------------------------
* This file (CK.SqlServer.Parser\Parser\Expr\SqlExprColumnList.cs) is part of CK-Database. 
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
    public class SqlExprColumnList : SqlExprBaseExprList<SqlExprIdentifier>
    {
        /// <summary>
        /// Initializes a new list of columns.
        /// </summary>
        /// <param name="openPar">Opening parenthesis. Can not be null.</param>
        /// <param name="tokens">Comma separated list of <see cref="SqlExprIdentifier"/> (can not be empty).</param>
        /// <param name="closePar">Closing parenthesis. Can not be null.</param>
        public SqlExprColumnList( SqlTokenOpenPar openPar, IList<ISqlNode> tokens, SqlTokenClosePar closePar )
            : base( openPar, tokens, closePar, false )
        {
        }

        protected SqlExprColumnList( ImmutableList<SqlTrivia> leading, ISqlNode[] items, ImmutableList<SqlTrivia> trailing )
            : base( leading, items, trailing )
        {
            Debug.Assert( NonSeparatorCount > 0, "Column list must not be empty." );
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IReadOnlyList<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlExprColumnList( leading, EnsureArray( children ), trailing );
        }

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor )
        {
            return visitor.Visit( this );
        }

    }

}
