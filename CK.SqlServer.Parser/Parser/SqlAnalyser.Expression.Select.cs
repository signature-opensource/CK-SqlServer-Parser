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
        bool MatchSelectSpecification( out SelectSpec e, SqlTokenIdentifier select )
        {
            e = null;
            SelectHeader header;
            if( !MatchSelectHeader( out header, select ) ) return false;
            SelectColumnList columns = IsCommaList( 0, IsSelectColumn, i => new SelectColumnList( i ) );
            if( columns == null ) return false;

            SpecificationPart c = ToSpecificationPart( R.Current );
            if( c == SpecificationPart.None )
            {
                e = new SelectSpec( header, columns );
            }
            else
            {
                SelectInto into = null;
                SelectFrom from = null;
                SelectWhere where = null;
                SelectGroupBy groupBy = null;
                if( c == SpecificationPart.Into )
                {
                    SqlTokenIdentifier partName = R.Read<SqlTokenIdentifier>();
                    ISqlIdentifier table = IsIdentifier( true );
                    if( table == null ) return false;
                    into = new SelectInto( partName, table );
                    c = ToSpecificationPart( R.Current );
                }
                if( c == SpecificationPart.From )
                {
                    SqlTokenIdentifier partName = R.Read<SqlTokenIdentifier>();
                    ISqlNode content = InternalIsExtendedExpression( true, SelectPartStopper );
                    if( content == null ) return false;
                    from = new SelectFrom( partName, content );
                    c = ToSpecificationPart( R.Current );
                }
                if( c == SpecificationPart.Where )
                {
                    SqlTokenIdentifier partName = R.Read<SqlTokenIdentifier>();
                    ISqlNode whereCond = IsOneExpression( true );
                    if( whereCond == null ) return false;
                    where = new SelectWhere( partName, whereCond );
                    c = ToSpecificationPart( R.Current );
                }
                if( c == SpecificationPart.Group )
                {
                    SqlTokenIdentifier partName = R.Read<SqlTokenIdentifier>();
                    SqlTokenIdentifier by;
                    ISqlNode content;
                    SqlTokenIdentifier having;
                    ISqlNode havingClause = null;
                    if( !R.IsToken( out by, SqlTokenType.By, true ) ) return false;
                    if( (content = InternalIsExtendedExpression( true, SelectPartStopper )) == null ) return false;
                    if( R.IsToken( out having, SqlTokenType.Having, false ) )
                    {
                        if( (havingClause = IsOneExpression( true )) == null ) return false;
                    }
                    groupBy = new SelectGroupBy( partName, by, content, having, havingClause );
                    c = ToSpecificationPart( R.Current );
                }
                e = new SelectSpec( header, columns, into, from, where, groupBy );
            }
            return true;
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
                    return new SelectColumn( alias, eA.AssignT, eA.Right );
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
            SqlTokenIdentifier id = t as SqlTokenIdentifier;
            if( id != null && !id.IsQuoted )
            {
                if( id.TokenType == SqlTokenType.Into ) c = SpecificationPart.Into;
                else if( id.TokenType == SqlTokenType.From ) c = SpecificationPart.From;
                else if( id.TokenType == SqlTokenType.Where ) c = SpecificationPart.Where;
                else if( id.TokenType == SqlTokenType.Group ) c = SpecificationPart.Group;
            }
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

        SelectOrderByOffset IsSelectOrderByOffset( bool expected )
        {
            SqlTokenIdentifier offsetToken;
            if( !R.IsToken( out offsetToken, SqlTokenType.Offset, expected ) ) return null;
            ISqlNode offsetExpr = IsOneExpression( true );
            if( offsetExpr == null ) return null;
            SqlTokenIdentifier rowsToken;
            if( !R.IsToken( out rowsToken, SqlTokenType.Rows, true ) ) return null;
            SqlTokenIdentifier fetchToken;
            if( R.IsToken( out fetchToken, SqlTokenType.Fetch, false ) )
            {
                SqlTokenIdentifier firstOrNextToken;
                if( !R.IsToken( out firstOrNextToken, SqlTokenType.First, false ) 
                    && !R.IsToken( out firstOrNextToken, SqlTokenType.Next, true ) ) return null;
                ISqlNode fetchExpr = IsOneExpression( true );
                if( fetchExpr == null ) return null;
                SqlTokenIdentifier fetchRowsToken;
                if( !R.IsToken( out fetchRowsToken, SqlTokenType.Rows, true ) ) return null;
                SqlTokenIdentifier onlyToken;
                if( !R.IsToken( out onlyToken, SqlTokenType.Only, true ) ) return null;
                return new SelectOrderByOffset( offsetToken, offsetExpr, rowsToken, fetchToken, firstOrNextToken, fetchExpr, fetchRowsToken, onlyToken );
            }
            return new SelectOrderByOffset( offsetToken, offsetExpr, rowsToken );
        }
    }
}

