#region Proprietary License
/*----------------------------------------------------------------------------
* This file (CK.SqlServer.Parser\Parser\SqlAnalyser.LowLevel.cs) is part of CK-Database. 
* Copyright © 2007-2014, Invenietis <http://www.invenietis.com>. All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using CK.SqlServer;

namespace CK.SqlServer.Parser
{
    public partial class SqlAnalyser
    {
        //public bool IsAnyToken( out SqlToken e, bool expected )
        //{
        //    e = null;
        //    return !R.IsErrorOrEndOfInput && R.IsToken( out e, expected );
        //}

        //public bool IsAny( out ISqlNode e, bool expected )
        //{
        //    e = null;
        //    SqlToken t;
        //    if( !R.IsToken( out t, expected ) ) return false;
        //    e = t;
        //    return true;
        //}

        /// <summary>
        /// Matches a list of comma separated expressions optionally enclosed in parenthesis.
        /// </summary>
        /// <typeparam name="T">Type of the expressions to match.</typeparam>
        /// <param name="openPar">Optional opening parenthesis.</param>
        /// <param name="items">List of items: contains expressions and comma tokens. Can be empty if no expression have been matched.</param>
        /// <param name="closePar">Closing parenthesis. Not null if and only if an opening parenthesis exists.</param>
        /// <param name="expectParenthesis">True to expect parenthesis. An error is set if the current token is not an opening parenthesis.</param>
        /// <param name="match">Function that knows how to match an expression.</param>
        /// <returns>True on success. Can be false only if <paramref name="expectParenthesis"/> is true.</returns>
        bool IsCommaList<T>( out SqlTokenOpenPar openPar, out List<ISqlNode> items, out SqlTokenClosePar closePar, bool expectParenthesis, IsFunc<T> match ) where T : class, ISqlNode 
        {
            items = null;
            closePar = null;

            if( !R.IsToken( out openPar, expectParenthesis ) && expectParenthesis )
            {
                Debug.Assert( R.IsError, "Set by R.IsToken." );
                return false;
            }
            items = new List<ISqlNode>();
            T item;
            if( !R.IsErrorOrEndOfInput && match( out item, false ) )
            {
                // Match may have returned null. this is the case for an empty list.
                if( item != null ) items.Add( item );
                SqlTokenComma comma;
                while( R.IsToken( out comma, false ) )
                {
                    items.Add( comma );
                    if( !match( out item, true ) )
                    {
                        if( !R.IsError ) R.SetCurrentError( "Match failed." );
                        break;
                    }
                    if( item != null ) items.Add( item );
                }
            }
            if( !R.IsError && openPar != null && !R.IsToken( out closePar, true ) )
            {
                Debug.Assert( R.IsError, "Set by R.IsToken." );
                return false;
            }
            return !R.IsError;
        }

        /// <summary>
        /// Matches a list of comma separated expressions not enclosed in parenthesis.
        /// </summary>
        /// <typeparam name="T">Type of the expressions to match.</typeparam>
        /// <param name="items">List of items: contains expressions and comma tokens. Can be empty if no expression have been matched.</param>
        /// <param name="match">Function that knows how to match an expression.</param>
        /// <returns>True on success. Can be false only when <paramref name="expectAtLeastOne"/> is true or if an open parenthesis has been found.</returns>
        bool IsCommaListNonEnclosed<T>( out List<ISqlNode> items, IsFunc<T> match, bool expectAtLeastOne ) where T : class, ISqlNode 
        {
            SqlTokenOpenPar openPar;
            SqlTokenClosePar closePar;
            if( !IsCommaList( out openPar, out items, out closePar, false, match ) ) return false;
            if( openPar != null ) return R.SetCurrentError( "Unexpected parenthesis." );
            if( expectAtLeastOne && items.Count == 0 ) return R.SetCurrentError( "Expected a '{0}' definition.", typeof( T ).Name.Replace( "SqlExpr", String.Empty ).Replace( "SqlNoExpr", String.Empty ) );
            return !R.IsError;
        }

        /// <summary>
        /// Combines multiple identifier into one <see cref="SqlMultiIdentifier"/> or returns a <see cref="SqlTokenIdentifier"/>.
        /// </summary>
        /// <param name="e">The resulting identifier.</param>
        /// <param name="expected">True to set an error if no identifier is matched.</param>
        /// <param name="first">Optional already read token.</param>
        /// <returns>True on success, otherwise false.</returns>
        bool IsIdentifier( out ISqlIdentifier e, bool expected, SqlTokenIdentifier first )
        {
            e = null;
            if( first == null && !R.IsToken( out first, expected ) ) return false;
            if( R.Current is ISqlTokenIdentifierSeparator )
            {
                List<ISqlNode> parts = new List<ISqlNode>();
                parts.Add( first );
                do
                {
                    // Adds the separator.
                    parts.Add( R.Read<SqlToken>() );
                    // Expects * or a token identifier.
                    if( R.Current.TokenType == SqlTokenType.Mult )
                    {
                        first = new SqlTokenIdentifier( SqlTokenType.IdentifierStar, "*", R.Current.LeadingTrivias, R.Current.TrailingTrivias );
                        R.MoveNext();
                    }
                    else if( !R.IsToken( out first, true ) ) return false;
                    parts.Add( first );
                }
                while( R.Current is ISqlTokenIdentifierSeparator );
                e = new SqlMultiIdentifier( parts );
            }
            else e = first;
            return true;
        }

        /// <summary>
        /// Combines multiple identifier into one <see cref="SqlMultiIdentifier"/> or returns a <see cref="SqlTokenIdentifier"/>.
        /// </summary>
        /// <param name="e">The resulting identifier.</param>
        /// <param name="expected">True to set an error if no identifier is matched.</param>
        /// <returns>True on success, otherwise false.</returns>
        bool IsIdentifier( out ISqlIdentifier e, bool expected )
        {
            return IsIdentifier( out e, expected, null );
        }

        SqlTokenTerminal GetOptionalTerminator()
        {
            SqlTokenTerminal term;
            R.IsToken( out term, SqlTokenType.SemiColon, false );
            return term;
        }
    }
}

