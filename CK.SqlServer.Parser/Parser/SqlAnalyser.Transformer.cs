using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using CK.Core;
using CK.SqlServer;

namespace CK.SqlServer.Parser
{
    public partial class SqlAnalyser
    {
        SqlTransformer MatchTransformer( SqlCreateOrAlter createOrAlter )
        {
            SqlTokenIdentifier type = R.Read<SqlTokenIdentifier>();
            Debug.Assert( type.TokenType == SqlTokenType.Transformer );

            ISqlIdentifier nameOrOnOrAs = IsIdentifier( true );
            SqlTokenIdentifier onT = null;
            ISqlIdentifier targetName = null;
            SqlTokenIdentifier asT = null;

            if( nameOrOnOrAs.IsToken( SqlTokenType.As ) )
            {
                asT = (SqlTokenIdentifier)nameOrOnOrAs;
                nameOrOnOrAs = null;
            }
            else
            {
                if( nameOrOnOrAs.IsToken( SqlTokenType.On ) )
                {
                    onT = (SqlTokenIdentifier)nameOrOnOrAs;
                    nameOrOnOrAs = null;
                }
                if( onT != null || R.IsToken( out onT, SqlTokenType.On, false ) )
                {
                    targetName = IsIdentifier( true );
                    if( targetName == null ) return null;
                }
            }
            if( asT == null && !R.IsToken( out asT, SqlTokenType.As, true ) ) return null;

            SqlTokenIdentifier beginT;
            if( !R.IsToken( out beginT, SqlTokenType.Begin, true ) ) return null;
            SqlTStatementList block = IsList( false, IsTransformStatement, statements => R.IsToken( out SqlTokenIdentifier endT, SqlTokenType.End, true )
                                                                                        ? new SqlTStatementList( beginT, statements, endT )
                                                                                        : null );
            if( block == null ) return null;

            return new SqlTransformer( createOrAlter, type, nameOrOnOrAs, onT, targetName, asT, block, GetOptionalTerminator() );
        }

        ISqlTStatement IsTransformStatement( bool expected )
        {
            SqlTokenIdentifier initT;
            if( R.IsToken( out initT, SqlTokenType.Add, false ))
            {
                SqlTokenIdentifier whatT;
                if( R.IsToken( out whatT, SqlTokenType.Parameter, false ) )
                {
                    SqlParameterList parameters = IsParameterList( Parenthesis.Rejected, 1 );
                    if( parameters == null ) return null;
                    SqlTokenIdentifier afterOrBeforeT;
                    SqlTokenIdentifier paramName = null;
                    if( R.IsToken( out afterOrBeforeT, SqlTokenType.After, false ) || R.IsToken( out afterOrBeforeT, SqlTokenType.Before, false ) )
                    {
                        if( !R.IsToken( out paramName, t => t.IsVariable, true ) ) return null;
                    }
                    return new SqlTAddParameter( initT, whatT, parameters, afterOrBeforeT, paramName, GetOptionalTerminator() );
                }
                if( R.IsToken( out whatT, SqlTokenType.Column, false ) )
                {
                    SelectColumnList columns = IsCommaList( 1, IsSelectColumn, i => new SelectColumnList( i ) );
                    if( columns == null ) return null;

                    return new SqlTAddColumn( initT, whatT, columns, GetOptionalTerminator() );
                }
            }
            else if( R.IsToken( out initT, SqlTokenType.Inject, false ) )
            {
                ISqlHasStringValue content = IsSqlHasStringValue( true );
                if( content == null ) return null;

                SqlTokenIdentifier intoT;
                ISqlHasStringValue target = null;
                if( R.IsToken( out intoT, SqlTokenType.Into, false ) )
                {
                    target = IsSqlHasStringValue( true );
                    if( target == null ) return null;
                    return new SqlTInjectInto( initT, content, intoT, target, GetOptionalTerminator() );
                }

                SqlTokenIdentifier andT;
                ISqlHasStringValue content2 = null;
                if( R.IsToken( out andT, SqlTokenType.And, false ) )
                {
                    content2 = IsSqlHasStringValue( true );
                    if( content2 == null ) return null;
                }

                SqlTokenIdentifier beforeAfterOrAroundT;
                if( content2 != null )
                {
                    if( !R.IsToken( out beforeAfterOrAroundT, SqlTokenType.Around, true ) ) return null;
                }
                else
                {
                    if( !R.IsToken( out beforeAfterOrAroundT, SqlTokenType.Before, false )
                        && !R.IsToken( out beforeAfterOrAroundT, SqlTokenType.After, true ) ) return null;
                }

                ISqlTLocationFinder loc = IsISqlTLocationFinder( true );
                if( loc == null ) return null;

                return new SqlTInject( initT, content, andT, content2, beforeAfterOrAroundT, loc, GetOptionalTerminator() );
            }
            else if( R.IsToken( out initT, SqlTokenType.Replace, false ) )
            {
                ISqlTLocationFinder loc = IsISqlTLocationFinder( true );
                if( loc == null ) return null;

                SqlTokenIdentifier withT;
                if( !R.IsToken( out withT, SqlTokenType.With, true ) ) return null;

                ISqlHasStringValue content = IsSqlHasStringValue( true );
                if( content == null ) return null;

                return new SqlTReplace( initT, loc, withT, content, GetOptionalTerminator() );
            }
            else if( R.IsToken( out initT, SqlTokenType.In, false ) )
            {
                return MatchSqlTInScope( initT );
            }
            else if( R.IsToken( out initT, SqlTokenType.Combine, false ) )
            {
                SqlTokenIdentifier selectT, operatorT, allT = null, withT;
                if( !R.IsToken( out selectT, SqlTokenType.Select, true ) ) return null;
                if( !R.IsToken( out operatorT, SqlTokenType.Intersect, false )
                    || !R.IsToken( out operatorT, SqlTokenType.Except, false ) )
                {
                    if( !R.IsToken( out operatorT, SqlTokenType.Union, true ) ) return null;
                    R.IsToken( out allT, SqlTokenType.All, false );
                }
                if( !R.IsToken( out withT, SqlTokenType.With, true ) ) return null;
                ISqlNamedStatement select = IsNamedStatement( true );
                if( select == null ) return null;
                return new SqlTCombineSelect( initT, selectT, operatorT, allT, withT, select, GetOptionalTerminator() );
            }
            if( expected ) R.SetCurrentError( "Expected transform statement." );
            return null;
        }
        SqlTInScope MatchSqlTInScope( SqlTokenIdentifier inT )
        {
            Debug.Assert( inT.TokenType == SqlTokenType.In && !R.IsError );
            ISqlNode loc = IsISqlTLocationFinder( false );
            if( loc == null && !R.IsError ) loc = IsSqlTRangeLocationFinder( false );
            if( loc == null )
            {
                if( !R.IsError ) R.SetCurrentError( "Expected 'after', 'before', 'between' or 'first', 'last', 'single', 'each' or 'all' token." );
                return null;
            }
            SqlTokenIdentifier subordinatedInT;
            if( R.IsToken( out subordinatedInT, SqlTokenType.In, false ) )
            {
                SqlTInScope sub = MatchSqlTInScope( subordinatedInT );
                if( sub == null ) return null;
                return new SqlTInScope( inT, loc, sub, GetOptionalTerminator() );
            }
            SqlTokenIdentifier begintT;
            if( !R.IsToken( out begintT, SqlTokenType.Begin, true ) ) return null;

            SqlTStatementList block = IsList( false, IsTransformStatement, statements => R.IsToken( out SqlTokenIdentifier endT, SqlTokenType.End, true )
                                                                                        ? new SqlTStatementList( begintT, statements, endT )
                                                                                        : null );
            if( block == null ) return null;

            return new SqlTInScope( inT, loc, block, GetOptionalTerminator() );
        }


