#region Proprietary License
/*----------------------------------------------------------------------------
* This file (CK.SqlServer.Parser\Parser\Expr\Base\SqlExpr.cs) is part of CK-Database. 
* Copyright © 2007-2014, Invenietis <http://www.invenietis.com>. All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using CK.Core;
using System.Diagnostics;
using System.Globalization;
using System.Collections.Immutable;

namespace CK.SqlServer.Parser
{
    /// <summary>
    /// SqlExpr is a SqlItem with optionals <see cref="Opener"/> and <see cref="Closer"/>.
    /// </summary>
    public abstract class SqlExpr : SqlItem
    {
        internal SqlExpr( ImmutableList<SqlTrivia> leading, SqlNode[] slots, ImmutableList<SqlTrivia> trailing )
            : base( leading, slots, trailing )
        {
            Debug.Assert( slots != null && slots.Length >= 2 && slots[0] is SqlTokenList<SqlTokenOpenPar> && slots[slots.Length - 1] is SqlTokenList<SqlTokenClosePar> );
        }

        /// <summary>
        /// Gets the opening parenthesis. Can be empty.
        /// </summary>
        public SqlTokenList<SqlTokenOpenPar> Opener { get { return (SqlTokenList<SqlTokenOpenPar>)Slots[0]; } }

        /// <summary>
        /// Gets the closing parenthesis. Can be empty.
        /// </summary>
        public SqlTokenList<SqlTokenClosePar> Closer { get { return (SqlTokenList<SqlTokenClosePar>)Slots[Slots.Length-1]; } }

        /// <summary>
        /// Gets the last token of the expression.
        /// </summary>
        public sealed override SqlToken LastOrEmptyT { get { return Closer.Tokens.Count > 0 ? Closer.LastOrEmptyT : Slots[Slots.Length - 2].LastOrEmptyT; } }

        /// <summary>
        /// Gets the first token of the expression.
        /// </summary>
        public sealed override SqlToken FirstOrEmptyT { get { return Opener.Tokens.Count > 0 ? Opener.FirstOrEmptyT : Slots[1].FirstOrEmptyT; } }

        /// <summary>
        /// Gets the sql items without the enclosing parenthesis if they exist.
        /// </summary>
        public IEnumerable<ISqlItem> ItemsWithoutParenthesis { get { return Slots.Skip( 1 ).Take( Slots.Length - 2 ); } }

        /// <summary>
        /// Gets the tokens without the enclosing parenthesis if they exist.
        /// </summary>
        public IEnumerable<SqlToken> TokensWithoutParenthesis { get { return Flatten( ItemsWithoutParenthesis ); } }

        /// <summary>
        /// Gets whether this expression is an only token of the given type (by default without any enclosing parenthesis).
        /// </summary>
        /// <param name="type">The token type.</param>
        /// <param name="allowEnclosingParenthesis">True to allow enclosing parenthesis.</param>
        /// <returns>True if this is single token of the given type.</returns>
        public bool IsToken( SqlTokenType type, bool allowEnclosingParenthesis = false )
        {
            return Slots.Length == 3 && Slots[1].IsToken( type ) && (allowEnclosingParenthesis || Opener.Tokens.Count == 0);
        }

        internal SqlExpr MutableEnclose( SqlTokenOpenPar openPar, SqlTokenClosePar closePar )
        {
            Slots[0] = SqlTokenList<SqlTokenOpenPar>.Create( openPar, Opener );
            Slots[Slots.Length-1] = SqlTokenList<SqlTokenClosePar>.Create( Closer, closePar );
            return this;
        }

        /// <summary>
        /// Creates items betweem parenthesis.
        /// </summary>
        /// <param name="openPar">Opening parenthesis token.</param>
        /// <param name="closePar">Closing parenthesis token.</param>
        /// <returns>An array of <see cref="ISqlItem"/>.</returns>
        protected ISqlItem[] EncloseComponents( SqlTokenOpenPar openPar, SqlTokenClosePar closePar )
        {
            return CreateEnclosedArray( openPar, Slots, closePar );
        }

    }

}
