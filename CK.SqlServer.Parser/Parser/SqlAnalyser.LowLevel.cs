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
        bool IsSqlNodeList<T>( out SqlNodeList e, out T stopper, Predicate<T> stopperDefinition = null, Func<bool, ISqlNode> matcher = null, int minCount = 0 ) where T : SqlToken
        {
            e = null;
            List<ISqlNode> items = new List<ISqlNode>();
            if( !R.CollectUntil<T>( items, out stopper, matcher, stopperDefinition ) ) return false;
            if( items.Count < minCount ) return R.SetCurrentError( "Expected at least {0} item(s).", minCount );
            e = new SqlNodeList( items );
            return true;
        }

        bool IsSqlNodeList<T>( out SqlNodeList e, Predicate<T> stopperDefinition = null, Func<bool, ISqlNode> matcher = null, int minCount = 0 ) where T : SqlToken
        {
            e = null;
            List<ISqlNode> items = new List<ISqlNode>();
            if( !R.CollectUntil<T>( items, matcher, stopperDefinition ) ) return false;
            if( items.Count < minCount ) return R.SetCurrentError( "Expected at least {0} item(s).", minCount );
            e = new SqlNodeList( items );
            return true;
        }

        /// <summary>
        /// Reads a comma separated list of extended expressions that may be enclosed or not in parenthesis.
        /// </summary>
        /// <param name="expected">True to set an error if no enclosed list exists.</param>
        /// <returns>A <see cref="SqlEnclosedCommaList"/> or null.</returns>
        SqlEnclosedCommaList IsEnclosedCommaList( bool expected, Parenthesis parenthesis = Parenthesis.Optional )
        {
            if( !expected && R.Current.TokenType != SqlTokenType.OpenPar ) return null;
            SqlTokenOpenPar openPar;
            SqlTokenClosePar closePar;
            List<ISqlNode> items = new List<ISqlNode>();
            if( !R.CollectCommaList( items, out openPar, out closePar, IsExtendedExpression, 0, parenthesis ) ) return null;
            return new SqlEnclosedCommaList( openPar, items, closePar );
        }

        /// <summary>
        /// Combines multiple identifier into one <see cref="SqlMultiIdentifier"/> or 
        /// returns a <see cref="SqlTokenIdentifier"/>.
        /// </summary>
        /// <param name="expected">True to set an error if no identifier is matched.</param>
        /// <param name="first">Optional already read token.</param>
        /// <returns>The resulting identifier.</returns>
        ISqlIdentifier IsIdentifier( bool expected, SqlTokenIdentifier first )
        {
            if( first == null && !R.IsToken( out first, expected ) ) return null;
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
                    else if( !R.IsToken( out first, true ) ) return null;
                    parts.Add( first );
                }
                while( R.Current is ISqlTokenIdentifierSeparator );
                return new SqlMultiIdentifier( parts );
            }
            return first;
        }

        /// <summary>
        /// Combines multiple identifier into one <see cref="SqlMultiIdentifier"/> or 
        /// returns a <see cref="SqlTokenIdentifier"/>.
        /// </summary>
        /// <param name="expected">True to set an error if no identifier is matched.</param>
        /// <returns>The resulting identifier.</returns>
        ISqlIdentifier IsIdentifier( bool expected )
        {
            return IsIdentifier( expected, null );
        }

        SqlTokenTerminal GetOptionalTerminator()
        {
            SqlTokenTerminal term;
            R.IsToken( out term, SqlTokenType.SemiColon, false );
            return term;
        }
    }
}

