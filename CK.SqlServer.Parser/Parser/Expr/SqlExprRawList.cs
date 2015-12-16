#region Proprietary License
/*----------------------------------------------------------------------------
* This file (CK.SqlServer.Parser\Parser\Expr\SqlExprRawList.cs) is part of CK-Database. 
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
    /// Raw list of contiguous <see cref="SqlNode"/> that can be enclosed in parenthesis.
    /// </summary>
    public sealed class SqlExprRawItemList : SqlExpr
    {
        /// <summary>
        /// Initializes a new raw list without any opener/closer parenthesis.
        /// </summary>
        /// <param name="items">List of any kind of <see cref="SqlNode"/> that compose this block.</param>
        public SqlExprRawItemList( IList<SqlNode> items )
            : this( null, CreateEnclosedArray( items.AsReadOnlyList() ), null )
        {
        }

        /// <summary>
        /// Initializes a new raw list that is enclosed in a pair of opener/closer parenthesis.
        /// </summary>
        /// <param name="openPar">Opening parenthesis.</param>
        /// <param name="items">
        /// List of <see cref="SqlNode"/> that compose this block. 
        /// This MUST not contain the <see cref="Opener"/> and/or the <see cref="Closer"/>.</param>
        /// <param name="closePar">Closing parenthesis.</param>
        public SqlExprRawItemList( SqlTokenOpenPar openPar, IList<SqlNode> items, SqlTokenClosePar closePar )
            : this( null, CreateArray( openPar, items.AsReadOnlyList(), items.Count, closePar ), null )
        {
        }

        SqlExprRawItemList( ImmutableList<SqlTrivia> leading, ISqlNode[] items, ImmutableList<SqlTrivia> trailing )
            : base( leading, items, trailing )
        {
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IReadOnlyList<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlExprRawItemList( leading, EnsureArray( children ), trailing );
        }

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor )
        {
            return visitor.Visit( this );
        }

    }


}
