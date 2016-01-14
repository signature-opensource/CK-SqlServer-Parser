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
        SelectSpec MatchSelectSpecification( SqlTokenIdentifier select )
        {
            SelectHeader header;
            if( !MatchSelectHeader( out header, select ) ) return null;
            SelectColumnList columns = IsCommaList( 0, IsSelectColumn, i => new SelectColumnList( i ) );
            if( columns == null ) return null;

            SpecificationPart c = ToSpecificationPart( R.Current );
            if( c == SpecificationPart.None )
            {
                return new SelectSpec( header, columns );
            }
            else
            {
                SelectInto into = null;
                SelectFrom from = null;
                SqlTokenIdentifier whereT = null;
                ISqlNode whereExpression = null;
                SelectGroupBy groupBy = null;
                if( c == SpecificationPart.Into )
                {
                    SqlTokenIdentifier partName = R.Read<SqlTokenIdentifier>();
                    ISqlIdentifier table = IsIdentifier( true );
                    if( table == null ) return null;
                    into = new SelectInto( partName, table );
                    c = ToSpecificationPart( R.Current );
                }
                if( c == SpecificationPart.From )
                {
                    SqlTokenIdentifier partName = R.Read<SqlTokenIdentifier>();
                    ISqlNode content = InternalIsExtendedExpression( true, SelectPartStopper );
                    if( content == null ) return null;
                    from = new SelectFrom( partName, content );
                    c = ToSpecificationPart( R.Current );
                }
                if( c == SpecificationPart.Where )
                {
                    whereT = R.Read<SqlTokenIdentifier>();
                    whereExpression = IsOneExpression( true );
                    if( whereExpression == null ) return null;
                    c = ToSpecificationPart( R.Current );
                }
                if( c == SpecificationPart.Group )
                {
                    SqlTokenIdentifier partName = R.Read<SqlTokenIdentifier>();
                    SqlTokenIdentifier by;
                    ISqlNode content;
                    SqlTokenIdentifier having;
                    ISqlNode havingClause = null;
                    if( !R.IsToken( out by, SqlTokenType.By, true ) ) return null;
                    if( (content = InternalIsExtendedExpression( true, SelectPartStopper )) == null ) return null;
                    if( R.IsToken( out having, SqlTokenType.Having, false ) )
                    {
                        if( (havingClause = IsOneExpression( true )) == null ) return null;
                    }
                    groupBy = new SelectGroupBy( partName, by, content, having, havingClause );
                    c = ToSpecificationPart( R.Current );
                }
                return new SelectSpec( header, columns, into, from, whereT, whereExpression, groupBy );
            }
        }

        SelectColumn IsSelectColumn( bool expected )
        {
            if( !IsPossibleColumnDefinition( R.Current ) )
            {
                if( expected ) R.SetCurrentError( "Expected column definition." );
                return null;
            }
            using( R.SetAssignmentContext( true ) )
            {
                ISqlNode e = IsOneExpression( true );
                if( e == null ) return null;
                SqlAssign eA = e as SqlAssign;
                if( eA != null )
                {
                    SqlToken alias = eA.Left as SqlToken;
                    if( alias == null || !alias.TokenType.IsValidColumnAliasNameOrVariable() )
                    {
                        R.SetCurrentError( "Invalid Column alias. Expected string, a unicode string or an identifier that is not reserved nor special." );
                        return null;
                    }
                    return new SelectColumn( alias, eA.Operator, eA.Right );
                }
                SqlTokenIdentifier asToken;
                SqlToken colName = null;
                if( R.IsToken( out asToken, SqlTokenType.As, false ) )
                {
                    if( !R.IsToken( out colName, true ) ) return null;
                }
                else if( !SelectPartStopper( R.Current ) && R.Current.TokenType.IsValidColumnAliasName() )
                {
                    colName = R.Read<SqlToken>();
                }
                if( colName != null )
                {
                    if( !colName.TokenType.IsValidColumnAliasName() )
                    {
                        R.SetCurrentError( "Invalid Column alias. Exepected string, a unicode string or an identifier that is not reserved nor special nor is a variable name." );
                        return null;
                    }
                    return asToken != null ? new SelectColumn( e, asToken, colName ) : new SelectColumn( e, colName );
                }
                return new SelectColumn( e );
            }
        }

        SqlOverClause IsOverClause( bool expected )
        {
            SqlTokenIdentifier overToken;
            if( !R.IsToken( out overToken, SqlTokenType.Over, expected ) ) return null;
            using( R.SetAssignmentContext( false ) )
            {
                SqlTokenOpenPar openPar;
                if( !R.IsToken( out openPar, true ) ) return null;
                SqlTokenClosePar closePar;
                SqlNodeList overContent = IsSqlNodeList( out closePar, null, minCount: 1 );
                return overContent != null ? new SqlOverClause( overToken, openPar, overContent, closePar ) : null;
            }
        }

        bool SelectPartStopper( SqlToken t )
        {
            return t.TokenType == SqlTokenType.EndOfInput
                    || SqlToken.IsCloseParenthesisOrTerminatorOrPossibleStartStatement( t )
                    || t.TokenType.IsSelectOperator()
                    || ToSpecificationPart( t ) != SpecificationPart.None
                    || t.TokenType == SqlTokenType.Having;
        }

        bool IsPossibleColumnDefinition( SqlToken t )
        {
            return !SelectPartStopper( t );
        }

        enum SpecificationPart
        {
            None = 0,
            Into = 1,
            From = 2,
            Where = 3,
            Group = 4
        }

        SpecificationPart ToSpecificationPart( SqlToken t )
        {
            SpecificationPart c = SpecificationPart.None;
            if( t.TokenType == SqlTokenType.Into ) c = SpecificationPart.Into;
            else if( t.TokenType == SqlTokenType.From ) c = SpecificationPart.From;
            else if( t.TokenType == SqlTokenType.Where ) c = SpecificationPart.Where;
            else if( t.TokenType == SqlTokenType.Group ) c = SpecificationPart.Group;
            return c;
        }

        bool MatchSelectHeader( out SelectHeader e, SqlTokenIdentifier select )
        {
            e = null;
            SqlTokenIdentifier allOrDistinct = null;
            SqlTokenIdentifier top = null;
            ISqlNode topExpression = null;
            SqlTokenIdentifier percent = null;
            SqlTokenIdentifier with = null;
            SqlTokenIdentifier ties = null;

            if( !R.IsToken( out allOrDistinct, SqlTokenType.All, false ) ) R.IsToken( out allOrDistinct, SqlTokenType.Distinct, false );
            if( R.IsToken( out top, SqlTokenType.Top, false ) )
            {
                if( (topExpression = IsOneExpression( true )) == null ) return false;
                if( R.IsToken( out percent, SqlTokenType.Percent, false ) )
                {
                    if( R.IsToken( out with, SqlTokenType.With, false ) ) R.IsToken( out ties, SqlTokenType.Ties, true );
                }
            }
            e = new SelectHeader( select, allOrDistinct, top, topExpression, percent, with, ties );
            return true;
        }

        SqlOrderByItem IsOrderByItem( bool expected )
        {
            ISqlNode definition = IsOneExpression( true );
            if( definition == null ) return null;
            SqlTokenIdentifier ascOrDesc;
            if( !R.IsToken( out ascOrDesc, SqlTokenType.Asc, false ) ) R.IsToken( out ascOrDesc, SqlTokenType.Desc, false );
            return new SqlOrderByItem( definition, ascOrDesc );
        }

        SelectOrderBy IsSelectOrderBy( bool expected )
        {
            SqlTokenIdentifier orderT, byT = null;
            if( !R.IsToken( out orderT, SqlTokenType.Order, expected ) 
                || !R.IsToken( out byT, SqlTokenType.By, true ) ) return null;
            SqlOrderByList orderByList = IsCommaList( 1, IsOrderByItem, i => new SqlOrderByList( i ) );
            if( orderByList == null ) return null;

            SqlTokenIdentifier offsetToken;
            ISqlNode offsetExpr = null;
            SqlTokenIdentifier rowsToken = null;
            SqlTokenIdentifier fetchToken = null;
            if( R.IsToken( out offsetToken, SqlTokenType.Offset, false ) )
            {
                offsetExpr = IsOneExpression( true );
                if( offsetExpr == null ) return null;
                if( !R.IsToken( out rowsToken, SqlTokenType.Rows, true ) ) return null;
                if( R.IsToken( out fetchToken, SqlTokenType.Fetch, false ) )
                {
                    SqlTokenIdentifier firstOrNextToken;
                    if( !R.IsToken( out firstOrNextToken, SqlTokenType.First, false )
                        && !R.IsToken( out firstOrNextToken, SqlTokenType.Next, true ) )
                        return null;
                    ISqlNode fetchExpr = IsOneExpression( true );
                    if( fetchExpr == null ) return null;
                    SqlTokenIdentifier fetchRowsToken;
                    if( !R.IsToken( out fetchRowsToken, SqlTokenType.Rows, true ) ) return null;
                    SqlTokenIdentifier onlyToken;
                    if( !R.IsToken( out onlyToken, SqlTokenType.Only, true ) ) return null;
                    return new SelectOrderBy( orderT, byT, orderByList, offsetToken, offsetExpr, rowsToken, fetchToken, firstOrNextToken, fetchExpr, fetchRowsToken, onlyToken );
                }
            }
            return new SelectOrderBy( orderT, byT, orderByList, offsetToken, offsetExpr, rowsToken );
        }

        SelectFor IsSelectFor( bool expected )
        {
            if( R.Current.TokenType != SqlTokenType.For || !R.RawLookup.TokenType.IsSelectForTargetType() )
            {
                if( expected ) R.SetCurrentError( "Expected Select for clase." );
                return null;
            }
            SqlTokenIdentifier forT = R.Read<SqlTokenIdentifier>();
            SqlTokenIdentifier targetType = R.Read<SqlTokenIdentifier>();
            ISqlNode forExpression = IsSqlNodeList<SqlToken>( SelectPartStopper, IsOneExpression, 1 );
            if( forExpression == null ) return null;
            return new SelectFor( forT, targetType, forExpression );
        }


    }
}

