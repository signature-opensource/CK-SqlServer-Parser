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
            SelectColumnList columns;
            if( !MatchSelectHeader( out header, select ) ) return false;
            if( !IsSelectColumnList( out columns, false ) ) return false;

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
                    ISqlIdentifier table;
                    if( !IsIdentifier( out table, true ) ) return true;
                    into = new SelectInto( partName, table );
                    c = IsSpecificationPart( R.Current );
                }
                if( c == SpecificationPart.From )
                {
                    SqlTokenIdentifier partName = R.Read<SqlTokenIdentifier>();
                    ISqlNode content;
                    if( !IsExpressionOrNodeList( out content, SelectPartStopper, false, true ) ) return false;
                    from = new SelectFrom( partName, content );
                    c = IsSpecificationPart( R.Current );
                }
                if( c == SpecificationPart.Where )
                {
                    SqlTokenIdentifier partName = R.Read<SqlTokenIdentifier>();
                    ISqlNode whereCond;
                    if( !IsOneExpression( out whereCond, true ) ) return false;
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
                    if( !IsExpressionOrNodeList( out content, SelectPartStopper, false, true ) ) return false;
                    if( R.IsToken( out having, SqlTokenType.Having, false ) )
                    {
                        if( !IsOneExpression( out havingClause, true ) ) return false;
                    }
                    groupBy = new SelectGroupBy( partName, by, content, having, havingClause );
                    c = IsSpecificationPart( R.Current );
                }
                e = new SelectSpecification( header, columns, into, from, where, groupBy );
            }
            return true;
        }

        bool IsSelectColumnList( out SelectColumnList e, bool expectAtLeastOne )
        {
            e = null;
            List<ISqlNode> items;
            if( !IsCommaListNonEnclosed<SelectColumn>( out items, MatchColumn, expectAtLeastOne ) ) return false;
            e = new SelectColumnList( items );
            return true;
        }

        bool MatchColumn( out SelectColumn column, bool expected )
        {
            column = null;
            if( !IsPossibleColumnDefinition( R.Current ) )
            {
                if( expected ) R.SetCurrentError( "Expected column definition." );
                return false;
            }
            using( R.SetAssignmentContext( true ) )
            {
                ISqlNode e;
                if( !IsOneExpression( out e, true ) ) return false;
                SqlAssign eA = e as SqlAssign;
                if( eA != null )
                {
                    column = new SelectColumn( eA.Identifier, eA.AssignT, eA.Right );
                }
                else
                {
                    SqlTokenIdentifier asToken;
                    SqlTokenIdentifier colName = null;
                    if( R.IsToken( out asToken, SqlTokenType.As, false ) )
                    {
                        if( !R.IsToken( out colName, true ) ) return false;
                        column = new SelectColumn( e, asToken, colName );
                    }
                    else
                    {
                        if( IsPossibleColumnDefinition( R.Current ) && R.IsToken( out colName, false ) )
                        {
                            column = new SelectColumn( e, colName );
                        }
                        else
                        {
                            column = new SelectColumn( e );
                        }
                    }
                }
            }
            return true;
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
                if( !IsSqlNodeList( out overContent, out closePar, null, true ) ) return false;
                over = new SqlOverClause( overToken, openPar, overContent, closePar );
                return true;
            }
        }

        bool SelectPartStopper( SqlToken t )
        {
            return t.TokenType == SqlTokenType.EndOfInput
                    || t.IsCloseParenthesisOrTerminatorOrPossibleStartStatement()
                    || t.TokenType.IsSelectOperator()
                    || IsSpecificationPart( t ) != SpecificationPart.None
                    || t.IsUnquotedIdentifier( "having", "option" );
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
                if( id.NameEquals( "into" ) ) c = SpecificationPart.Into;
                else if( id.NameEquals( "from" ) ) c = SpecificationPart.From;
                else if( id.NameEquals( "where" ) ) c = SpecificationPart.Where;
                else if( id.NameEquals( "group" ) ) c = SpecificationPart.Group;
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
                if( !IsOneExpression( out topExpression, true ) ) return false;
                if( R.IsToken( out percent, SqlTokenType.Percent, false ) )
                {
                    if( R.IsToken( out with, SqlTokenType.With, false ) ) R.IsUnquotedIdentifier( out ties, "ties", true );
                }
            }
            e = new SelectHeader( select, allOrDistinct, top, topExpression, percent, with, ties );
            return true;
        }

        bool IsSelectOrderByColumnList( out SelectOrderByColumnList e )
        {
            e = null;
            List<ISqlNode> items;
            if( !IsCommaListNonEnclosed<SelectOrderByColumn>( out items, MatchOrderByColumn, true ) ) return false;
            e = new SelectOrderByColumnList( items );
            return true;
        }

        bool MatchOrderByColumn( out SelectOrderByColumn column, bool expected )
        {
            column = null;
            ISqlNode definition;
            if( !IsOneExpression( out definition, true ) ) return false;
            SqlTokenIdentifier ascOrDesc;
            if( !R.IsToken( out ascOrDesc, SqlTokenType.Asc, false ) ) R.IsToken( out ascOrDesc, SqlTokenType.Desc, false );
            column = new SelectOrderByColumn( definition, ascOrDesc );
            return true;
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

