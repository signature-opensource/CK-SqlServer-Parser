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
        bool MatchSelectSpecification( out SelectSpecification e, SqlTokenIdentifier select )
        {
            e = null;
            SelectHeader header;
            if( !MatchSelectHeader( out header, select ) ) return false;
            SelectColumnList columns = IsSelectColumnList( 0 );
            if( columns == null ) return false;

            SpecificationPart c = IsSpecificationPart( R.Current );
            if( c == SpecificationPart.None )
            {
                e = new SelectSpecification( header, columns );
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
                    c = IsSpecificationPart( R.Current );
                }
                if( c == SpecificationPart.From )
                {
                    SqlTokenIdentifier partName = R.Read<SqlTokenIdentifier>();
                    ISqlNode content = InternalIsExtendedExpression( true, SelectPartStopper );
                    if( content == null ) return false;
                    from = new SelectFrom( partName, content );
                    c = IsSpecificationPart( R.Current );
                }
                if( c == SpecificationPart.Where )
                {
                    SqlTokenIdentifier partName = R.Read<SqlTokenIdentifier>();
                    ISqlNode whereCond = IsOneExpression( true );
                    if( whereCond == null ) return false;
                    where = new SelectWhere( partName, whereCond );
                    c = IsSpecificationPart( R.Current );
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
                    c = IsSpecificationPart( R.Current );
                }
                e = new SelectSpecification( header, columns, into, from, where, groupBy );
            }
            return true;
        }

        SelectColumnList IsSelectColumnList( int minCount )
        {
            List<ISqlNode> items = new List<ISqlNode>();
            if( !R.CollectCommaList( items, IsSelectColumn, minCount ) ) return null;
            return new SelectColumnList( items );
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
                    return new SelectColumn( eA.Identifier, eA.AssignT, eA.Right );
                }
                SqlTokenIdentifier asToken;
                SqlTokenIdentifier colName = null;
                if( R.IsToken( out asToken, SqlTokenType.As, false ) )
                {
                    if( !R.IsToken( out colName, true ) ) return null;
                    return new SelectColumn( e, asToken, colName );
                }
                if( IsPossibleColumnDefinition( R.Current ) && R.IsToken( out colName, false ) )
                {
                    return new SelectColumn( e, colName );
                }
                return new SelectColumn( e );
            }
        }

        private bool IsOverClause( out SqlOverClause over )
        {
            over = null;
            SqlTokenIdentifier overToken;
            if( !R.IsToken( out overToken, SqlTokenType.Over, false ) ) return false;
            using( R.SetAssignmentContext( false ) )
            {
                SqlTokenOpenPar openPar;
                if( !R.IsToken( out openPar, true ) ) return false;
                SqlNodeList overContent;
                SqlTokenClosePar closePar;
                if( !IsSqlNodeList( out overContent, out closePar, null, minCount: 1 ) ) return false;
                over = new SqlOverClause( overToken, openPar, overContent, closePar );
                return true;
            }
        }

        bool SelectPartStopper( SqlToken t )
        {
            return t.TokenType == SqlTokenType.EndOfInput
                    || SqlToken.IsCloseParenthesisOrTerminatorOrPossibleStartStatement( t )
                    || t.TokenType.IsSelectOperator()
                    || IsSpecificationPart( t ) != SpecificationPart.None
                    || t.TokenType == SqlTokenType.Having
                    || t.TokenType == SqlTokenType.Option;
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

        SpecificationPart IsSpecificationPart( SqlToken t )
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
                    if( R.IsToken( out with, SqlTokenType.With, false ) ) R.IsUnquotedIdentifier( out ties, "ties", true );
                }
            }
            e = new SelectHeader( select, allOrDistinct, top, topExpression, percent, with, ties );
            return true;
        }

        SqlOrderByList IsOrderByList()
        {
            List<ISqlNode> items = new List<ISqlNode>();
            if( !R.CollectCommaList<SqlOrderByItem>( items, IsOrderByItem, 1 ) ) return null;
            return new SqlOrderByList( items );
        }

        SqlOrderByItem IsOrderByItem( bool expected )
        {
            ISqlNode definition = IsOneExpression( true );
            if( definition == null ) return null;
            SqlTokenIdentifier ascOrDesc;
            if( !R.IsToken( out ascOrDesc, SqlTokenType.Asc, false ) ) R.IsToken( out ascOrDesc, SqlTokenType.Desc, false );
            return new SqlOrderByItem( definition, ascOrDesc );
        }

        bool IsSelectOrderByOffset( out SelectOrderByOffset e )
        {
            e = null;
            SqlTokenIdentifier offsetToken;
            ISqlNode offsetExpr;
            SqlTokenIdentifier rowsToken;
            if( !R.IsToken( out offsetToken, SqlTokenType.Offset, false ) ) return false;
            if( !IsExpression( out offsetExpr, 0, true ) ) return false;
            if( !R.IsToken( out rowsToken, SqlTokenType.Rows, true ) ) return false;
            SqlTokenIdentifier fetchToken;
            if( R.IsToken( out fetchToken, SqlTokenType.Fetch, false ) )
            {
                SqlTokenIdentifier firstOrNextToken;
                if( !R.IsToken( out firstOrNextToken, SqlTokenType.First, false ) && !R.IsToken( out firstOrNextToken, SqlTokenType.Next, true ) ) return false;
                ISqlNode fetchExpr;
                if( !IsExpression( out fetchExpr, 0, true ) ) return false;
                SqlTokenIdentifier fetchRowsToken;
                if( !R.IsToken( out fetchRowsToken, SqlTokenType.Rows, true ) ) return false;
                SqlTokenIdentifier onlyToken;
                if( !R.IsToken( out onlyToken, SqlTokenType.Only, true ) ) return false;
                e = new SelectOrderByOffset( offsetToken, offsetExpr, rowsToken, fetchToken, firstOrNextToken, fetchExpr, fetchRowsToken, onlyToken );
            }
            else e = new SelectOrderByOffset( offsetToken, offsetExpr, rowsToken );
            return true;
        }
    }
}

