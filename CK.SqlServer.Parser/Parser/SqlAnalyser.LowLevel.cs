using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using CK.SqlServer;
using CK.Core;

namespace CK.SqlServer.Parser
{
    public partial class SqlAnalyser
    {
        /// <summary>
        /// Reads a <see cref="SqlNodeList"/> until a stopper is found.
        /// </summary>
        /// <typeparam name="T">Stopper type.</typeparam>
        /// <param name="stopperDefinition"></param>
        /// <param name="matcher"></param>
        /// <param name="minCount"></param>
        /// <returns></returns>
        SqlNodeList IsSqlNodeList<T>( Predicate<T> stopperDefinition = null, Func<bool, ISqlNode> matcher = null, int minCount = 0 ) where T : SqlToken
        {
            List<ISqlNode> items = new List<ISqlNode>();
            if( !R.CollectUntil( items, matcher, stopperDefinition ) ) return null;
            if( items.Count < minCount )
            {
                R.SetCurrentError( "Expected at least {0} item(s).", minCount );
                return null;
            }
            return items.Count > 0 ? new SqlNodeList( items ) : SqlNodeList.Empty;
        }

        /// <summary>
        /// Reads a <see cref="SqlNodeList"/> until a stopper is found.
        /// </summary>
        /// <typeparam name="T">Stopper type.</typeparam>
        /// <param name="stopper"></param>
        /// <param name="stopperDefinition"></param>
        /// <param name="matcher"></param>
        /// <param name="minCount"></param>
        /// <returns></returns>
        SqlNodeList IsSqlNodeList<T>( out T stopper, Predicate<T> stopperDefinition = null, Func<bool, ISqlNode> matcher = null, int minCount = 0 ) where T : SqlToken
        {
            stopper = null;
            SqlNodeList result = IsSqlNodeList( stopperDefinition, matcher, minCount );
            if( result == null ) return null;
            stopper = (T)R.Current;
            R.MoveNext();
            return result;
        }

        /// <summary>
        /// Reads an enclosed comma separated list of extended expressions.
        /// </summary>
        /// <param name="expected">True to set an error if no enclosed list exists.</param>
        /// <returns>A <see cref="SqlEnclosedCommaList"/> or null.</returns>
        SqlEnclosedCommaList IsEnclosedCommaList( bool expected )
        {
            if( !expected && R.Current.TokenType != SqlTokenType.OpenPar ) return null;
            SqlTokenOpenPar openPar;
            SqlTokenClosePar closePar;
            List<ISqlNode> items = new List<ISqlNode>();
            if( !R.CollectCommaList( items, out openPar, out closePar, IsExtendedExpression, 0, Parenthesis.Required ) ) return null;
            return new SqlEnclosedCommaList( openPar, items, closePar );
        }

        /// <summary>
        /// Reads a comma separated list of extended expressions that may be enclosed or not in parenthesis.
        /// </summary>
        /// <param name="expected">True to set an error if no enclosed list exists.</param>
        /// <returns>A <see cref="SqlEnclosableCommaList"/> or null.</returns>
        SqlEnclosableCommaList IsEnclosableCommaList( bool expected, Parenthesis parenthesis = Parenthesis.Optional )
        {
            if( !expected 
                && parenthesis == Parenthesis.Required
                && R.Current.TokenType != SqlTokenType.OpenPar ) return null;
            SqlTokenOpenPar openPar;
            SqlTokenClosePar closePar;
            List<ISqlNode> items = new List<ISqlNode>();
            if( !R.CollectCommaList( items, out openPar, out closePar, IsExtendedExpression, 0, parenthesis ) ) return null;
            if( openPar == null && items.Count == 0 )
            {
                if( expected ) R.SetCurrentError( "Expected enclosable comma list." );
                return null;
            }
            return new SqlEnclosableCommaList( openPar, items, closePar );
        }

