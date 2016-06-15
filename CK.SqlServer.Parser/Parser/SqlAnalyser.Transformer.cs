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
        SqlTransformer MatchTransformer( SqlTokenIdentifier alterOrCreate )
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

            SqlTokenIdentifier begintT;
            if( !R.IsToken( out begintT, SqlTokenType.Begin, true ) ) return null;

            SqlTStatementList s = IsList( false, IsTransformStatement, statements => new SqlTStatementList( statements ) );
            if( s == null ) return null;

            SqlTokenIdentifier endT;
            if( !R.IsToken( out endT, SqlTokenType.End, true ) ) return null;

            return new SqlTransformer( alterOrCreate, type, nameOrOnOrAs, onT, targetName, asT, begintT, s, endT, GetOptionalTerminator() );
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

                SqlTLocationFinder loc = IsSqlTLocationFinder( true );
                if( loc == null ) return null;

                return new SqlTInject( initT, content, andT, content2, beforeAfterOrAroundT, loc, GetOptionalTerminator() );
            }
            else if( R.IsToken( out initT, SqlTokenType.Replace, false ) )
            {
                SqlTLocationFinder loc = IsSqlTLocationFinder( true );
                if( loc == null ) return null;

                SqlTokenIdentifier withT;
                if( !R.IsToken( out withT, SqlTokenType.With, true ) ) return null;

                ISqlHasStringValue content = IsSqlHasStringValue( true );
                if( content == null ) return null;

                return new SqlTReplace( initT, loc, withT, content, GetOptionalTerminator() );
            }
            else if( R.IsToken( out initT, SqlTokenType.In, false ) )
            {
                SqlTLocationFinder loc = IsSqlTLocationFinder( true );
                if( loc == null ) return null;

                SqlTokenIdentifier begintT;
                if( !R.IsToken( out begintT, SqlTokenType.Begin, true ) ) return null;

                SqlTStatementList s = IsList( false, IsTransformStatement, statements => new SqlTStatementList( statements ) );
                if( s == null ) return null;

                SqlTokenIdentifier endT;
                if( !R.IsToken( out endT, SqlTokenType.End, true ) ) return null;

                return new SqlTInScope( initT, loc, begintT, s, endT, GetOptionalTerminator() );
            }
            if( expected ) R.SetCurrentError( "Expected transform statement." );
            return null;
        }


        SqlTLocationFinder IsSqlTLocationFinder( bool expected )
        {
            SqlTokenIdentifier firstOrLastOrSingleOrAll;
            SqlTokenTerminal plusOrMinusT = null;
            SqlTokenLiteralInteger offset = null;
            if( R.IsToken( out firstOrLastOrSingleOrAll, SqlTokenType.First, false ) )
            {
                if( R.IsToken( out plusOrMinusT, SqlTokenType.Plus, false ) )
                {
                    if( !R.IsToken( out offset, true ) ) return null;
                }
            }
            else if( R.IsToken( out firstOrLastOrSingleOrAll, SqlTokenType.Last, false ) )
            {
                if( R.IsToken( out plusOrMinusT, SqlTokenType.Minus, false ) )
                {
                    if( !R.IsToken( out offset, true ) ) return null;
                }
            }
            else if( !R.IsToken( out firstOrLastOrSingleOrAll, SqlTokenType.Single, false ) 
                     && !R.IsToken( out firstOrLastOrSingleOrAll, SqlTokenType.All, false ) )
            {
                if( expected ) R.SetCurrentError( "Expected: first [+n] | last [-n] | single | all." );
                return null;
            }
            SqlTokenIdentifier outT, ofT = null;
            SqlTokenLiteralInteger expectedMatchCount = null;
            if( R.IsToken( out outT, SqlTokenType.Out, false ) )
            {
                if( !R.IsToken( out ofT, SqlTokenType.Of, true ) ) return null;
                if( !R.IsToken( out expectedMatchCount, true ) ) return null;
                if( firstOrLastOrSingleOrAll.TokenType == SqlTokenType.Single )
                {
                    R.SetCurrentError( "Invalid 'out of n' specification after 'single'." );
                    return null;
                }
            }
            else if( firstOrLastOrSingleOrAll.TokenType == SqlTokenType.All )
            {
                R.IsToken( out expectedMatchCount, false );
            }

            ISqlNode textOrSimplePattern = IsTNodeSimplePattern( false ); ;
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
                R.SetCurrentError( @"Expected: string litteral [...] or ""..."" or '...' or {pattern}." );
                return null;
            }
            return new SqlTLocationFinder( firstOrLastOrSingleOrAll, plusOrMinusT, offset, outT, ofT, expectedMatchCount, textOrSimplePattern );
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

