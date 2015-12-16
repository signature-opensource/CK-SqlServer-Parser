#region Proprietary License
/*----------------------------------------------------------------------------
* This file (CK.SqlServer.Parser\Parser\Expr\SqlExprBaseExprList.cs) is part of CK-Database. 
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
    /// Base class for comma separated list (possibly empty) of <typeparamref name="T"/> that are <see cref="SqlItem"/> optionally enclosed in parenthesis.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class SqlExprBaseExprList<T> : SqlExprBaseListWithSeparatorList<T> where T : SqlItem 
    {
        /// <summary>
        /// Initializes a new <see cref="SqlExprBaseExprList{T}"/> of <typeparamref name="T"/> enclosed in a <see cref="SqlTokenOpenPar"/> and a <see cref="SqlTokenClosePar"/>.
        /// </summary>
        /// <param name="openPar">Opening parenthesis.</param>
        /// <param name="exprOrCommaTokens">List of tokens or expressions.</param>
        /// <param name="closePar">Closing parenthesis.</param>
        /// <param name="allowEmpty">False to throw an argument exception if the <paramref name="exprOrCommaTokens"/> is empty.</param>
        protected SqlExprBaseExprList( SqlTokenOpenPar openPar, IList<ISqlNode> exprOrCommaTokens, SqlTokenClosePar closePar, bool allowEmpty )
            : base( openPar, exprOrCommaTokens, closePar, allowEmpty, IsCommaSeparator )
        {
        }

        /// <summary>
        /// Initializes a new <see cref="SqlExprBaseExprList{T}"/> of <typeparamref name="T"/> without Opener nor Closer.
        /// </summary>
        /// <param name="exprOrCommaTokens">List of tokens or expressions.</param>
        /// <param name="allowEmpty">Allows empty list.</param>
        protected SqlExprBaseExprList( IList<ISqlNode> exprOrCommaTokens, bool allowEmpty )
            : base( exprOrCommaTokens, allowEmpty, IsCommaSeparator )
        {
        }

        protected SqlExprBaseExprList( ImmutableList<SqlTrivia> leading, ISqlNode[] items, ImmutableList<SqlTrivia> trailing )
            : base( leading, items, trailing )
        {
        }
        
    }

}