        /// <summary>
        /// Reads a typed list of nodes.
        /// </summary>
        /// <typeparam name="T">The type of nodes to read.</typeparam>
        /// <param name="atLeastOne">True to expect at least one item.</param>
        /// <param name="matcher">The node matcher.</param>
        /// <returns>Null on error or a posssibly empty list (if <paramref name="atLeastOne"/> is false).</returns>
        T IsList<T,TItem>( bool atLeastOne, Func<bool, TItem> matcher, Func<IEnumerable<TItem>,T> listCreator ) 
            where TItem : class, ISqlNode
            where T : ASqlNodeList<TItem>
        {
            TItem item = matcher( atLeastOne );
            if( item == null ) return R.IsError ? null : listCreator( Enumerable.Empty<TItem>() );
            TItem item2 = matcher( false );
            if( item2 == null ) return R.IsError ? null : listCreator( new CKReadOnlyListMono<TItem>( item ) );
            List<TItem> items = new List<TItem>();
            items.Add( item );
            items.Add( item2 );
            while( (item = matcher( false )) != null )
            {
                items.Add( item );
            }
            return R.IsError ? null : listCreator( items );
        }

        /// <summary>
        /// Reads an enclosed comma separated list of typed nodes.
        /// </summary>
        /// <typeparam name="T">The type of the created object.</typeparam>
        /// <param name="expected">True to expect at least one opening parenthesis.</param>
        /// <param name="minCount">Minimal number of objects.</param>
        /// <param name="typeCreator">The function that knows how to concretize a <typeparamref name="T"/>.</param>
        /// <returns>Null if no list has been found or if an error occured.</returns>
        T IsEnclosedCommaList<T,TItem>( bool expected, int minCount, Func<bool,TItem> matcher, Func<SqlTokenOpenPar,IEnumerable<ISqlNode>,SqlTokenClosePar,T> typeCreator )
            where TItem : class, ISqlNode
            where T : ASqlNodeEnclosableSeparatedList<SqlTokenOpenPar, TItem, SqlTokenComma, SqlTokenClosePar>
        {
            if( !expected && R.Current.TokenType != SqlTokenType.OpenPar ) return null;
            SqlTokenOpenPar openPar;
            SqlTokenClosePar closePar;
            List<ISqlNode> items = new List<ISqlNode>();
            if( !R.CollectCommaList( items, out openPar, out closePar, matcher, minCount, Parenthesis.Required ) ) return null;
            return typeCreator( openPar, items, closePar );
        }

        T IsCommaList<T, TItem>( int minCount, Func<bool, TItem> matcher, Func<IEnumerable<ISqlNode>, T> listCreator )
            where TItem : class, ISqlNode
            where T : ASqlNodeSeparatedList<TItem, SqlTokenComma>
        {
            List<ISqlNode> items = new List<ISqlNode>();
            if( !R.CollectCommaList( items, matcher, minCount ) ) return null;
            return listCreator( items );
        }

        SqlCommaList IsCommaList( int minCount, Func<bool, ISqlNode> matcher )
        {
            List<ISqlNode> items = new List<ISqlNode>();
            if( !R.CollectCommaList( items, matcher, minCount ) ) return null;
            return new SqlCommaList( items );
        }

        T IsPrefixedCommaList<T, TPrefix, TItem>( TPrefix prefix, int minCount, Func<bool, TItem> matcher, Func<TPrefix, IEnumerable<ISqlNode>, T> listCreator )
            where TPrefix : class, ISqlNode
            where TItem : class, ISqlNode
            where T : ASqlNodePrefixedSeparatedList<TPrefix, TItem, SqlTokenComma>
        {
            List<ISqlNode> items = new List<ISqlNode>();
            if( !R.CollectCommaList( items, matcher, minCount ) ) return null;
            return listCreator( prefix, items );
        }

