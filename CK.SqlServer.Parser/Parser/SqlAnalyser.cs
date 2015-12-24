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
        readonly SqlTokenReader R;

        public class ErrorResult : ISqlServerParserError
        {
            readonly string _errorMessage;
            readonly string _headSource;
            public bool IsError { get { return this != NoError; } }
            public static implicit operator bool ( ErrorResult r ) { return r == NoError; }

            internal ErrorResult( string errorMessage, string headSource )
            {
                Debug.Assert( NoError == null || (errorMessage != null && headSource != null) );
                _errorMessage = errorMessage;
                _headSource = headSource;
            }

            public string ErrorMessage { get { return _errorMessage; } }

            public string HeadSource { get { return _headSource; } }

            public override string ToString()
            {
                return IsError ? String.Format( "Error: {0}\r\nText: {1}", _errorMessage, _headSource ) : "<success>";
            }

            static internal readonly ErrorResult NoError = new ErrorResult( null, null );

            /// <summary>
            /// Logs the error message if <see cref="IsError"/> is true, otherwise does nothing.
            /// </summary>
            /// <param name="monitor">Monitor to log into.</param>
            /// <param name="asWarning">True to log a warning instead of an error.</param>
            public void LogOnError( IActivityMonitor monitor, bool asWarning = false )
            {
                if( monitor == null ) throw new ArgumentNullException( "monitor" );
                if( IsError )
                {
                    using( asWarning ? monitor.OpenWarn().Send( _errorMessage ) : monitor.OpenError().Send( _errorMessage ) )
                    {
                        // OpenError automatically sets the filter to Debug for the group, but not OpenWarn.
                        if( asWarning ) monitor.SetMinimalFilter( LogFilter.Debug );
                        monitor.Info().Send( _headSource );
                    }
                }
            }
        }

        [DebuggerStepThrough]
        public static ErrorResult ParseStatement( out ISqlStatement statement, string text )
        {
            SqlAnalyser a = new SqlAnalyser( new SqlTokenizer(), text );
            if( a.IsStatement( out statement, true ) ) return ErrorResult.NoError;
            return a.CreateErrorResult();
        }

        [DebuggerStepThrough]
        public static ErrorResult ParseStatement<T>( out T statement, string text ) where T : class
        {
            statement = null;

            ISqlStatement st;
            SqlAnalyser a = new SqlAnalyser( new SqlTokenizer(), text );
            if( !a.IsStatement( out st, true ) ) return a.CreateErrorResult();

            statement = st as T;
            if( statement == null )
            {
                a.R.SetCurrentError( "Expected '{0}' statement but found a '{1}'.", statement.GetType().Name, st.GetType().Name );
                return a.CreateErrorResult();
            }
            return ErrorResult.NoError;
        }

        [DebuggerStepThrough]
        public static ErrorResult ParseExpression( out ISqlNode expression, string text )
        {
            SqlAnalyser a = new SqlAnalyser( new SqlTokenizer(), text );
            if( a.IsExpression( out expression, 0, true ) ) return ErrorResult.NoError;
            return a.CreateErrorResult();
        }

        [DebuggerStepThrough]
        public static ErrorResult Parse( out SqlNodeList sql, string text )
        {
            sql = null;
            SqlAnalyser a = new SqlAnalyser( new SqlTokenizer(), text );
            List<ISqlNode> items = new List<ISqlNode>();
            IsFunc<ISqlStatement> isStatement = a.IsStatement;
            IsFunc<ISqlNode> isStatementOrExpr = isStatement.AsNode().Or( a.IsMultiExpression );
            if( !a.R.CollectUntil<SqlTokenError>( items, isStatementOrExpr ) ) return a.CreateErrorResult();
            sql = new SqlNodeList( items );
            return ErrorResult.NoError;
        }

        SqlAnalyser( SqlTokenizer t, string text )
        {
            R = new SqlTokenReader( t.Parse( text ), t.ToString, t.GetTokenPosition );
            R.MoveNext();
        }

        public override string ToString()
        {
            return R.ToString();
        }

        ErrorResult CreateErrorResult()
        {
            return new ErrorResult( R.GetErrorMessage(), R.ToString() );
        }

        bool IsStatement( out ISqlStatement statement, bool expected = true )
        {
            statement = null;
            SqlTokenIdentifier id = R.Current as SqlTokenIdentifier;
            // A statement starts with an identifier that must be non quoted and not a variable.
            if( id == null || id.IsQuoted || id.IsVariable )
            {
                if( R.Current.TokenType == SqlTokenType.SemiColon )
                {
                    statement = new SqlEmptyStatement( R.Read<SqlTokenTerminal>() );
                    return true;
                }
                if( R.Current.TokenType == SqlTokenType.OpenPar )
                {
                    ISqlNode e;
                    if( !IsExpression( out e, 0, true ) ) return false;
                    statement = new SqlUnmodeledStatement( e, GetOptionalTerminator() );
                    return true;
                }
                if( expected ) R.SetCurrentError( "Statement expected." );
                return false;
            }
            // End Conversation ... is a statement.
            // Otherwise, we handle it: this is the end of a block above.
            if( id.TokenType == SqlTokenType.End && R.RawLookup.TokenType != SqlTokenType.Conversation )
            {
                if( expected ) R.SetCurrentError( "Statement expected." );
                return false;
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
                            if( !R.IsUnquotedIdentifier( out markToken, "mark", true ) ) return false;
                            R.IsToken( out description, false );
                        }
                    }
                    statement = new SqlBeginTransaction( id, tranOrTry, tranNameOrVariable, withToken, markToken, description, GetOptionalTerminator() );
                    return true;
                }
                R.IsToken( out tranOrTry, SqlTokenType.Try, false );
                SqlStatementList body;
                if( !IsStatementList( out body, true ) ) return false;
                SqlTokenIdentifier end;
                if( !R.IsToken( out end, SqlTokenType.End, true ) ) return false;
                if( tranOrTry == null )
                {
                    statement = new SqlBeginEndBlock( id, body, end );
                    return true;
                }
                // Begin Try ... End Try Begin Catch ... End Catch.
                SqlTokenIdentifier endTry;
                if( !R.IsToken( out endTry, SqlTokenType.Try, true ) ) return false;
                SqlTokenIdentifier begCatch, begCatchToken;
                if( !R.IsToken( out begCatch, SqlTokenType.Begin, true ) || !R.IsToken( out begCatchToken, SqlTokenType.Catch, true ) ) return false;
                SqlStatementList bodyCatch;
                if( !IsStatementList( out bodyCatch, true ) ) return false;
                SqlTokenIdentifier endCatch, endCatchToken;
                if( !R.IsToken( out endCatch, SqlTokenType.End, true ) || !R.IsToken( out endCatchToken, SqlTokenType.Catch, true ) ) return false;
                statement = new SqlTryCatch( id, tranOrTry,
                                                   body,
                                                   end, endTry, begCatch, begCatchToken,
                                                   bodyCatch,
                                                   endCatch, endCatchToken,
                                                   GetOptionalTerminator() );
                return true;
            }
            if( id.TokenType == SqlTokenType.Create || id.TokenType == SqlTokenType.Alter )
            {
                R.MoveNext();
                SqlTokenIdentifier type;
                if( !R.IsToken( out type, true ) ) return false;
                if( type.TokenType == SqlTokenType.Procedure )
                {
                    SqlStoredProcedure sp;
                    if( !IsStoredProcedure( out sp, id, type ) ) return false;
                    statement = sp;
                    return true;
                }
                if( type.TokenType == SqlTokenType.View )
                {
                    SqlView view;
                    if( !IsView( out view, id, type ) ) return false;
                    statement = view;
                    return true;
                }
                if( type.TokenType == SqlTokenType.Function )
                {
                    if( !IsFunction( out statement, id, type ) ) return false;
                    return true;
                }
            }
            if( id.TokenType == SqlTokenType.Break || id.TokenType == SqlTokenType.Continue )
            {
                R.MoveNext();
                statement = new SqlMonoStatement( id, GetOptionalTerminator() );
                return true;
            }
            if( id.TokenType == SqlTokenType.If )
            {
                R.MoveNext();
                ISqlNode expr;
                if( !IsExpression( out expr, 0, true ) ) return false;
                ISqlStatement thenSt;
                if( !IsStatement( out thenSt, true ) ) return false;
                SqlTokenIdentifier elseToken;
                ISqlStatement elseSt = null;
                if( R.IsToken( out elseToken, SqlTokenType.Else, false ) )
                {
                    if( !IsStatement( out elseSt, true ) ) return false;
                }
                statement = new SqlIf( id, expr, thenSt, elseToken, elseSt, GetOptionalTerminator() );
                return true;
            }
            if( id.TokenType == SqlTokenType.Return )
            {
                R.MoveNext();
                ISqlNode expr;
                IsExpression( out expr, 0, false );
                statement = new SqlReturn( id, expr, GetOptionalTerminator() );
                return true;
            }
            if( id.TokenType == SqlTokenType.Goto )
            {
                R.MoveNext();
                SqlTokenIdentifier target;
                if( !R.IsToken( out target, true ) ) return false;
                statement = new SqlGoto( id, target, GetOptionalTerminator() );
                return true;
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
                        if( !R.IsToken( out assignT, t => (t.TokenType & SqlTokenType.IsAssignOperator) != 0, true ) ) return false;

                        SqlTokenIdentifier cursorT;
                        if( R.IsToken( out cursorT, SqlTokenType.Cursor, false ) )
                        {
                            ISqlCursorDefinition c;
                            if( !MatchCursorDefinition( null, null, cursorT, out c ) )
                            {
                                Debug.Assert( R.IsError );
                                return false;
                            }
                            right = c;
                        }
                        else if( !IsExpression( out right, 0, true ) ) return false;
                        statement = new SqlSetVariable( id, left, assignT, right, GetOptionalTerminator() );
                        return true;
                    }
                    SqlNodeList list = ReadToTerminatorOrPossibleStartStatement();
                    if( list == null ) return false;
                    statement = new SqlSetOption( id, list, GetOptionalTerminator() );
                }
                return true;
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
                    if( !R.IsToken( out name, true ) ) return false;
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
                    if( !R.IsToken( out cursorToken, SqlTokenType.Cursor, true ) ) return false;
                    ISqlCursorDefinition cursorExpr;
                    if( !MatchCursorDefinition( insensitiveOrScrollT, scrollOrInsensitiveT, cursorToken, out cursorExpr ) )
                    {
                        Debug.Assert( R.IsError );
                        return false;
                    }
                    statement = new SqlDeclareCursor( id, name, cursorExpr, GetOptionalTerminator() );
                    return true;
                }
                SqlTokenOpenPar openPar;
                SqlTokenClosePar closePar;
                List<ISqlNode> items;
                if( !IsCommaList<SqlDeclareVariable>( out openPar, out items, out closePar, false, IsVariableDeclare ) ) return false;
                if( openPar != null || closePar != null ) return R.SetCurrentError( "Unexpected parenthesis in Declare statement." );
                if( items.Count == 0 ) return R.SetCurrentError( "Declare expect at least one variable." );
                SqlDeclareVariableList declarations = new SqlDeclareVariableList( items );
                statement = new SqlDeclare( id, declarations, GetOptionalTerminator() );
                return true;
            }
            bool canBeAStatement = id.IsStartStatement || id.TokenType == SqlTokenType.With;
            if( id.IsStartStatement || id.TokenType == SqlTokenType.With )
            {
                SqlNodeList unmodeled = ReadToTerminatorOrPossibleStartStatement();
                if( unmodeled == null ) return false;
                statement = new SqlUnmodeledStatement( unmodeled, GetOptionalTerminator() );
                return true;
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
                return false;
            }
            R.MoveNext();
            R.MoveNext();
            statement = new SqlLabelDefinition( id, colon );
            return true;
        }

        SqlNodeList ReadToTerminatorOrPossibleStartStatement()
        {
            List<ISqlNode> items = new List<ISqlNode>();
            // Unconditionally adds the current token since it may be a StartStatement.
            items.Add( R.Read<SqlToken>() );
            return R.CollectUntil<SqlToken>( items, IsOneExpression, SqlToken.IsTerminatorOrPossibleStartStatement ) 
                ? new SqlNodeList( items )
                : null;
        }

        bool IsStatementList( out SqlStatementList l, bool atLeastOneStatement )
        {
            l = null;
            var statements = new List<ISqlStatement>();
            ISqlStatement st;
            while( IsStatement( out st, false ) )
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

        bool IsView( out SqlView view, SqlTokenIdentifier alterOrCreate, SqlTokenIdentifier type )
        {
            view = null;

            ISqlIdentifier name;
            if( !IsIdentifier( out name, true ) ) return false;

            SqlColumnNameList columns;
            IsColumnList( out columns );

            SqlNodeList options;
            SqlTokenIdentifier asToken;
            if( !IsSqlNodeList( out options, out asToken, t => t.TokenType == SqlTokenType.As ) ) return false;

            SqlNodeList body;
            SqlTokenTerminal term;
            if( !IsSqlNodeList( out body, out term, t => t.TokenType == SqlTokenType.SemiColon, true, IsOneExpression ) ) return false;
            term = GetOptionalTerminator();
            view = new SqlView( alterOrCreate, type, name, columns, options, asToken, body, term );
            return true;
        }

        bool IsColumnList( out SqlColumnNameList columns )
        {
            columns = null;
            SqlTokenOpenPar openPar;
            SqlTokenClosePar closePar;
            List<ISqlNode> items;
            if( R.Current.TokenType != SqlTokenType.OpenPar ) return false;
            if( !IsCommaList<SqlTokenIdentifier>( out openPar, out items, out closePar, true, R.IsToken ) ) return false;
            columns = new SqlColumnNameList( openPar, items, closePar );
            return true;
        }

        bool IsFunction( out ISqlStatement func, SqlTokenIdentifier alterOrCreate, SqlTokenIdentifier type )
        {
            func = null;

            /*
            CREATE FUNCTION [ schema_name. ] function_name 
                ( [ { @parameter_name [ AS ][ type_schema_name. ] parameter_data_type [ = default ] [ READONLY ] } 
                    [ ,...n ]
                  ]
                )
            RETURNS 
             */
            ISqlIdentifier name;
            if( !IsIdentifier( out name, true ) ) return false;

            SqlParameterList parameters;
            if( !IsParameterList( out parameters, requiresParenthesis: true ) ) return false;

            SqlTokenIdentifier returns;
            if( !R.IsToken( out returns, SqlTokenType.Returns, true ) ) return false;

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
                if( !IsFunctionOptionsAsAndBeginOrReturn( out options, out endOptionToken, out asToken, out returnToken, isBegin: false ) ) return false;
                ISqlNode e;
                if( !IsExpression( out e, 0, true ) ) return false;
                SqlTokenTerminal term = GetOptionalTerminator();
                SelectSpecification q = e.UnPar as SelectSpecification;
                if( q == null ) return R.SetCurrentError( "Expected select statement." );

                func = new SqlFunctionInlineTable(
                                alterOrCreate,
                                type,
                                name,
                                parameters,
                                returns,
                                table,
                                options,
                                asToken,
                                returnToken,
                                q,
                                term );
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
                ISqlUnifiedTypeDecl returnScalarType;
                if( !IsTypeDecl( out returnScalarType, true ) ) return false;
                // Scalar Function Syntax
                SqlNodeList options;
                SqlTokenIdentifier endOptionToken;
                SqlTokenIdentifier asToken;
                SqlTokenIdentifier begin;
                if( !IsFunctionOptionsAsAndBeginOrReturn( out options, out endOptionToken, out asToken, out begin ) ) return false;
                SqlStatementList bodyStatements;
                SqlTokenIdentifier end;
                SqlTokenTerminal term;
                if( !IsBodyStatementListSafe( out bodyStatements, ref begin, out end, out term ) ) return false;
                func = new SqlFunctionScalar(
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
            return true;
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

        bool IsStoredProcedure( out SqlStoredProcedure sp, SqlTokenIdentifier alterOrCreate, SqlTokenIdentifier type )
        {
            sp = null;

            ISqlIdentifier name;
            if( !IsIdentifier( out name, true ) ) return false;

            SqlParameterList parameters;
            if( !IsParameterList( out parameters, requiresParenthesis: false ) ) return false;

            IsFunc<SqlExecuteAs> isExecAs = IsExecuteAs;
            SqlNodeList options;
            SqlTokenIdentifier asToken;
            if( !IsSqlNodeList( out options, out asToken, t => t.TokenType == SqlTokenType.As, false, isExecAs.AsNode() ) ) return false;

            SqlTokenIdentifier begin = null;
            SqlStatementList bodyStatements;
            SqlTokenIdentifier end;
            SqlTokenTerminal term;
            if( !IsBodyStatementListSafe( out bodyStatements, ref begin, out end, out term ) ) return false;
            if( begin == null )
            {
                sp = new SqlStoredProcedure( alterOrCreate, type, name, parameters, options, asToken, null, bodyStatements, null, term );
            }
            else
            {
                sp = new SqlStoredProcedure( alterOrCreate, type, name, parameters, options, asToken, begin, bodyStatements, end, term );
            }
            return true;
        }

        bool IsBodyStatementListSafe( 
            out SqlStatementList bodyStatements, 
            ref SqlTokenIdentifier begin, 
            out SqlTokenIdentifier end, 
            out SqlTokenTerminal term )
        {
            end = null;
            term = null;
            using( var collector = R.OpenCollector() )
            {
                if( begin == null ) R.IsToken( out begin, SqlTokenType.Begin, false );

                // Attempts to read a statement list. If it fails, reads the whole stream as an unmodeled list of tokens.
                if( IsStatementList( out bodyStatements, true ) )
                {
                    if( begin != null && !R.IsToken( out end, SqlTokenType.End, true ) ) return false;
                    term = GetOptionalTerminator();
                }
                else
                {
                    // Collects all tokens and generates a Statement list with one unmodeled list of tokens.
                    // Saves the begin/end and semi colon terminator if possible.
                    term = collector.ReadToEnd();
                    if( begin != null )
                    {
                        if( collector.Count > 0 && collector[collector.Count - 1].TokenType == SqlTokenType.End )
                        {
                            end = (SqlTokenIdentifier)collector[collector.Count - 1];
                        }
                        else
                        {
                            return R.SetCurrentError( "Missing END." );
                        }
                    }
                    var t = new SqlUnmodeledStatement( new SqlNodeList( begin != null ? collector.Skip( 1 ).Take( collector.Count - 2 ) : collector ) );
                    bodyStatements = new SqlStatementList( new[] { t } );
                }
            }
            return true;
        }

        bool IsParameterList( out SqlParameterList parameters, bool requiresParenthesis )
        {
            parameters = null;
            SqlTokenOpenPar openPar;
            SqlTokenClosePar closePar;
            List<ISqlNode> items;
            if( !IsCommaList<SqlParameter>( out openPar, out items, out closePar, requiresParenthesis, IsParameter ) ) return false;
            parameters = new SqlParameterList( openPar, items, closePar );
            return true;
        }

        bool IsParameter( out SqlParameter parameter, bool expected )
        {
            parameter = null;
            SqlTypedIdentifier declVar;
            SqlParameterDefaultValue defValue = null;
            using( R.SetAssignmentContext( true ) )
            {
                if( !IsTypedIdentifer( out declVar, t => t.IsVariable, expected ) ) return false;
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
                        if( !R.IsToken( out value, true ) ) return false;
                        defValue = new SqlParameterDefaultValue( assign, minusSign, value );
                    }
                }
            }
            SqlTokenIdentifier outputClause;
            R.IsToken( out outputClause, SqlTokenType.Output, false );

            SqlTokenIdentifier readonlyClause;
            R.IsToken( out readonlyClause, SqlTokenType.Readonly, false );

            parameter = new SqlParameter( declVar, defValue, outputClause, readonlyClause );
            return true;
        }

        bool IsVariableDeclare( out SqlDeclareVariable declare, bool expected = true )
        {
            declare = null;
            SqlTypedIdentifier declVar;
            SqlTokenTerminal assignToken = null;
            ISqlNode initialValue = null;
            // Syntax: declare @name [as] type
            using( R.SetAssignmentContext( true ) )
            {
                if( !IsTypedIdentifer( out declVar, t => t.IsVariable, expected ) ) return false;
                if( R.IsToken( out assignToken, SqlTokenType.Assign, false ) )
                {
                    if( !IsExpression( out initialValue, SqlTokenizer.PrecedenceLevel( SqlTokenType.Comma ), true ) ) return false;
                }
            }
            declare = new SqlDeclareVariable( declVar, assignToken, initialValue );
            return true;
        }

        bool MatchCursorDefinition( 
            SqlTokenIdentifier insensitiveOrScrollT, 
            SqlTokenIdentifier scrollOrInsensitiveT, 
            SqlTokenIdentifier cursorToken, 
            out ISqlCursorDefinition cursor )
        {
            cursor = null;
            SqlNodeList options;
            SqlTokenIdentifier forToken;
            if( !IsSqlNodeList( out options, out forToken, t => t.TokenType == SqlTokenType.For ) ) return false;
            Debug.Assert( forToken.TokenType == SqlTokenType.For );
            ISqlNode eSelect;
            if( !IsExpression( out eSelect, 0, true ) ) return false;
            ISelectSpecification select;
            if( (select = eSelect as ISelectSpecification) == null ) return R.SetCurrentError( "Select statement expected." );
            SqlTokenIdentifier forOptionsToken;
            SqlTokenIdentifier readTokenSql92 = null;
            SqlTokenIdentifier onlyTokenSql92 = null;
            SqlTokenIdentifier updateToken = null;
            SqlTokenIdentifier ofToken = null;
            SqlIdentifierCommaList updateColumns = null;
            if( R.IsToken( out forOptionsToken, SqlTokenType.For, false ) )
            {
                if( R.IsUnquotedIdentifier( out readTokenSql92, "read", false ) )
                {
                    if( !R.IsToken( out onlyTokenSql92, SqlTokenType.Only, true ) ) return false;
                }
                else
                {
                    if( !R.IsToken( out updateToken, SqlTokenType.Update, true ) ) return false;
                    if( R.IsToken( out ofToken, SqlTokenType.Of, false ) )
                    {
                        List<ISqlNode> columns = null;
                        if( !IsCommaListNonEnclosed<SqlTokenIdentifier>( out columns, R.IsToken, true ) ) return false;
                        updateColumns = new SqlIdentifierCommaList( columns );
                    }
                }
            }
            if( readTokenSql92 != null )
            {
                if( options != null ) return R.SetCurrentError( "Sql92: There can be no options in 'cursor [Options] for <select> for read only;'." );
                cursor = new SqlCursorDefinition92( insensitiveOrScrollT, scrollOrInsensitiveT, cursorToken, forToken, select, forOptionsToken, readTokenSql92, onlyTokenSql92, updateToken, ofToken, updateColumns );
            }
            else
            {
                if( insensitiveOrScrollT != null || scrollOrInsensitiveT != null )
                {
                    return R.SetCurrentError( "INSENSITIVE or SCROLL cursor requires Sql92 syntax." );
                }
                cursor = new SqlCursorDefinition( cursorToken, options, forToken, select, forOptionsToken, updateToken, ofToken, updateColumns );
            }
            return true;
        }

        bool IsTypedIdentifer( out SqlTypedIdentifier declVar, Predicate<SqlTokenIdentifier> idFilter, bool expected = true )
        {
            declVar = null;
            SqlTokenIdentifier identifier;
            if( !R.IsToken( out identifier, idFilter, expected ) ) return false;

            SqlTokenIdentifier asToken;
            R.IsToken( out asToken, SqlTokenType.As, false );

            ISqlUnifiedTypeDecl typeDecl;
            if( !IsTypeDecl( out typeDecl, true ) ) return false;

            declVar = new SqlTypedIdentifier( identifier, asToken, typeDecl );
            return true;
        }

        /// <summary>
        /// Is a SqlExprTypeDecl: either a SqlDbType (int, sql_variant) or multiple identifiers that is a user defined type.
        /// </summary>
        /// <returns></returns>
        bool IsTypeDecl( out ISqlUnifiedTypeDecl typeDecl, bool expected )
        {
            typeDecl = null;
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
                    case SqlDbType.SmallDateTime:
                        {
                            typeDecl = new SqlTypeDeclDateAndTime( dbType, id );
                            break;
                        }
                    case SqlDbType.Time:
                    case SqlDbType.DateTime2:
                    case SqlDbType.DateTimeOffset:
                        {
                            SqlTokenOpenPar openPar;
                            SqlTokenClosePar closePar;
                            if( R.IsToken( out openPar, false ) )
                            {
                                SqlTokenLiteralInteger fractSecond;
                                if( !R.IsToken( out fractSecond, true ) ) return false;
                                if( fractSecond.Value > 7 )
                                {
                                    R.SetCurrentError( "Fractional seconds precision must be less or equal to 7." );
                                    return false;
                                }
                                if( !R.IsToken( out closePar, true ) ) return false;
                                typeDecl = new SqlTypeDeclDateAndTime( dbType, id, openPar, fractSecond, closePar );
                            }
                            else typeDecl = new SqlTypeDeclDateAndTime( dbType, id );
                            break;
                        }
                    case SqlDbType.Decimal:
                        {
                            SqlTokenOpenPar openPar;
                            SqlTokenClosePar closePar;
                            SqlTokenComma comma;
                            if( R.IsToken( out openPar, false ) )
                            {
                                SqlTokenLiteralInteger precision;
                                if( !R.IsToken( out precision, true ) ) return false;
                                if( precision.Value > 38 )
                                {
                                    R.SetCurrentError( "Precision must be less or equal to 38." );
                                    return false;
                                }
                                if( R.IsToken( out comma, false ) )
                                {
                                    SqlTokenLiteralInteger scale;
                                    if( !R.IsToken( out scale, true ) ) return false;
                                    if( scale.Value > precision.Value )
                                    {
                                        R.SetCurrentError( "Scale must be less or equal to Precision." );
                                        return false;
                                    }
                                    if( !R.IsToken( out closePar, true ) ) return false;
                                    typeDecl = new SqlTypeDeclDecimal( id, openPar, precision, comma, scale, closePar );
                                }
                                else
                                {
                                    if( !R.IsToken( out closePar, SqlTokenType.ClosePar, true ) ) return false;
                                    typeDecl = new SqlTypeDeclDecimal( id, openPar, precision, closePar );
                                }
                            }
                            else typeDecl = new SqlTypeDeclDecimal( id );
                            break;
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
                                if( !R.IsToken( out sizeMax, SqlTokenType.Max, false ) && !R.IsToken( out size, true ) ) return false;
                                if( size != null && size.Value == 0 )
                                {
                                    R.SetCurrentError( "Size can not be 0." );
                                    return false;
                                }
                                if( !R.IsToken( out closePar, true ) ) return false;
                                typeDecl = new SqlTypeDeclWithSize( dbType, id, openPar, (SqlToken)size ?? sizeMax, closePar );
                            }
                            else typeDecl = new SqlTypeDeclWithSize( dbType, id );
                            break;
                        }
                    default:
                        {
                            typeDecl = new SqlTypeDeclSimple( id );
                            break;
                        }
                }
                #endregion
                Debug.Assert( typeDecl != null );
            }
            else
            {
                // A Userd defined type is simply one or more identifiers.
                ISqlIdentifier identifier;
                if( IsIdentifier( out identifier, expected ) ) return false;
                typeDecl = new SqlTypeDeclUserDefined( identifier );
            }
            return true;
        }

        bool IsExecuteAs( out SqlExecuteAs execAs, bool expected )
        {
            execAs = null;
            SqlTokenIdentifier execToken;
            if( !R.IsToken( out execToken, SqlTokenType.Execute, expected ) ) return false;

            SqlTokenIdentifier asToken;
            if( !R.IsToken( out asToken, SqlTokenType.As, true ) ) return false;

            SqlToken right;
            if( !R.IsToken( out right, true ) ) return false;

            execAs = new SqlExecuteAs( execToken, asToken, right );
            return true;
        }


    }


}