        public SqlTOneLocationFinder IsSqlTOneLocationFinder( bool expected )
        {
            SqlTokenIdentifier firstOrLastOrSingle;
            SqlTokenTerminal plusOrMinusT = null;
            SqlTokenLiteralInteger offset = null;
            if( R.IsToken( out firstOrLastOrSingle, SqlTokenType.First, false ) )
            {
                if( R.IsToken( out plusOrMinusT, SqlTokenType.Plus, false ) )
                {
                    if( !R.IsToken( out offset, true ) ) return null;
                }
            }
            else if( R.IsToken( out firstOrLastOrSingle, SqlTokenType.Last, false ) )
            {
                if( R.IsToken( out plusOrMinusT, SqlTokenType.Minus, false ) )
                {
                    if( !R.IsToken( out offset, true ) ) return null;
                }
            }
            else if( !R.IsToken( out firstOrLastOrSingle, SqlTokenType.Single, false ) )
            {
                if( expected ) R.SetCurrentError( "Expected: first [+n] | last [-n] | single." );
                return null;
            }
            SqlTokenIdentifier outT, ofT = null;
            SqlTokenLiteralInteger expectedMatchCount = null;
            if( R.IsToken( out outT, SqlTokenType.Out, false ) )
            {
                if( !R.IsToken( out ofT, SqlTokenType.Of, true ) ) return null;
                if( !R.IsToken( out expectedMatchCount, true ) ) return null;
                if( firstOrLastOrSingle.TokenType == SqlTokenType.Single )
                {
                    R.SetCurrentError( "Invalid 'out of n' cardinality specification after 'single'." );
                    return null;
                }
            }
            ISqlNode textOrSimplePattern = IsTextOrSimpleMatchPattern( true );
            return textOrSimplePattern != null
                    ? new SqlTOneLocationFinder( firstOrLastOrSingle, plusOrMinusT, offset, outT, ofT, expectedMatchCount, textOrSimplePattern )
                    : null;
        }