        T IsIdentifierPrefixedCommaList<T, TItem>( bool expected, SqlTokenType prefixType, int minCount, Func<bool, TItem> matcher, Func<SqlTokenIdentifier, IEnumerable<ISqlNode>, T> listCreator )
            where TItem : class, ISqlNode
            where T : ASqlNodePrefixedSeparatedList<SqlTokenIdentifier, TItem, SqlTokenComma>
        {
            SqlTokenIdentifier prefix;
            if( !R.IsToken( out prefix, prefixType, expected ) ) return null;
            return IsPrefixedCommaList( prefix, minCount, matcher, listCreator );
        }

        T IsPrefixedEnclosedCommaList<T, TPrefix, TItem>( TPrefix prefix, int minCount, Func<bool, TItem> matcher, Func<TPrefix, SqlTokenOpenPar, IEnumerable<ISqlNode>, SqlTokenClosePar, T> listCreator )
            where TPrefix : class, ISqlNode
            where TItem : class, ISqlNode
            where T : ASqlNodePrefixedEnclosedSeparatedList<TPrefix, SqlTokenOpenPar, TItem, SqlTokenComma, SqlTokenClosePar>
        {
            List<ISqlNode> items = new List<ISqlNode>();
            SqlTokenOpenPar opener;
            SqlTokenClosePar closer;
            if( !R.CollectCommaList( items, out opener, out closer, matcher, minCount, Parenthesis.Required ) ) return null;
            return listCreator( prefix, opener, items, closer );
        }

        T IsIdentifierPrefixedCommaList<T, TItem>( bool expected, SqlTokenType prefixType, int minCount, Func<bool, TItem> matcher, Func<SqlTokenIdentifier, SqlTokenOpenPar, IEnumerable<ISqlNode>, SqlTokenClosePar, T> listCreator )
            where TItem : class, ISqlNode
            where T : ASqlNodePrefixedEnclosedSeparatedList<SqlTokenIdentifier, SqlTokenOpenPar, TItem, SqlTokenComma, SqlTokenClosePar>
        {
            SqlTokenIdentifier prefix;
            if( !R.IsToken( out prefix, prefixType, expected ) ) return null;
            return IsPrefixedEnclosedCommaList( prefix, minCount, matcher, listCreator );
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
            ISqlNode eFirst = first;
            if( first.TokenType == SqlTokenType.OpenDataSource )
            {
                var parameters = IsEnclosedCommaList( true );
                if( parameters == null ) return null;
                eFirst = new SqlOpenDataSource( first, parameters );
            }
            if( R.Current is ISqlTokenIdentifierSeparator )
            {
                List<ISqlNode> parts = new List<ISqlNode>();
                parts.Add( eFirst );
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

        /// <summary>
        /// A basic value is NULL, a variable name (like @varName) or a litteral value ('string', N'string', 90, 12e11)
        /// or the unary minus followed by a number.
        /// </summary>
        /// <param name="expected">True to set an error if not found.</param>
        /// <returns>The found basic value or null.</returns>
        SqlBasicValue IsBasicValue( bool expected )
        {
            SqlTokenIdentifier variable;
            if( R.IsToken( out variable, SqlTokenType.Null, false )
                || R.IsToken( out variable, SqlTokenType.IdentifierVariable, false ) )
            {
                return new SqlBasicValue( null, variable );
            }
            if( R.Current.TokenType == SqlTokenType.Minus )
            {
                if( (R.RawLookup.TokenType & SqlTokenType.IsNumber) != 0 )
                {
                    return new SqlBasicValue( R.Read<SqlTokenTerminal>(), R.Read<SqlToken>() );
                }
                if( expected ) R.SetCurrentError( "numerical value expected." );
                return null;
            }
            SqlTokenBaseLiteral value;
            if( !R.IsToken( out value, expected ) ) return null;
            return new SqlBasicValue( null, value );
        }

        SqlTokenTerminal GetOptionalTerminator()
        {
            SqlTokenTerminal term;
            R.IsToken( out term, SqlTokenType.SemiColon, false );
            return term;
        }
    }
}

