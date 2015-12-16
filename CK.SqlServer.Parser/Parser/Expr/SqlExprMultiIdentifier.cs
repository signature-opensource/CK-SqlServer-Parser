#region Proprietary License
/*----------------------------------------------------------------------------
* This file (CK.SqlServer.Parser\Parser\Expr\SqlExprMultiIdentifier.cs) is part of CK-Database. 
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

    public class SqlExprMultiIdentifier : SqlExpr, ISqlIdentifier
    {
        /// <summary>
        /// Initializes a new <see cref="SqlExprMultiIdentifier"/> that may be enclosed or not. 
        /// Separator is <see cref="IsDotOrDoubleColonSeparator"/>.
        /// </summary>
        /// <param name="isEnclosed">Whether given tokens are enclosed or not.</param>
        /// <param name="tokens">Identifiers and separator tokens. It may be enclosed or not.</param>
        public SqlExprMultiIdentifier( bool isEnclosed, IList<SqlNode> tokens )
            : this( null, Build( isEnclosed, tokens ), null )
        {
        }

        static SqlNode[] Build( bool isEnclosed, IList<SqlNode> tokens )
        {
            if( tokens.Count == 0 ) throw new ArgumentException();
            SqlNode[] r;
            if( isEnclosed ) r = tokens.ToArray();
            else r = CreateEnclosedArray( tokens.AsReadOnlyList() );
            SqlExprBaseListWithSeparator<SqlTokenIdentifier>.CheckArray( r, false, true, false, ISqlItemExtension.IsDotOrDoubleColonSeparator );
            return r;
        }

        internal SqlExprMultiIdentifier( ImmutableList<SqlTrivia> leading, SqlNode[] items, ImmutableList<SqlTrivia> trailing )
            : base( leading, items, trailing )
        {
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IReadOnlyList<SqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlExprMultiIdentifier( leading, EnsureArray( children ), trailing );
        }


        static internal string BuildArray( IEnumerator<SqlNode> tokens, out SqlNode[] result, SqlTokenIdentifier firstForLookup = null )
        {
            return SqlExprBaseListWithSeparator<SqlTokenIdentifier>.BuildArray( tokens, false, ISqlItemExtension.IsDotOrDoubleColonSeparator, "identifier", out result, firstForLookup );
        }

        /// <summary>
        /// Gets the number of <see cref="SeparatorTokens"/>.
        /// </summary>
        public int SeparatorCount { get { return (Slots.Length / 2) - 1; } }

        /// <summary>
        /// Gets the separators token.
        /// </summary>
        public IEnumerable<SqlTokenTerminal> SeparatorTokens { get { return ItemsWithoutParenthesis.Where( ( x, i ) => i % 2 != 0 ).Cast<SqlTokenTerminal>(); } }
        
        public SqlTokenIdentifier IdentifierAt( int index )
        {
            return (SqlTokenIdentifier)Slots[index * 2 + 1];
        }

        public int IdentifiersCount
        {
            get { return (Slots.Length - 1) / 2; }
        }

        public IEnumerable<SqlTokenIdentifier> Identifiers
        {
            get { return ItemsWithoutParenthesis.Where( ( x, i ) => i % 2 == 0 ).Cast<SqlTokenIdentifier>(); }
        }

        public SqlExprMultiIdentifier RemoveQuoteIfPossible( bool keepIfReservedKeyword )
        {
            SqlNode[] c = SqlExprBaseListWithSeparator<SqlTokenIdentifier>.ReplaceNonSeparator( Slots, true, t => t.RemoveQuoteIfPossible( keepIfReservedKeyword ) );
            return c != null ? new SqlExprMultiIdentifier( LeadingTrivias, c, TrailingTrivias ) : this;
        }

        [DebuggerStepThrough]
        internal protected override SqlNode Accept( SqlItemVisitor visitor )
        {
            return visitor.Visit( this );
        }

        bool ISqlIdentifier.IsVariable
        {
            get { return false; }
        }
    }

}
