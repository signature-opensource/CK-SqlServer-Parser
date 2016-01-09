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
        /// A named statement or any expression considered as a <see cref="SqlUnnamedStatement"/>.
        /// </summary>
        /// <param name="expected"></param>
        /// <returns></returns>
        public ISqlStatement IsExtendedStatement( bool expected )
        {
            ISqlStatement e = IsNamedStatement( false );
            if( e != null || R.IsErrorOrEndOfInput ) return e;
            if( R.Current.TokenType == SqlTokenType.End ) return null;
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
                return R.IsError ? null : new SqlReturnStatement( id, value, GetOptionalTerminator() );
            }
            if( id.TokenType == SqlTokenType.Execute )
            {
                R.MoveNext();
                return MatchExecute( id, false );
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
            if( id.TokenType == SqlTokenType.Insert )
            {
                R.MoveNext();
                return MatchInsertStatement( id );
            }
            if( id.TokenType == SqlTokenType.Merge )
            {
                R.MoveNext();
                return MatchMergeStatement( id );
            }
            if( id.TokenType == SqlTokenType.Update )
            {
                R.MoveNext();
                return MatchUpdateStatement( id );
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

        SqlInsertStatement MatchInsertStatement( SqlTokenIdentifier id )
        {
            MIUDHeader header = MatchCUDHeader( id );
            if( header == null ) return null;

            SqlTokenIdentifier intoT;
            R.IsToken( out intoT, SqlTokenType.Into, false );

            IUDTarget target = MatchCUDTarget();
            if( target == null ) return null;

            SqlEnclosedIdentifierCommaList columns = IsEnclosedCommaList( false, 1, IsIdentifier, ( o, i, c ) => new SqlEnclosedIdentifierCommaList( o, i, c ) );
            if( R.IsError ) return null;

            SqlOutputClause outputClause = IsOutputClause( false );
            if( R.IsError ) return null;

            ISqlNode values = null;
            SqlTokenIdentifier execT;
            if( R.IsToken( out execT, SqlTokenType.Execute, false ) )
            {
                values = MatchExecute( execT, true );
            }
            else values = IsAnyExpression( true );
            if( values == null ) return null;
            return new SqlInsertStatement( header, intoT, target, columns, outputClause, values, GetOptionalTerminator() );
        }

        MIUDHeader MatchCUDHeader( SqlTokenIdentifier id )
        {
            SqlTokenIdentifier top = null;
            ISqlNode topExpression = null;
            SqlTokenIdentifier percent = null;
            if( R.IsToken( out top, SqlTokenType.Top, false ) )
            {
                if( (topExpression = IsOneExpression( true )) == null ) return null;
                R.IsToken( out percent, SqlTokenType.Percent, false );
            }
            return new MIUDHeader( id, top, topExpression, percent );
        }

        IUDTarget MatchCUDTarget()
        {
            ISqlIdentifier targetId = IsIdentifier( true );
            if( targetId == null ) return null;
            ISqlNode target;
            if( targetId.IsToken( SqlTokenType.OpenRowSet )
                || targetId.IsToken( SqlTokenType.OpenQuery ) )
            {
                SqlEnclosedCommaList parameters = IsEnclosedCommaList( true );
                if( parameters == null ) return null;
                target = new SqlKoCall( targetId, parameters );
            }
            else target = targetId;
            SqlWithParOptions withTableHints = IsIdentifierPrefixedCommaList( false, SqlTokenType.With, 1, IsExtendedExpression, ( p, o, i, c ) => new SqlWithParOptions( p, o, i, c ) );
            return R.IsError ? null : new IUDTarget( target, withTableHints );
        }

        SqlUpdateStatement MatchUpdateStatement( SqlTokenIdentifier id )
        {
            MIUDHeader header = MatchCUDHeader( id );
            if( header == null ) return null;

            IUDTarget target = MatchCUDTarget();
            if( target == null ) return null;

            SqlTokenIdentifier setT;
            if( !R.IsToken( out setT, SqlTokenType.Set, true ) ) return null;

            SqlCommaList assigns = IsCommaList( 1, IsUpdateSetAssign );
            if( assigns == null ) return null;

            SqlOutputClause outputClause = IsOutputClause( false );
            if( R.IsError ) return null;

            SelectFrom from = IsFrom( false );
            if( R.IsError ) return null;

            SqlTokenIdentifier whereT;
            ISqlNode whereExpression = null;
            if( R.IsToken( out whereT, SqlTokenType.Where, false ) )
            {
                SqlTokenIdentifier currentT;
                if( R.IsToken( out currentT, SqlTokenType.Current, false ) )
                {
                    SqlTokenIdentifier ofT;
                    SqlTokenIdentifier globalT = null;
                    ISqlIdentifier cursorName = null;
                    if( !R.IsToken( out ofT, SqlTokenType.Of, true ) ) return null;
                    R.IsToken( out globalT, SqlTokenType.Global, false );
                    cursorName = IsIdentifier( true );
                    if( cursorName == null ) return null;
                    whereExpression = globalT != null 
                                        ? new SqlNodeList( currentT, ofT, globalT, cursorName )
                                        : new SqlNodeList( currentT, ofT, cursorName );
                }
                else whereExpression = IsOneExpression( true );
            }

            SqlOptionParOptions options = IsIdentifierPrefixedCommaList( false, SqlTokenType.Option, 1, IsExtendedExpression, ( p, o, i, c ) => new SqlOptionParOptions( p, o, i, c ) ); 

            return new SqlUpdateStatement( header, target, setT, assigns, outputClause, from, whereT, whereExpression, options, GetOptionalTerminator() );
        }

        SelectFrom IsFrom( bool expected )
        {
            SqlTokenIdentifier fromT;
            if( !R.IsToken( out fromT, SqlTokenType.From, expected ) ) return null;
            ISqlNode content = IsSqlNodeList<SqlToken>( SelectPartStopper, IsOneExpression, 1 );
            if( content == null ) return null;
            return new SelectFrom( fromT, content );
        }

        ISqlNode IsUpdateSetAssign( bool expected )
        {
            using( R.SetAssignmentContext( true ) )
            {
                ISqlNode safeExpr;
                if( R.Current.TokenType == SqlTokenType.From
                    || R.Current.TokenType == SqlTokenType.Where
                    || R.Current.TokenType == SqlTokenType.Option
                    || R.Current.TokenType == SqlTokenType.Output
                    || R.Current.TokenType == SqlTokenType.SemiColon
                    || (R.ParenthesisDepth == 0 && SqlToken.IsStatementStopper( R.Current ))
                    || (safeExpr = IsOneExpression( false )) == null
                    || !(safeExpr is SqlAssign || safeExpr is SqlKoCall) )
                {
                    if( expected ) R.SetCurrentError( "Expected assignment or call." );
                    return null;
                }
                return safeExpr;
            }
        }

        SqlMergeStatement MatchMergeStatement( SqlTokenIdentifier id )
        {
            MIUDHeader header = MatchCUDHeader( id );
            if( header == null ) return null;

            SqlTokenIdentifier intoT;
            R.IsToken( out intoT, SqlTokenType.Into, false );

            ISqlIdentifier targetTable = IsIdentifier( true );

            SqlWithParOptions withMergeHints = IsIdentifierPrefixedCommaList( false, SqlTokenType.With, 1, IsExtendedExpression, ( p, o, i, c ) => new SqlWithParOptions( p, o, i, c ) );

            SqlTokenIdentifier asT = null;
            SqlTokenIdentifier targetAliasName = null;
            SqlTokenIdentifier usingT;
            if( !R.IsToken( out usingT, SqlTokenType.Using, false ) )
            {
                R.IsToken( out asT, SqlTokenType.As, false );
                if( !R.IsToken( out targetAliasName, true ) ) return null;
            }
            if( !R.IsToken( out usingT, SqlTokenType.Using, true ) ) return null;

            // We cannot use IsAnyExpression here since on top level, 
            // we hit the WHEN NOT MATCHED THEN INSERT clause (note: the insert here has no target table)
            // that ends the AnyExpression (same for THEN UPDATE SET and THEN DELETE).
            // But, since MERGE statement MUST end with a ; (or is enclosed), we can collect 
            // every token until ; or ).
            // And we can not match IsOneExpression inside because INSERT/UPDATE/DELETE have not the same syntax
            // as their regular statement.
            ISqlNode unmodeledRemaider = IsSqlNodeList( R.GetDepthBasedStopper() );
            if( unmodeledRemaider == null ) return null;

            return new SqlMergeStatement( header, intoT, targetTable, withMergeHints, asT, targetAliasName, usingT, unmodeledRemaider, GetOptionalTerminator() );
        }

        ISqlNamedStatement MatchExecute( SqlTokenIdentifier execT, bool ignoreTerminator )
        {
            if( R.Current.TokenType == SqlTokenType.OpenPar )
            {
                var args = IsEnclosedCommaList( true );
                if( args == null ) return null;
                List<ISqlNode> optExec = null;
                using( R.SetAssignmentContext( true ) )
                {
                    SqlTokenIdentifier asT;
                    SqlTokenIdentifier userOrLoginT;
                    SqlTokenTerminal asAssignT;
                    SqlTokenLiteralString userOrLoginName;
                    if( R.IsToken( out asT, SqlTokenType.As, false ) )
                    {
                        optExec = new List<ISqlNode>();
                        optExec.Add( asT );
                        if( !R.IsToken( out userOrLoginT, SqlTokenType.User, false )
                            && !R.IsToken( out userOrLoginT, SqlTokenType.Login, false ) )
                        {
                            R.SetCurrentError( "Expected User or Login." );
                            return null;
                        }
                        optExec.Add( userOrLoginT );
                        if( !R.IsToken( out asAssignT, SqlTokenType.Assign, true ) ) return null;
                        if( !R.IsToken( out userOrLoginName, true ) ) return null;
                        optExec.Add( asAssignT );
                        optExec.Add( userOrLoginName );
                    }
                    SqlTokenIdentifier atT;
                    ISqlIdentifier atTarget;
                    if( R.IsToken( out atT, SqlTokenType.At, false ) )
                    {
                        if( optExec == null ) optExec = new List<ISqlNode>();
                        atTarget = IsIdentifier( true );
                        if( atTarget == null ) return null;
                        optExec.Add( atTarget );
                    }
                    var optionList = optExec != null ? new SqlNodeList( optExec ) : null;
                    return new SqlExecuteStringStatement( execT, args, optionList, ignoreTerminator ? null : GetOptionalTerminator() );
                }
            }
            SqlTokenIdentifier returnVar = null;
            SqlTokenTerminal assignT = null;
            ISqlIdentifier name = IsIdentifier( true );
            if( name != null && R.IsToken(out assignT, SqlTokenType.Assign, false))
            {
                if( !name.IsVariable )
                {
                    R.SetCurrentError( "Invalid syntax: variable name expected." );
                    return null;
                }
                returnVar = (SqlTokenIdentifier)name;
                name = IsIdentifier( true );
            }
            if( name == null ) return null;
            SqlCallParameterList parameters = IsCommaList( 0, IsCallParameter, i => new SqlCallParameterList( i ) );
            SqlWithOptions options = IsIdentifierPrefixedCommaList( false, SqlTokenType.With, 1, IsExecuteOption, (p,i) => new SqlWithOptions( p, i ) );
            return new SqlExecuteStatement( execT, returnVar, assignT, name, parameters, options, ignoreTerminator ? null : GetOptionalTerminator() );
        }

        ISqlNode IsExecuteOption( bool expected )
        {
            if( R.Current.TokenType == SqlTokenType.Recompile ) return R.Read<SqlTokenIdentifier>();
            if( R.Current.TokenType == SqlTokenType.Result )
            {
                SqlTokenIdentifier result = R.Read<SqlTokenIdentifier>();
                SqlTokenIdentifier sets;
                if( !R.IsToken( out sets, SqlTokenType.Sets, true ) ) return null;
                ISqlNode definition = IsEnclosedCommaList( false );
                if( definition == null )
                {
                    SqlTokenIdentifier undefinedOrNone;
                    if( !R.IsToken( out undefinedOrNone, SqlTokenType.Undefined, false )
                        && !R.IsToken( out undefinedOrNone, SqlTokenType.None, false ) )
                    {
                        R.SetCurrentError( "Expected sets definition, UNDEFINED or NONE." );
                        return null;
                    }
                    definition = undefinedOrNone;
                }
                return new SqlNodeList( result, sets, definition );
            }
            if( expected ) R.SetCurrentError( "Expected execute option." );
            return null;
        }

        SqlCallParameter IsCallParameter( bool expected )
        {
            SqlTokenIdentifier name = null;
            SqlTokenTerminal assignT = null;
            SqlTokenIdentifier variable = null;
            if( R.Current.TokenType == SqlTokenType.IdentifierVariable )
            {
                using( R.SetAssignmentContext( true ) )
                {
                    variable = R.Read<SqlTokenIdentifier>();
                    if( R.IsToken( out assignT, SqlTokenType.Assign, false ) )
                    {
                        name = variable;
                        variable = null;
                        expected = true;
                    }
                }
            }
            if( R.Current.TokenType == SqlTokenType.Default )
            {
                return new SqlCallParameter( name, assignT, R.Read<SqlTokenIdentifier>() );
            }
            if( variable != null || R.IsToken( out variable, SqlTokenType.IdentifierVariable, false ) )
            {
                SqlTokenIdentifier outputT = null;
                if( R.IsToken( out outputT, SqlTokenType.Output, false ) )
                {
                    return new SqlCallParameter( name, assignT, new SqlNodeList( variable, outputT ) );
                }
                return new SqlCallParameter( name, assignT, variable );
            }
            SqlBasicValue value = IsBasicValue( expected );
            if( value == null ) return null;
            return new SqlCallParameter( name, assignT, value );
        }

        SqlOutputClause IsOutputClause( bool expected )
        {
            SqlTokenIdentifier outputT;
            if( R.IsToken( out outputT, SqlTokenType.Output, expected ) )
            {
                SelectColumnList columns = IsCommaList( 0, IsSelectColumn, i => new SelectColumnList( i ) );
                SqlTokenIdentifier intoT;
                ISqlIdentifier targetTable = null;
                SqlEnclosedIdentifierCommaList columnNames = null;
                if( R.IsToken( out intoT, SqlTokenType.Into, false ) )
                {
                    targetTable = IsIdentifier( true );
                    if( targetTable == null ) return null;
                    columnNames = IsEnclosedCommaList( false, 1, IsIdentifier, ( o, i, c ) => new SqlEnclosedIdentifierCommaList( o, i, c ) );
                }
                return new SqlOutputClause( outputT, columns, intoT, targetTable, columnNames );
            }
            return null;
        }

        SqlCTEName IsSqlCTEName( bool expected )
        {
            SqlTokenIdentifier name;
            if( !R.IsToken( out name, expected ) ) return null;
            SqlEnclosedIdentifierCommaList columnNames = IsEnclosedCommaList( false, 1, IsIdentifier, ( o, i, c ) => new SqlEnclosedIdentifierCommaList( o, i, c ) );
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
            return new SqlCTEName( name, columnNames, asT, opener, select, closer );
        }

        ISqlNamedStatement IsStatementStartedByIdentifier( SqlTokenIdentifier id )
        {
            var content = IsAnyExpression( false, false );
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
            SqlTokenTerminal assign;
            SqlBasicValue defValue = null;
            using( R.SetAssignmentContext( true ) )
            {
                declVar = IsTypedIdentifer( t => t.IsVariable, expected );
                if( declVar == null ) return null;
                if( R.IsToken( out assign, SqlTokenType.Assign, false ) )
                {
                    defValue = IsBasicValue( true );
                }
            }
            SqlTokenIdentifier outputClause;
            R.IsToken( out outputClause, SqlTokenType.Output, false );

            SqlTokenIdentifier readonlyClause;
            R.IsToken( out readonlyClause, SqlTokenType.Readonly, false );

            return new SqlParameter( declVar, assign, defValue, outputClause, readonlyClause );
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
        /// Either a SqlDbType (int, sql_variant) or multiple identifiers that is a user defined type.
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
            else if( R.Current.TokenType.IsReservedKeyword() 
                        || R.Current.TokenType.IsVariableNameOrLiteral()
                        || R.Current.TokenType.IsIdentifierSpecial() )
            {
                if( expected ) R.SetCurrentError( "Expected type or user defined type (not a reserved keyword, a variable, a special identifier or a literal)." );
                return null;
            }
            else
            {
                // A User defined type is simply one or more identifiers.
                ISqlIdentifier identifier = IsIdentifier( expected );
                if( identifier == null ) return null;
                SqlTokenIdentifier tId = identifier as SqlTokenIdentifier;
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

