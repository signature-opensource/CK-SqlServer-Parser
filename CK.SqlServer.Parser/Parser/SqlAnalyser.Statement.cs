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
        /// <summary>
        /// A named statement.
        /// </summary>
        /// <param name="expected"></param>
        /// <returns></returns>
        public ISqlStatement IsExtendedStatement( bool expected )
        {
            ISqlStatement e = IsNamedStatement( false );
            if( e != null || R.IsErrorOrEndOfInput ) return e;
            ISqlNode n = IsAnyExpressionForStatement( expected );
            if( n == null ) return null;
            return new SqlUnnamedStatement( n, GetOptionalTerminator() );
        }

        public ISqlNamedStatement IsNamedStatement( bool expected )
        {
            if( R.Current.TokenType == SqlTokenType.SemiColon )
            {
                return new SqlEmptyStatement( R.Read<SqlTokenTerminal>() );
            }
            SqlTokenIdentifier id = R.Current as SqlTokenIdentifier;
            // A statement starts with an identifier that must be non quoted and not a variable.
            if( id == null || id.IsQuoted || id.IsVariable )
            {
                if( expected ) R.SetCurrentError( "Statement expected." );
                return null;
            }
            // End Conversation ... is a statement.
            // Otherwise, we handle it: this is the end of a block above.
            if( id.TokenType == SqlTokenType.End && R.RawLookup.TokenType != SqlTokenType.Conversation )
            {
                if( expected ) R.SetCurrentError( "Statement expected." );
                return null;
            }
            // Begin Dialog ... or Begin Conversation are statement.
            // Otherwise we handle it as a:
            // - Begin transaction
            // - Begin Try ... End Catch block
            // - or a Begin ... End block.
            if( id.TokenType == SqlTokenType.Begin && R.RawLookup.TokenType != SqlTokenType.Conversation && R.RawLookup.TokenType != SqlTokenType.Dialog )
            {
                R.MoveNext();
                SqlTokenIdentifier tranOrTry;
                // "tran" and "transaction" both map to SqlTokenType.Transaction.
                if( R.IsToken( out tranOrTry, SqlTokenType.Transaction, false ) )
                {
                    SqlTokenIdentifier tranNameOrVariable;
                    SqlTokenIdentifier withToken = null;
                    SqlTokenIdentifier markToken = null;
                    SqlTokenLiteralString description = null;
                    if( R.IsToken( out tranNameOrVariable, false ) )
                    {
                        if( R.IsToken( out withToken, SqlTokenType.With, false ) )
                        {
                            if( !R.IsToken( out markToken, SqlTokenType.Mark, true ) ) return null;
                            R.IsToken( out description, false );
                        }
                    }
                    return new SqlBeginTransaction( id, tranOrTry, tranNameOrVariable, withToken, markToken, description, GetOptionalTerminator() );
                }
                R.IsToken( out tranOrTry, SqlTokenType.Try, false );
                SqlStatementList body;
                if( !IsStatementList( out body, true ) ) return null;
                SqlTokenIdentifier end;
                if( !R.IsToken( out end, SqlTokenType.End, true ) ) return null;
                if( tranOrTry == null )
                {
                    return new SqlBeginEndBlock( id, body, end );
                }
                // Begin Try ... End Try Begin Catch ... End Catch.
                SqlTokenIdentifier endTry;
                if( !R.IsToken( out endTry, SqlTokenType.Try, true ) ) return null;
                SqlTokenIdentifier begCatch, begCatchToken;
                if( !R.IsToken( out begCatch, SqlTokenType.Begin, true ) || !R.IsToken( out begCatchToken, SqlTokenType.Catch, true ) ) return null;
                SqlStatementList bodyCatch;
                if( !IsStatementList( out bodyCatch, true ) ) return null;
                SqlTokenIdentifier endCatch, endCatchToken;
                if( !R.IsToken( out endCatch, SqlTokenType.End, true ) || !R.IsToken( out endCatchToken, SqlTokenType.Catch, true ) ) return null;
                return new SqlTryCatch( id, tranOrTry,
                                        body,
                                        end, endTry, begCatch, begCatchToken,
                                        bodyCatch,
                                        endCatch, endCatchToken,
                                        GetOptionalTerminator() );
            }
            if( id.TokenType == SqlTokenType.Create || id.TokenType == SqlTokenType.Alter )
            {
                R.MoveNext();
                SqlTokenIdentifier type;
                if( !R.IsToken( out type, true ) ) return null;
                if( type.TokenType == SqlTokenType.Procedure )
                {
                    return MatchStoredProcedure( id, type );
                }
                if( type.TokenType == SqlTokenType.View )
                {
                    return MatchView( id, type );
                }
                if( type.TokenType == SqlTokenType.Function )
                {
                    return MatchFunction( id, type );
                }
            }
            if( id.TokenType == SqlTokenType.If )
            {
                R.MoveNext();
                ISqlNode expr = IsOneExpression( true );
                if( expr == null ) return null;
                ISqlStatement thenSt = IsExtendedStatement( true );
                if( thenSt == null ) return null;
                SqlTokenIdentifier elseToken;
                ISqlStatement elseSt = null;
                if( R.IsToken( out elseToken, SqlTokenType.Else, false ) )
                {
                    elseSt = IsExtendedStatement( true );
                    if( elseSt == null ) return null;
                }
                return new SqlIf( id, expr, thenSt, elseToken, elseSt, GetOptionalTerminator() );
            }
            if( id.TokenType == SqlTokenType.Return )
            {
                R.MoveNext();
                return new SqlReturn( id, IsOneExpression( false ), GetOptionalTerminator() );
            }
            if( id.TokenType == SqlTokenType.Goto )
            {
                R.MoveNext();
                SqlTokenIdentifier target;
                if( !R.IsToken( out target, true ) ) return null;
                return new SqlGoto( id, target, GetOptionalTerminator() );
            }
            if( id.TokenType == SqlTokenType.Set )
            {
                R.MoveNext();
                using( R.SetAssignmentContext( true ) )
                {
                    if( R.Current.IsToken( SqlTokenType.IdentifierVariable ) )
                    {
                        SqlTokenIdentifier left = R.Read<SqlTokenIdentifier>();
                        SqlTokenTerminal assignT;
                        ISqlNode right;
                        if( !R.IsToken( out assignT, t => (t.TokenType & SqlTokenType.IsAssignOperator) != 0, true ) ) return null;

                        SqlTokenIdentifier cursorT;
                        if( R.IsToken( out cursorT, SqlTokenType.Cursor, false ) )
                        {
                            right = MatchCursorDefinition( null, null, cursorT );
                        }
                        else right = IsAnyExpression( true );
                        if( right == null ) return null;
                        return new SqlSetVariable( id, left, assignT, right, GetOptionalTerminator() );
                    }
                    var options = IsAnyExpressionForStatement( true );
                    if( options == null ) return null;
                    return new SqlSetOption( id, options, GetOptionalTerminator() );
                }
            }
            if( id.TokenType == SqlTokenType.Declare )
            {
                R.MoveNext();
                // Syntax: declare cursorName cursor ...
                //   - cursorName can not be a @Variable.
                //   - cursorName can be a quoted identifier.
                //   - no 'as' between cursorName and 'cursor'.
                if( R.Current.TokenType != SqlTokenType.IdentifierVariable )
                {
                    SqlTokenIdentifier name;
                    SqlTokenIdentifier cursorToken;
                    if( !R.IsToken( out name, true ) ) return null;
                    // Handles SQL92 syntax here:
                    SqlTokenIdentifier insensitiveOrScrollT;
                    SqlTokenIdentifier scrollOrInsensitiveT;
                    if( R.IsToken( out insensitiveOrScrollT, SqlTokenType.Insensitive, false ) )
                    {
                        R.IsToken( out scrollOrInsensitiveT, SqlTokenType.Scroll, false );
                    }
                    else if( R.IsToken( out scrollOrInsensitiveT, SqlTokenType.Scroll, false ) )
                    {
                        R.IsToken( out insensitiveOrScrollT, SqlTokenType.Insensitive, false );
                    }
                    if( !R.IsToken( out cursorToken, SqlTokenType.Cursor, true ) ) return null;
                    ISqlCursorDefinition cursorExpr = MatchCursorDefinition( insensitiveOrScrollT, scrollOrInsensitiveT, cursorToken );
                    if( cursorExpr == null ) return null;
                    return new SqlDeclareCursor( id, name, cursorExpr, GetOptionalTerminator() );
                }
                List<ISqlNode> items = new List<ISqlNode>();
                if( !R.CollectCommaList( items, IsVariableDeclaration, 1 ) ) return null;
                return new SqlDeclareVariable( id, new SqlVariableDeclarationList( items ), GetOptionalTerminator() );
            }
            if( id.IsStartStatement || id.TokenType == SqlTokenType.With )
            {
                R.MoveNext();
                var content = IsAnyExpressionForStatement( false );
                if( content == null )
                {
                    if( R.IsError ) return null;
                    content = SqlNodeList.Empty;
                }
                return new SqlStatement( id, content, GetOptionalTerminator() );
            }
            // If it is not a reserved keyword, it can only be 
            // a label definition.
            SqlTokenTerminal colon;
            if( id.TrailingTrivias.Count > 0
                || (colon = R.RawLookup as SqlTokenTerminal) == null
                || colon.TokenType != SqlTokenType.Colon
                || colon.LeadingTrivias.Count > 0 )
            {
                if( expected ) R.SetCurrentError( "Statement expected." );
                return null;
            }
            R.MoveNext();
            R.MoveNext();
            return new SqlLabelDefinition( id, colon );
        }

        bool IsStatementList( out SqlStatementList l, bool atLeastOneStatement )
        {
            l = null;
            var statements = new List<ISqlStatement>();
            ISqlStatement st;
            while( (st = IsExtendedStatement( false )) != null )
            {
                statements.Add( st );
            }
            if( statements.Count == 0 )
            {
                if( atLeastOneStatement && !R.IsError ) R.SetCurrentError( "At least one statement expected." );
                return false;
            }
            l = new SqlStatementList( statements );
            return !R.IsError;
        }

        SqlView MatchView( SqlTokenIdentifier alterOrCreate, SqlTokenIdentifier type )
        {
            ISqlIdentifier name = IsIdentifier( true );
            if( name == null ) return null;

            SqlEnclosedIdentiferCommaList columns = IsSqlEnclosedIdentiferCommaList( false );

            SqlNodeList options;
            SqlTokenIdentifier asToken;
            if( !IsSqlNodeList( out options, out asToken, t => t.TokenType == SqlTokenType.As ) ) return null;

            ISqlNode body = IsAnyExpressionForStatement( true );
            if( body == null ) return null;
            return new SqlView( alterOrCreate, type, name, columns, options, asToken, body, GetOptionalTerminator() );
        }

        SqlEnclosedIdentiferCommaList IsSqlEnclosedIdentiferCommaList( bool expected )
        {
            if( !expected && R.Current.TokenType != SqlTokenType.OpenPar ) return null;
            SqlTokenOpenPar openPar;
            SqlTokenClosePar closePar;
            List<ISqlNode> items = new List<ISqlNode>();
            if( !R.CollectCommaList<ISqlIdentifier>( items, out openPar, out closePar, IsIdentifier, 1, Parenthesis.Required ) ) return null;
            return new SqlEnclosedIdentiferCommaList( openPar, items, closePar );
        }

        ISqlNamedStatement MatchFunction( SqlTokenIdentifier alterOrCreate, SqlTokenIdentifier type )
        {
            /*
            CREATE FUNCTION [ schema_name. ] function_name 
                ( [ { @parameter_name [ AS ][ type_schema_name. ] parameter_data_type [ = default ] [ READONLY ] } 
                    [ ,...n ]
                  ]
                )
            RETURNS 
             */
            ISqlIdentifier name = IsIdentifier( true );
            if( name == null ) return null;

            SqlParameterList parameters = IsParameterList( Parenthesis.Required );
            if( parameters == null ) return null;

            SqlTokenIdentifier returns;
            if( !R.IsToken( out returns, SqlTokenType.Returns, true ) ) return null;

            SqlTokenIdentifier table;
            SqlTokenIdentifier tableVariableNameToken;
            if( R.IsToken( out table, SqlTokenType.Table, false ) )
            {
                // Inline Table-Valued Function Syntax
                // CREATE FUNCTION [ schema_name. ] function_name 
                //    ( [ { @parameter_name [ AS ] [ type_schema_name. ] parameter_data_type 
                //        [ = default ] [ READONLY ] } 
                //        [ ,...n ]
                //      ]
                //    )
                // RETURNS TABLE
                //    [ WITH <function_option> [ ,...n ] ]
                //    [ AS ]
                //    RETURN [ ( ] select_stmt [ ) ]
                // [ ; ]
                SqlNodeList options;
                SqlTokenIdentifier endOptionToken;
                SqlTokenIdentifier asToken;
                SqlTokenIdentifier returnToken;
                if( !IsFunctionOptionsAsAndBeginOrReturn( out options, out endOptionToken, out asToken, out returnToken, isBegin: false ) ) return null;
                ISqlStatement st = IsExtendedStatement( true );
                if( st == null ) return null;

                return new SqlFunctionInlineTable(
                                alterOrCreate,
                                type,
                                name,
                                parameters,
                                returns,
                                table,
                                options,
                                asToken,
                                returnToken,
                                st,
                                GetOptionalTerminator() );
            }
            else if( R.IsToken( out tableVariableNameToken, t => t.IsVariable, false ) )
            {
                // Multistatement Table-valued Function Syntax
                // CREATE FUNCTION [ schema_name. ] function_name 
                //   ( [ { @parameter_name [ AS ] [ type_schema_name. ] parameter_data_type 
                //         [ = default ] [READONLY] } 
                //       [ ,...n ]
                //     ]
                //   )
                // RETURNS @return_variable TABLE <table_type_definition>
                //    [ WITH <function_option> [ ,...n ] ]
                //    [ AS ]
                //    BEGIN 
                //        function_body 
                //        RETURN
                //    END
                // [ ; ]
                throw new NotSupportedException( "Multistatement Table-valued Function Syntax" );
            }
            else
            {
                // Scalar Function Syntax
                // CREATE FUNCTION [ schema_name. ] function_name 
                //   ( [ { @parameter_name [ AS ][ type_schema_name. ] parameter_data_type 
                //       [ = default ] [ READONLY ] } 
                //       [ ,...n ]
                //     ]
                //   )
                //   RETURNS return_data_type
                //       [ WITH <function_option> [ ,...n ] ]
                //       [ AS ]
                //       BEGIN 
                //           function_body 
                //           RETURN scalar_expression
                //       END
                //  [ ; ]
                ISqlUnifiedTypeDecl returnScalarType = IsTypeDecl( true );
                if( returnScalarType == null ) return null;
                // Scalar Function Syntax
                SqlNodeList options;
                SqlTokenIdentifier endOptionToken;
                SqlTokenIdentifier asToken;
                SqlTokenIdentifier begin;
                if( !IsFunctionOptionsAsAndBeginOrReturn( out options, out endOptionToken, out asToken, out begin ) ) return null;
                SqlStatementList bodyStatements;
                SqlTokenIdentifier end;
                SqlTokenTerminal term;
                if( !IsBodyStatementListSafe( out bodyStatements, ref begin, out end, out term ) ) return null;
                return new SqlFunctionScalar(
                                alterOrCreate,
                                type,
                                name,
                                parameters,
                                returns,
                                returnScalarType,
                                options,
                                asToken,
                                begin,
                                bodyStatements,
                                end,
                                term );
            }
        }

        bool IsFunctionOptionsAsAndBeginOrReturn( out SqlNodeList options, out SqlTokenIdentifier endOptionToken, out SqlTokenIdentifier asToken, out SqlTokenIdentifier beginOrReturn, bool isBegin = true )
        {
            asToken = beginOrReturn = null;
            if( !IsSqlNodeList( out options, out endOptionToken, t => t.TokenType == SqlTokenType.As || t.TokenType == SqlTokenType.Begin ) ) return false;
            asToken = null;
            beginOrReturn = null;
            if( endOptionToken.TokenType == SqlTokenType.As )
            {
                asToken = endOptionToken;
                if( !R.IsToken( out beginOrReturn, isBegin ? SqlTokenType.Begin : SqlTokenType.Return, true ) ) return false;
            }
            else
            {
                beginOrReturn = endOptionToken;
            }
            return true;
        }

        SqlStoredProcedure MatchStoredProcedure( SqlTokenIdentifier alterOrCreate, SqlTokenIdentifier type )
        {
            ISqlIdentifier name = IsIdentifier( true );
            if( name == null ) return null;

            SqlParameterList parameters = IsParameterList( Parenthesis.Optional );
            if( parameters == null ) return null;

            SqlNodeList options;
            SqlTokenIdentifier asToken;
            if( !IsSqlNodeList( out options, out asToken, t => t.TokenType == SqlTokenType.As, IsExecuteAs ) ) return null;

            SqlTokenIdentifier begin = null;
            SqlStatementList bodyStatements;
            SqlTokenIdentifier end;
            SqlTokenTerminal term;
            if( !IsBodyStatementListSafe( out bodyStatements, ref begin, out end, out term ) ) return null;
            return new SqlStoredProcedure( alterOrCreate, type, name, parameters, options, asToken, begin, bodyStatements, end, term );
        }

        bool IsBodyStatementListSafe( 
            out SqlStatementList bodyStatements, 
            ref SqlTokenIdentifier begin, 
            out SqlTokenIdentifier end, 
            out SqlTokenTerminal term )
        {
            end = null;
            term = null;
            if( begin == null ) R.IsToken( out begin, SqlTokenType.Begin, false );
            if( !IsStatementList( out bodyStatements, true ) ) return false;
            if( begin != null && !R.IsToken( out end, SqlTokenType.End, true ) ) return false;
            term = GetOptionalTerminator();

            //using( var collector = R.OpenCollector() )
            //{
            //    if( begin == null ) R.IsToken( out begin, SqlTokenType.Begin, false );

            //    // Attempts to read a statement list. If it fails, reads the whole stream as an unmodeled list of tokens.
            //    if( IsStatementList( out bodyStatements, true ) )
            //    {
            //        if( begin != null && !R.IsToken( out end, SqlTokenType.End, true ) ) return false;
            //        term = GetOptionalTerminator();
            //    }
            //    else
            //    {
            //        // Collects all tokens and generates a Statement list with one unmodeled list of tokens.
            //        // Saves the begin/end and semi colon terminator if possible.
            //        term = collector.ReadToEnd();
            //        if( begin != null )
            //        {
            //            if( collector.Count > 0 && collector[collector.Count - 1].TokenType == SqlTokenType.End )
            //            {
            //                end = (SqlTokenIdentifier)collector[collector.Count - 1];
            //            }
            //            else
            //            {
            //                return R.SetCurrentError( "Missing END." );
            //            }
            //        }
            //        var t = new SqlStatement( new SqlNodeList( begin != null ? collector.Skip( 1 ).Take( collector.Count - 2 ) : collector ) );
            //        bodyStatements = new SqlStatementList( new[] { t } );
            //    }
            //}
            return true;
        }

        SqlParameterList IsParameterList( Parenthesis parenthesis )
        {
            SqlTokenOpenPar openPar;
            SqlTokenClosePar closePar;
            List<ISqlNode> items = new List<ISqlNode>();
            if( !R.CollectCommaList( items, out openPar, out closePar, IsParameter, 0, parenthesis ) ) return null;
            return new SqlParameterList( openPar, items, closePar );
        }

        SqlParameter IsParameter( bool expected )
        {
            SqlTypedIdentifier declVar;
            SqlParameterDefaultValue defValue = null;
            using( R.SetAssignmentContext( true ) )
            {
                if( !IsTypedIdentifer( out declVar, t => t.IsVariable, expected ) ) return null;
                SqlTokenTerminal assign;
                if( R.IsToken( out assign, SqlTokenType.Assign, false ) )
                {
                    SqlTokenIdentifier variable;
                    if( R.IsToken( out variable, SqlTokenType.Null, false )
                        || R.IsToken( out variable, SqlTokenType.IdentifierVariable, false ) )
                    {
                        defValue = new SqlParameterDefaultValue( assign, variable );
                    }
                    else
                    {
                        SqlTokenTerminal minusSign;
                        R.IsToken( out minusSign, false );
                        SqlTokenBaseLiteral value;
                        if( !R.IsToken( out value, true ) ) return null;
                        defValue = new SqlParameterDefaultValue( assign, minusSign, value );
                    }
                }
            }
            SqlTokenIdentifier outputClause;
            R.IsToken( out outputClause, SqlTokenType.Output, false );

            SqlTokenIdentifier readonlyClause;
            R.IsToken( out readonlyClause, SqlTokenType.Readonly, false );

            return new SqlParameter( declVar, defValue, outputClause, readonlyClause );
        }

        SqlVariableDeclaration IsVariableDeclaration( bool expected )
        {
            SqlTypedIdentifier declVar;
            SqlTokenTerminal assignToken = null;
            ISqlNode initialValue = null;
            // Syntax: declare @name [as] type
            using( R.SetAssignmentContext( true ) )
            {
                if( !IsTypedIdentifer( out declVar, t => t.IsVariable, expected ) ) return null;
                if( R.IsToken( out assignToken, SqlTokenType.Assign, false ) )
                {
                    initialValue = IsOneExpression( true );
                    if( initialValue == null ) return null;
                }
            }
            return new SqlVariableDeclaration( declVar, assignToken, initialValue );
        }

        ISqlCursorDefinition MatchCursorDefinition( 
            SqlTokenIdentifier insensitiveOrScrollT, 
            SqlTokenIdentifier scrollOrInsensitiveT, 
            SqlTokenIdentifier cursorToken )
        {
            SqlNodeList options;
            SqlTokenIdentifier forToken;
            if( !IsSqlNodeList( out options, out forToken, t => t.TokenType == SqlTokenType.For ) ) return null;
            Debug.Assert( forToken.TokenType == SqlTokenType.For );
            ISqlNode eSelect = IsOneExpression( true );
            ISelectSpecification select = eSelect?.UnPar as ISelectSpecification;
            if( select == null )
            {
                R.SetCurrentError( "Select statement expected." );
                return null;
            }
            SqlTokenIdentifier forOptionsToken;
            SqlTokenIdentifier readTokenSql92 = null;
            SqlTokenIdentifier onlyTokenSql92 = null;
            SqlTokenIdentifier updateToken = null;
            SqlTokenIdentifier ofToken = null;
            SqlIdentifierCommaList updateColumns = null;
            if( R.IsToken( out forOptionsToken, SqlTokenType.For, false ) )
            {
                if( R.IsToken( out readTokenSql92, SqlTokenType.Read, false ) )
                {
                    if( !R.IsToken( out onlyTokenSql92, SqlTokenType.Only, true ) ) return null;
                }
                else
                {
                    if( !R.IsToken( out updateToken, SqlTokenType.Update, true ) ) return null;
                    if( R.IsToken( out ofToken, SqlTokenType.Of, false ) )
                    {
                        List<ISqlNode> columns = new List<ISqlNode>();
                        if( !R.CollectCommaList( columns, IsIdentifier, 1 ) ) return null;
                        updateColumns = new SqlIdentifierCommaList( columns );
                    }
                }
            }
            if( readTokenSql92 != null )
            {
                if( options != null )
                {
                    R.SetCurrentError( "Sql92: There can be no options in 'cursor [Options] for <select> for read only;'." );
                    return null;
                }
                return new SqlCursorDefinition92( insensitiveOrScrollT, scrollOrInsensitiveT, cursorToken, forToken, select, forOptionsToken, readTokenSql92, onlyTokenSql92, updateToken, ofToken, updateColumns );
            }
            else
            {
                if( insensitiveOrScrollT != null || scrollOrInsensitiveT != null )
                {
                    R.SetCurrentError( "INSENSITIVE or SCROLL cursor requires Sql92 syntax." );
                    return null;
                }
                return new SqlCursorDefinition( cursorToken, options, forToken, select, forOptionsToken, updateToken, ofToken, updateColumns );
            }
        }

        bool IsTypedIdentifer( out SqlTypedIdentifier declVar, Predicate<SqlTokenIdentifier> idFilter, bool expected = true )
        {
            declVar = null;
            SqlTokenIdentifier identifier;
            if( !R.IsToken( out identifier, idFilter, expected ) ) return false;

            SqlTokenIdentifier asToken;
            R.IsToken( out asToken, SqlTokenType.As, false );

            ISqlUnifiedTypeDecl typeDecl = IsTypeDecl( true );
            if( typeDecl == null ) return false;

            declVar = new SqlTypedIdentifier( identifier, asToken, typeDecl );
            return true;
        }

        /// <summary>
        /// Is a SqlExprTypeDecl: either a SqlDbType (int, sql_variant) or multiple identifiers that is a user defined type.
        /// </summary>
        /// <returns></returns>
        ISqlUnifiedTypeDecl IsTypeDecl( bool expected )
        {
            SqlTokenIdentifier id;
            if( R.IsToken( out id, t => t.IsDbType, false ) )
            {
                Debug.Assert( SqlKeyword.FromSqlTokenTypeToSqlDbType( id.TokenType ).HasValue, "TokenType has been mapped to a SqlDbType." );

                #region Type mapped to SqlDbType.
                SqlDbType dbType = SqlKeyword.FromSqlTokenTypeToSqlDbType( id.TokenType ).Value;
                switch( dbType )
                {
                    case SqlDbType.Date:
                    case SqlDbType.DateTime:
                    case SqlDbType.SmallDateTime: return new SqlTypeDeclDateAndTime( dbType, id );

                    case SqlDbType.Time:
                    case SqlDbType.DateTime2:
                    case SqlDbType.DateTimeOffset:
                        {
                            SqlTokenOpenPar openPar;
                            SqlTokenClosePar closePar;
                            if( R.IsToken( out openPar, false ) )
                            {
                                SqlTokenLiteralInteger fractSecond;
                                if( !R.IsToken( out fractSecond, true ) ) return null;
                                if( fractSecond.Value > 7 )
                                {
                                    R.SetCurrentError( "Fractional seconds precision must be less or equal to 7." );
                                    return null;
                                }
                                if( !R.IsToken( out closePar, true ) ) return null;
                                return new SqlTypeDeclDateAndTime( dbType, id, openPar, fractSecond, closePar );
                            }
                            return new SqlTypeDeclDateAndTime( dbType, id );
                        }
                    case SqlDbType.Decimal:
                        {
                            SqlTokenOpenPar openPar;
                            SqlTokenClosePar closePar;
                            SqlTokenComma comma;
                            if( R.IsToken( out openPar, false ) )
                            {
                                SqlTokenLiteralInteger precision;
                                if( !R.IsToken( out precision, true ) ) return null;
                                if( precision.Value > 38 )
                                {
                                    R.SetCurrentError( "Precision must be less or equal to 38." );
                                    return null;
                                }
                                if( R.IsToken( out comma, false ) )
                                {
                                    SqlTokenLiteralInteger scale;
                                    if( !R.IsToken( out scale, true ) ) return null;
                                    if( scale.Value > precision.Value )
                                    {
                                        R.SetCurrentError( "Scale must be less or equal to Precision." );
                                        return null;
                                    }
                                    if( !R.IsToken( out closePar, true ) ) return null;
                                    return new SqlTypeDeclDecimal( id, openPar, precision, comma, scale, closePar );
                                }
                                if( !R.IsToken( out closePar, SqlTokenType.ClosePar, true ) ) return null;
                                return new SqlTypeDeclDecimal( id, openPar, precision, closePar );
                            }
                            return new SqlTypeDeclDecimal( id );
                        }
                    case SqlDbType.Char:
                    case SqlDbType.VarChar:
                    case SqlDbType.NChar:
                    case SqlDbType.NVarChar:
                    case SqlDbType.Binary:
                    case SqlDbType.VarBinary:
                    case SqlDbType.Float:
                        {
                            SqlTokenOpenPar openPar;
                            SqlTokenClosePar closePar;
                            if( R.IsToken( out openPar, false ) )
                            {
                                SqlTokenIdentifier sizeMax;
                                SqlTokenLiteralInteger size = null;
                                if( !R.IsToken( out sizeMax, SqlTokenType.Max, false ) && !R.IsToken( out size, true ) ) return null;
                                if( size != null && size.Value == 0 )
                                {
                                    R.SetCurrentError( "Size can not be 0." );
                                    return null;
                                }
                                if( !R.IsToken( out closePar, true ) ) return null;
                                return new SqlTypeDeclWithSize( dbType, id, openPar, (SqlToken)size ?? sizeMax, closePar );
                            }
                            return new SqlTypeDeclWithSize( dbType, id );
                        }
                    default:
                        {
                            return new SqlTypeDeclSimple( id );
                        }
                }
                #endregion
            }
            else
            {
                // A Userd defined type is simply one or more identifiers.
                ISqlIdentifier identifier = IsIdentifier( expected );
                if( identifier == null ) return null;
                return new SqlTypeDeclUserDefined( identifier );
            }
        }

        SqlExecuteAs IsExecuteAs( bool expected )
        {
            SqlTokenIdentifier execToken;
            if( !R.IsToken( out execToken, SqlTokenType.Execute, expected ) ) return null;

            SqlTokenIdentifier asToken;
            if( !R.IsToken( out asToken, SqlTokenType.As, true ) ) return null;

            SqlToken right;
            if( !R.IsToken( out right, true ) ) return null;

            return new SqlExecuteAs( execToken, asToken, right );
        }


    }


}

