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

            SqlTransformStatementList s = IsList( false, IsTransformStatement, statements => new SqlTransformStatementList( statements ) );
            if( s == null ) return null;

            SqlTokenIdentifier endT;
            if( !R.IsToken( out endT, SqlTokenType.End, true ) ) return null;

            return new SqlTransformer( alterOrCreate, type, nameOrOnOrAs, onT, targetName, asT, begintT, s, endT, GetOptionalTerminator() );
        }

        ISqlTransformStatement IsTransformStatement( bool expected )
        {
            SqlTokenIdentifier initT;
            if( R.IsToken( out initT, SqlTokenType.Add, false ))
            {
                SqlTokenIdentifier whatT;
                if( R.IsToken( out whatT, SqlTokenType.Parameter, false ) )
                {
                    SqlParameterList parameters = IsParameterList( Parenthesis.Rejected, 1 );
                    if( parameters == null ) return null;
                    SqlTokenIdentifier whereT;
                    if( R.IsToken( out whereT, SqlTokenType.After, false ) || R.IsToken( out whereT, SqlTokenType.Before, false ) )
                    {
                        SqlTokenIdentifier paramName;
                        if( !R.IsToken( out paramName, t => t.IsVariable, true ) ) return null;
                        return new SqlTAddParameter( initT, whatT, parameters, whereT, paramName, GetOptionalTerminator() ); 
                    }
                }
            }
            if( expected ) R.SetCurrentError( "Expected transform statement." );
            return null;
        }
    }


}