        public SqlTRangeLocationFinder IsSqlTRangeLocationFinder( bool expected )
        {
            SqlTokenIdentifier afterOrBeforeOrBetween;
            if( !R.IsToken( out afterOrBeforeOrBetween, SqlTokenType.After, false )
                && !R.IsToken( out afterOrBeforeOrBetween, SqlTokenType.Before, false )
                && !R.IsToken( out afterOrBeforeOrBetween, SqlTokenType.Between, false ) )
            {
                if( expected ) R.SetCurrentError( "Expected after | before | between." );
                return null;
            }
            SqlTOneLocationFinder firstLoc = IsSqlTOneLocationFinder( true );
            if( firstLoc == null ) return null;
            SqlTokenIdentifier andT = null;
            SqlTOneLocationFinder secondLoc = null;
            if( afterOrBeforeOrBetween.TokenType == SqlTokenType.Between )
            {
                if( !R.IsToken( out andT, SqlTokenType.And, true ) ) return null;
                secondLoc = IsSqlTOneLocationFinder( true );
                if( secondLoc == null ) return null;
            }
            return new SqlTRangeLocationFinder( afterOrBeforeOrBetween, firstLoc, andT, secondLoc );
        }

        public SqlTMultiLocationFinder IsSqlTMultiLocationFinder( bool expected )
        {
            SqlTokenIdentifier allOrEach;
            if( !R.IsToken( out allOrEach, SqlTokenType.All, false )
                && !R.IsToken( out allOrEach, SqlTokenType.Each, false ) )
            {
                if( expected ) R.SetCurrentError( "Expected: all | each token." );
                return null;
            }
            SqlTokenLiteralInteger expectedMatchCount;
            R.IsToken( out expectedMatchCount, false );

            ISqlNode textOrSimplePattern = IsTextOrSimpleMatchPattern( true ); ;
            return textOrSimplePattern != null
                   ? new SqlTMultiLocationFinder( allOrEach, expectedMatchCount, textOrSimplePattern )
                   : null;
        }

        /// <summary>
        /// Parses a <see cref="SqlTMultiLocationFinder"/> or <see cref="SqlTOneLocationFinder"/>.
        /// </summary>
        /// <param name="expected">Whether the finder must exist.</param>
        /// <returns>The finder or null.</returns>
        public ISqlTLocationFinder IsISqlTLocationFinder( bool expected )
        {
            SqlTMultiLocationFinder m = IsSqlTMultiLocationFinder( false );
            if( m != null ) return m;
            if( !R.IsError )
            {
                SqlTOneLocationFinder o = IsSqlTOneLocationFinder( false );
                if( o != null ) return o;
                if( !R.IsError && expected ) R.SetCurrentError( "Missing first | last | single | all | each." );
            }
            return null;
        }

        ISqlNode IsTextOrSimpleMatchPattern( bool expected )
        {
            ISqlNode textOrSimplePattern = IsTNodeSimplePattern( false );
            if( R.IsError ) return null;
            if( textOrSimplePattern == null )
            {
                ISqlHasStringValue text = R.Current as ISqlHasStringValue;
                if( text != null )
                {
                    if( !text.Value.StartsWith( "--" )
                        && (!text.Value.StartsWith( "/*" ) || !text.Value.EndsWith( "*/" )) )
                    {
                        R.SetCurrentError( @"Litteral string must start with -- or starts and ends with /* and */." );
                        return null;
                    }
                    R.MoveNext();
                    textOrSimplePattern = text;
                }
            }
            if( textOrSimplePattern == null )
            {
                if( expected ) R.SetCurrentError( @"Expected: string litteral [...] or ""..."" or '...' or {pattern}." );
                return null;
            }
            return textOrSimplePattern;
        }

        SqlTNodeSimplePattern IsTNodeSimplePattern( bool expected )
        {
            SqlTokenIdentifier matchKindT;
            if( R.IsToken( out matchKindT, SqlTokenType.Part, false )
                || R.IsToken( out matchKindT, SqlTokenType.Statement, false )
                || R.IsToken( out matchKindT, SqlTokenType.Range, false ) )
            {
                expected = true;
            }
            SqlTCurlyPattern pattern = IsTCurlyPattern( expected );
            if( pattern == null ) return null;
            return new SqlTNodeSimplePattern( matchKindT, pattern );
        }

        SqlTCurlyPattern IsTCurlyPattern( bool expected )
        {
            SqlTokenTerminal opener;
            if( !R.IsToken( out opener, SqlTokenType.OpenCurly, expected ) ) return null;
            var tokens = new List<SqlToken>();
            while( R.Current.TokenType != SqlTokenType.CloseCurly )
            {
                if( R.IsErrorOrEndOfInput )
                {
                    R.SetCurrentError( "Expected closing '}'." );
                    return null;
                }
                tokens.Add( R.Current );
                R.MoveNext();
            }
            if( tokens.Count == 0 )
            {
                R.SetCurrentError( "Expected at least one token in pattern." );
                return null;
            }
            SqlTokenTerminal closer = R.Read<SqlTokenTerminal>();
            return new SqlTCurlyPattern( opener, tokens, closer );
        }

    }


}

