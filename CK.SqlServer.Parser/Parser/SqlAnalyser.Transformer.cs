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
            }
            else if( R.IsToken( out initT, SqlTokenType.Insert, false ) )
            {
                SqlTokenTerminal opener;
                if( !R.IsToken( out opener, SqlTokenType.OpenCurly, true ) ) return null;

                SqlTokenTerminal closer;
                ISqlNode content = IsSqlNodeList( out closer, t => t.TokenType == SqlTokenType.CloseCurly );
                if( content == null ) content = new SqlEmptyStatement( null );
                SqlTrivia.ToMiddle( ref opener, ref content, ref closer );

                SqlTokenIdentifier beforeOrAfterT;
                if( !R.IsToken( out beforeOrAfterT, SqlTokenType.Before, false ) && !R.IsToken( out beforeOrAfterT, SqlTokenType.After, true ) ) return null;

                SqlTLocationSelector loc = IsSqlTLocation( true );
                if( loc == null ) return null;

                return new SqlTInsert( initT, opener, content, closer, beforeOrAfterT, loc, GetOptionalTerminator());
            }
            if( expected ) R.SetCurrentError( "Expected transform statement." );
            return null;
        }

        SqlTLocationSelector IsSqlTLocation( bool expected )
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

            ISqlHasStringValue text = R.Current as ISqlHasStringValue;
            ISqlNode textOrRange = null;
            if( text != null )
            {
                if( !text.Value.StartsWith( "--" )
                    && (!text.Value.StartsWith( "/*" ) || !text.Value.EndsWith( "*/" )) )
                {
                    R.SetCurrentError( @"Litteral string must start with -- or starts and ends with /* and */." );
                    return null;
                }
                R.MoveNext();
                textOrRange = text;
            }
            else textOrRange = IsNodeRange( false );
            if( textOrRange == null )
            {
                R.SetCurrentError( @"Expected: string litteral [...] or ""..."" or '...' or {node range}." );
                return null;
            }
            return new SqlTLocationSelector( firstOrLastOrSingleOrAll, plusOrMinusT, offset, outT, ofT, expectedMatchCount, textOrRange );
        }

        SqlTNodeRange IsNodeRange( bool expected )
        {
            SqlTokenTerminal opener;
            if( !R.IsToken( out opener, SqlTokenType.OpenCurly, expected ) ) return null;
            SqlTokenTerminal closer;
            List<ISqlNode> items = new List<ISqlNode>();
            if( !R.CollectUntil( items, out closer, null, t => t.TokenType == SqlTokenType.CloseCurly ) ) return null;
            return new SqlTNodeRange( opener, items, closer );
        }

    }


}

