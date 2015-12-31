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
            ISqlNode n = IsAnyExpression( expected );
            if( n == null ) return null;
            return n.UnPar is ISelectSpecification 
                    ? (ISqlStatement)new SqlSelectStatement( n, GetOptionalTerminator() )
                    : new SqlUnnamedStatement( n, GetOptionalTerminator() );
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
                    SqlTokenIdentifier tranNameOrVariable = null;
                    SqlTokenIdentifier withToken = null;
                    SqlTokenIdentifier markToken = null;
                    SqlTokenLiteralString description = null;
                    if( !R.Current.TokenType.IsStartStatement() && R.IsToken( out tranNameOrVariable, false ) )
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
                SqlStatementList body = IsList( false, IsExtendedStatement, i => new SqlStatementList( i ) );
                if( body == null ) return null;
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
                SqlStatementList bodyCatch = IsList( true, IsExtendedStatement, i => new SqlStatementList( i ) );
                if( bodyCatch == null ) return null;
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
                if( R.Current.TokenType == SqlTokenType.Procedure )
                {
                    return MatchStoredProcedure( id );
                }
                if( R.Current.TokenType == SqlTokenType.View )
                {
                    return MatchView( id );
                }
                if( R.Current.TokenType == SqlTokenType.Function )
                {
                    return MatchFunction( id );
                }
                return IsStatementStartedByIdentifier( id );
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
                ISqlNode value = IsOneExpression( false );
                return R.IsError ? null : new SqlReturn( id, value, GetOptionalTerminator() );
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
                        else right = IsExtendedExpression( true );
                        if( right == null ) return null;
                        return new SqlSetVariable( id, left, assignT, right, GetOptionalTerminator() );
                    }
                    var options = IsAnyExpression( true );
                    if( options == null ) return null;
                    return new SqlSetOption( id, options, GetOptionalTerminator() );
                }
            }
            if( id.TokenType == SqlTokenType.Select )
            {
                ISqlNode select = IsOneExpression( true );
                if( select == null ) return null;
                Debug.Assert( select.UnPar is ISelectSpecification );
                return new SqlSelectStatement( select, GetOptionalTerminator() );
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
                SqlVariableDeclarationList declarations = IsCommaList( 1, IsVariableDeclaration, i => new SqlVariableDeclarationList( i ) );
                if( declarations == null ) return null;
                return new SqlDeclareVariable( id, declarations, GetOptionalTerminator() );
            }
            if( id.TokenType == SqlTokenType.With )
            {
                R.MoveNext();
                SqlCTENameList names = IsCommaList( 1, IsSqlCTEName, i => new SqlCTENameList( i ) );
                if( names == null ) return null;
                ISqlNamedStatement s = IsNamedStatement( true );
                if( s == null ) return null;
                //if( s.StatementKnownName != StatementKnownName.Select
                //    && s.StatementKnownName != StatementKnownName.Insert
                //    && s.StatementKnownName != StatementKnownName.Update
                //    && s.StatementKnownName != StatementKnownName.Delete
                //    && s.StatementKnownName != StatementKnownName.Merge )
                //{
                //    R.SetCurrentError( "Outer statement of a With (CTE) must be Select, Insert, Update, Delete or Merge." );
                //    return null;
                //}
                return new SqlCTEStatement( id, names, s );
            }
            if( id.IsStartStatement || id.TokenType == SqlTokenType.With )
            {
                R.MoveNext();
                return IsStatementStartedByIdentifier( id );
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

        SqlCTEName IsSqlCTEName( bool expected )
        {
            SqlTokenIdentifier name;
            if( !R.IsToken( out name, expected ) ) return null;
            SqlEnclosedIdentifierCommaList columns = IsEnclosedCommaList( false, 1, IsIdentifier, ( o, i, c ) => new SqlEnclosedIdentifierCommaList( o, i, c ) );
            SqlTokenIdentifier asT;
            if( !R.IsToken( out asT, SqlTokenType.As , true ) ) return null;
            SqlTokenOpenPar opener;
            if( !R.IsToken( out opener, true ) ) return null;
            ISqlNode select = IsOneExpression( true );
            if( !(select is ISelectSpecification) )
            {
                R.SetCurrentError( "select specification expected." );
                return null;
            }
            SqlTokenClosePar closer;
            if( !R.IsToken( out closer, true ) ) return null;
            return new SqlCTEName( name, columns, asT, opener, select, closer );
        }

        ISqlNamedStatement IsStatementStartedByIdentifier( SqlTokenIdentifier id )
        {
            var content = IsAnyExpression( false );
            if( content == null )
            {
                if( R.IsError ) return null;
                content = SqlNodeList.Empty;
            }
            return new SqlStatement( id, content, GetOptionalTerminator() );
        }

        SqlView MatchView( SqlTokenIdentifier alterOrCreate )
        {
            SqlTokenIdentifier type = R.Read<SqlTokenIdentifier>();
            Debug.Assert( type.TokenType == SqlTokenType.View );
            ISqlIdentifier name = IsIdentifier( true );
            if( name == null ) return null;

            // There must be at least one defined column if there is a parenthesis.
            SqlEnclosedIdentifierCommaList columns = IsEnclosedCommaList( false, 1, IsIdentifier, ( o, i, c ) => new SqlEnclosedIdentifierCommaList( o, i, c ) ); ;

            SqlTokenIdentifier asToken;
            SqlNodeList options = IsSqlNodeList( out asToken, t => t.TokenType == SqlTokenType.As );
            if( options == null ) return null;
            if( options.IsEmpty ) options = null;
             
            ISqlNode body = IsAnyExpression( true );
            if( body == null ) return null;
            return new SqlView( alterOrCreate, type, name, columns, options, asToken, body, GetOptionalTerminator() );
        }

        ISqlNamedStatement MatchFunction( SqlTokenIdentifier alterOrCreate )
        {
            SqlTokenIdentifier type = R.Read<SqlTokenIdentifier>();
            Debug.Assert( type.TokenType == SqlTokenType.Function );
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
            if( R.IsToken( out table, SqlTokenType.TableDbType, false ) )
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
                if( begin == null ) R.IsToken( out begin, SqlTokenType.Begin, false );
                SqlStatementList bodyStatements = IsList( true, IsExtendedStatement, i => new SqlStatementList( i ) );
                if( bodyStatements == null ) return null;
                SqlTokenIdentifier end = null;
                if( begin != null && !R.IsToken( out end, SqlTokenType.End, true ) ) return null;
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
                                GetOptionalTerminator() );
            }
        }

        bool IsFunctionOptionsAsAndBeginOrReturn( out SqlNodeList options, out SqlTokenIdentifier endOptionToken, out SqlTokenIdentifier asToken, out SqlTokenIdentifier beginOrReturn, bool isBegin = true )
        {
            asToken = beginOrReturn = null;
            options = IsSqlNodeList( out endOptionToken, t => t.TokenType == SqlTokenType.As || t.TokenType == SqlTokenType.Begin );
            if( options == null ) return false;
            if( options.IsEmpty ) options = null;
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

        SqlStoredProcedure MatchStoredProcedure( SqlTokenIdentifier alterOrCreate )
        {
            SqlTokenIdentifier type = R.Read<SqlTokenIdentifier>();
            Debug.Assert( type.TokenType == SqlTokenType.Procedure );
            ISqlIdentifier name = IsIdentifier( true );
            if( name == null ) return null;

            SqlParameterList parameters = IsParameterList( Parenthesis.Optional );
            if( parameters == null ) return null;

            SqlTokenIdentifier asToken;
            SqlNodeList options = IsSqlNodeList( out asToken, t => t.TokenType == SqlTokenType.As, IsExecuteAs );
            if( options == null ) return null;
            if( options.IsEmpty ) options = null;

            SqlTokenIdentifier begin;
            R.IsToken( out begin, SqlTokenType.Begin, false );
            SqlStatementList bodyStatements = IsList( true, IsExtendedStatement, i => new SqlStatementList( i ) );
            if( bodyStatements == null ) return null;
            SqlTokenIdentifier end = null;
            if( begin != null && !R.IsToken( out end, SqlTokenType.End, true ) ) return null;
            return new SqlStoredProcedure( alterOrCreate, type, name, parameters, options, asToken, begin, bodyStatements, end, GetOptionalTerminator() );
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
                declVar = IsTypedIdentifer( t => t.IsVariable, expected );
                if( declVar == null ) return null;
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
                declVar = IsTypedIdentifer( t => t.IsVariable, expected );
                if( declVar == null ) return null;
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
            SqlTokenIdentifier forToken;
            SqlNodeList options = IsSqlNodeList( out forToken, t => t.TokenType == SqlTokenType.For );
            if( options == null ) return null;
            if( options.IsEmpty ) options = null;

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
                        updateColumns = IsCommaList( 1, IsIdentifier, i => new SqlIdentifierCommaList( i ) );
                        if( updateColumns == null ) return null;
                    }
                }
            }
            if( insensitiveOrScrollT != null || scrollOrInsensitiveT != null || readTokenSql92 != null )
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
                return new SqlCursorDefinition( cursorToken, options, forToken, select, forOptionsToken, updateToken, ofToken, updateColumns );
            }
        }

        SqlTypedIdentifier IsTypedIdentifer( Predicate<SqlTokenIdentifier> idFilter, bool expected = true )
        {
            SqlTokenIdentifier identifier;
            if( !R.IsToken( out identifier, idFilter, expected ) ) return null;

            SqlTokenIdentifier asToken;
            R.IsToken( out asToken, SqlTokenType.As, false );

            ISqlUnifiedTypeDecl typeDecl = IsTypeDecl( true );
            if( typeDecl == null ) return null;

            return new SqlTypedIdentifier( identifier, asToken, typeDecl );
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
                    case SqlDbType.Structured:
                        {
                            SqlTokenOpenPar opener;
                            if( !R.IsToken( out opener, true ) ) return null;
                            ISqlNode content = IsAnyExpression( true );
                            if( content == null ) return null;
                            SqlTokenClosePar closer;
                            if( !R.IsToken( out closer, true ) ) return null;
                            return new SqlTypeDeclTable( id, opener, content, closer );
                        }
                    default:
                        {
                            return new SqlTypeDeclSimple( dbType, id );
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

