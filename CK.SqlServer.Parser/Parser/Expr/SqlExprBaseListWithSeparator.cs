#region Proprietary License
/*----------------------------------------------------------------------------
* This file (CK.SqlServer.Parser\Parser\Expr\SqlExprBaseListWithSeparator.cs) is part of CK-Database. 
* Copyright © 2007-2014, Invenietis <http://www.invenietis.com>. All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using CK.Core;
using System.Collections.Immutable;

namespace CK.SqlServer.Parser
{
    public abstract class SqlExprBaseListWithSeparator<T> : SqlExpr where T : SqlNode
    {
        /// <summary>
        /// Initializes a new <see cref="SqlExprBaseListWithSeparator{T}"/> of <see cref="SqlNode"/> enclosed in a <see cref="SqlTokenOpenPar"/> and a <see cref="SqlTokenClosePar"/> 
        /// and with <paramref name="validSeparator"/> that is set to <see cref="IsCommaSeparator"/> by default.
        /// </summary>
        /// <param name="openPar">Opening parenthesis.</param>
        /// <param name="exprOrCommaTokens">List of tokens or expressions.</param>
        /// <param name="closePar">Closing parenthesis.</param>
        /// <param name="allowEmpty">False to throw an argument exception if the <paramref name="exprOrCommaTokens"/> is empty.</param>
        /// <param name="validSeparator">Defaults to a predicate that checks that separators are commas (see <see cref="IsCommaSeparator"/>).</param>
        public SqlExprBaseListWithSeparator( SqlTokenOpenPar openPar, IList<SqlNode> exprOrTokens, SqlTokenClosePar closePar, bool allowEmpty, Predicate<SqlNode> validSeparator = null )
            : this( null, Build( openPar, exprOrTokens, closePar, allowEmpty, validSeparator ), null )
        {
        }

        /// <summary>
        /// Initializes a new <see cref="SqlExprBaseListWithSeparator{T}"/> of <typeparamref name="T"/> without <see cref="Opener"/> nor <see cref="Closer"/> 
        /// and with <paramref name="validSeparator"/> that is set to <see cref="SqlToken.IsCommaSeparator"/> by default.
        /// </summary>
        /// <param name="exprOrTokens">List of tokens or expressions.</param>
        /// <param name="validSeparator">Defaults to a predicate that checks that separators are commas (see <see cref="SqlToken.IsCommaSeparator"/>).</param>
        public SqlExprBaseListWithSeparator( IList<SqlNode> exprOrTokens, bool allowEmpty, Predicate<SqlNode> validSeparator = null )
            : this( null, Build( exprOrTokens, allowEmpty, validSeparator ), null )
        {
        }

        static SqlNode[] Build( SqlTokenOpenPar openPar, IList<SqlNode> exprOrTokens, SqlTokenClosePar closePar, bool allowEmpty, Predicate<SqlNode> validSeparator = null )
        {
            if( openPar == null ) throw new ArgumentNullException( "openPar" );
            if( exprOrTokens == null ) throw new ArgumentNullException( "exprOrTokens" );
            if( closePar == null ) throw new ArgumentNullException( "closePar" );
            var c = CreateArray( openPar, exprOrTokens, exprOrTokens.Count, closePar );
            CheckArray( c, allowEmpty, true, true, validSeparator ?? IsCommaSeparator );
            return c;
        }

        static SqlNode[] Build( IList<SqlNode> exprOrTokens, bool allowEmpty, Predicate<SqlNode> validSeparator = null )
        {
            if( exprOrTokens == null ) throw new ArgumentNullException( "exprOrTokens" );
            var c = CreateArray( SqlTokenList<SqlTokenOpenPar>.Empty, exprOrTokens, 0, exprOrTokens.Count, SqlTokenList<SqlTokenClosePar>.Empty );
            CheckArray( c, allowEmpty, true, false, validSeparator ?? IsCommaSeparator );
            return c;
        }

        internal SqlExprBaseListWithSeparator( ImmutableList<SqlTrivia> leading, SqlNode[] components, ImmutableList<SqlTrivia> trailing )
            : base( leading, components, trailing )
        {
        }

        /// <summary>
        /// Gets the number of <see cref="SeparatorTokens"/>.
        /// </summary>
        public int SeparatorCount { get { return Slots.Length / 2 - 1; } }

        /// <summary>
        /// Gets the separators token.
        /// </summary>
        public IEnumerable<SqlNode> SeparatorTokens { get { return ItemsWithoutParenthesis.Skip( 1 ).Where( ( x, i ) => i % 2 != 0 ); } }

        protected SqlNode SeparatorTokenAt( int i ) { return Slots[(i+1) * 2]; }

        protected int NonSeparatorCount { get { return (Slots.Length + 1) / 2 - 1; } }

        protected IEnumerable<T> NonSeparatorTokens { get { return ItemsWithoutParenthesis.Where( ( x, i ) => i % 2 == 0 ).Cast<T>(); } }

        protected T NonSeparatorTokenAt( int i ) { return (T)Slots[i* 2+1]; }

        [Conditional("DEBUG")]
        protected static void DebugCheckArray( SqlNode[] t, bool allowEmpty, bool hasOpenerAndCloser, bool atLeastOneOpener, Predicate<SqlNode> validSeparator )
        {
            CheckArray( t, allowEmpty, hasOpenerAndCloser, atLeastOneOpener, validSeparator );
        }

        /// <summary>
        /// True if the <see cref="SqlNode"/> is a comma token.
        /// </summary>
        /// <param name="t">Potential comma token.</param>
        /// <returns>Whether the token is a comma or not.</returns>
        static internal bool IsCommaSeparator( SqlNode t )
        {
            return t.IsToken( SqlTokenType.Comma );
        }

        internal static void CheckArray( SqlNode[] t, bool allowEmpty, bool hasOpenerAndCloser, bool atLeastOneOpener, Predicate<SqlNode> validSeparator )
        {
            int len = t.Length;
            int offset = 0;
            if( hasOpenerAndCloser )
            {
                len -= 2;
                offset = 1;
                if( len < 0 ) throw new ArgumentException( "There must be at least the opener/closer pair.", "tokens" );
                SqlTokenList<SqlTokenOpenPar> opener = t[0] as SqlTokenList<SqlTokenOpenPar>;
                SqlTokenList<SqlTokenClosePar> closer = t[t.Length - 1] as SqlTokenList<SqlTokenClosePar>;
                if( opener == null || closer == null ) throw new ArgumentException( "Opener/Closer not found.", "tokens" );
                if( opener.Tokens.Count != closer.Tokens.Count ) throw new ArgumentException( "Opener/Closer are not balanced.", "tokens" );
                if( atLeastOneOpener && opener.Tokens.Count == 0 ) throw new ArgumentException( "There must be at least one parenthesis.", "tokens" );
            }
            if( (len % 2) == 0 && (len != 0 || !allowEmpty) ) throw new ArgumentException( "There must be an odd number of elements.", "tokens" );
            len = (len + 1) / 2;
            for( int i = 0; i < len; ++i )
            {
                if( !(t[i * 2 + offset] is T) )
                {
                    throw new ArgumentException( String.Format( "Invalid token at {0}. It must be {1}.", i * 2, typeof( T ).Name ), "tokens" );
                }
                if( validSeparator != null && i > 0 )
                {
                    if( !validSeparator( t[i * 2 - 1 + offset] ) )
                    {
                        throw new ArgumentException( String.Format( "Invalid separator at {0}.", i * 2 - 1, typeof( T ).Name ), "tokens" );
                    }
                }
            }
        }

        internal static string BuildArray( IEnumerator<SqlNode> tokens, bool allowEmpty, Predicate<SqlNode> validSeparator, string elementName, out SqlNode[] result, T firstForLookup = null )
        {
            Debug.Assert( tokens != null );
            result = null;
            List<SqlNode> all = new List<SqlNode>();
            SqlNode element = firstForLookup;
            if( element != null ) all.Add( firstForLookup );
            else 
            {
                element = tokens.Current;
                if( element is T )
                {
                    all.Add( element );
                    if( !tokens.MoveNext() )
                    {
                        result = all.ToArray();
                        return null;
                    }
                }
            }
            if( all.Count > 0 )
            {
                SqlNode separator;
                while( validSeparator( separator = tokens.Current ) )
                {
                    if( !tokens.MoveNext() || !((element = tokens.Current) is T) )
                    {
                        return String.Format( "Missing {0} after {1}.", elementName, separator.ToString() );
                    }
                    all.Add( separator );
                    all.Add( element );
                    if( !tokens.MoveNext() ) break;
                }
            }
            if( all.Count == 0 && !allowEmpty ) return String.Format( "Expected {0}.", elementName );
            result = all.ToArray();
            return null;
        }

        protected SqlNode[] ReplaceNonSeparator( Func<T, SqlNode> replacer )
        {
            return ReplaceNonSeparator( Slots, true, replacer );
        }

        internal static SqlNode[] ReplaceNonSeparator( SqlNode[] t, bool hasOpenerAndCloser, Func<T, SqlNode> replacer )
        {
            SqlNode[] modified = null;
            int len = t.Length;
            int i = 0;
            if( hasOpenerAndCloser )
            {
                len -= 1;
                i = 1;
            }
            for(; i < len; i += 2 )
            {
                var o = (T)t[i];
                SqlNode r = replacer( o );
                if( !ReferenceEquals( r, o ) )
                {
                    if( modified == null ) modified = (SqlNode[])t.Clone();
                    modified[i] = r;
                }
            }
            return modified;
        }
    }
}
