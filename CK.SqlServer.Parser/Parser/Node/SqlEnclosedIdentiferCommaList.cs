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
    /// <summary>
    /// Enclosed non empty comma separated list of <see cref="ISqlIdentifier"/>.
    /// </summary>
    public sealed class SqlEnclosedIdentiferCommaList : ASqlNodeEnclosedSeparatedList<SqlTokenOpenPar,ISqlIdentifier,SqlTokenComma,SqlTokenClosePar>
    {
        /// <summary>
        /// Initializes a new list of identifiers.
        /// </summary>
        /// <param name="openPar">Opening parenthesis. Can not be null.</param>
        /// <param name="tokens">Comma separated list of <see cref="ISqlIdentifier"/> (can not be empty).</param>
        /// <param name="closePar">Closing parenthesis. Can not be null.</param>
        public SqlEnclosedIdentiferCommaList( SqlTokenOpenPar openPar, IEnumerable<ISqlNode> tokens, SqlTokenClosePar closePar )
            : base( 1, false, openPar, tokens, closePar )
        {
        }

        SqlEnclosedIdentiferCommaList( SqlEnclosedIdentiferCommaList o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( o, 1, false, leading, items, trailing )
        {
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IReadOnlyList<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlEnclosedIdentiferCommaList( this, leading, children, trailing );
        }

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor ) => visitor.Visit( this );

    }

}
