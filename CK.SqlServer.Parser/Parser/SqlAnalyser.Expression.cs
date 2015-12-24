using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using CK.SqlServer;
using CK.Core;

namespace CK.SqlServer.Parser
{
    public partial class SqlAnalyser
    {

        bool IsExpression( out ISqlNode e, int rightBindingPower, bool expected )
        {
            e = null;
            if( R.IsErrorOrEndOfInput || !IsExpressionNud( ref e ) )
            {
                if( expected ) R.SetCurrentError( "Expected expression." );
                return false;
            }
            // Not (as a left denotation) is the same as a between or a like (since it introduces them).
            // This could have been handled with a left and right binding power instead of only one power per operator.
            while( !R.IsErrorOrEndOfInput
                    && ((R.Current.TokenType == SqlTokenType.Not && SqlTokenizer.PrecedenceLevel( SqlTokenType.OpNotRightLevel ) > rightBindingPower)
                        ||
                        (R.Current.TokenType != SqlTokenType.Not && R.CurrentPrecedenceLevel > rightBindingPower)) )
            {
                if( !ExpressionCombineLed( ref e ) ) break;
            }
            return !R.IsError;
        }

        /// <summary>
        /// Handles NUD (NUll left Denotation): the token has nothing to its left (it is a prefix).
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        bool IsExpressionNud( ref ISqlNode e )
        {
            Debug.Assert( e == null );
            Debug.Assert( !R.IsErrorOrEndOfInput );
            // Handles strings and numbers.
            if( (R.Current.TokenType & SqlTokenType.LitteralMask) != 0 )
            {
                e = R.Read<SqlTokenBaseLiteral>();
                return true;
            }
            if( R.Current.TokenType == SqlTokenType.SemiColon )
            {
                e = new SqlEmptyStatement( R.Read<SqlTokenTerminal>() );
                return true;
            }
            if( R.Current.TokenType == SqlTokenType.Minus
                || R.Current.TokenType == SqlTokenType.Plus
                || R.Current.TokenType == SqlTokenType.BitwiseNot
                || R.Current.TokenType == SqlTokenType.Not )
            {
                int precedenceLevel = R.CurrentPrecedenceLevel;
                SqlToken op = R.Read<SqlToken>();
                ISqlNode right;
                if( !IsExpression( out right, precedenceLevel, true ) ) return false;
                e = new SqlUnaryOperator( op, right );
                return true;
            }
            if( R.Current.TokenType == SqlTokenType.Mult )
            {
                // Handles a Nud * as a identifier.
                // Actual syntax prohibits *.part or *::part but for
                // coherency, we consider this as a valid construct.
                var star = R.Read<SqlToken>();
                var starT = new SqlTokenIdentifier( SqlTokenType.IdentifierStar, "*", star.LeadingTrivias, star.TrailingTrivias );
                ISqlIdentifier identifier;
                if( !IsIdentifier( out identifier, true, starT ) ) return false;
                e = identifier;
                return true;
            }
            if( R.Current.TokenType == SqlTokenType.OpenPar )
            {
                SqlTokenOpenPar openPar = R.Read<SqlTokenOpenPar>();
                if( IsExpressionOrParOrNodeListInternal( out e, openPar, t => t is SqlTokenClosePar, false, false ) ) return true;
                return R.SetCurrentError( "Expected )." );
            }
            if( (R.Current.TokenType & SqlTokenType.IsIdentifier) != 0 )
            {
                SqlTokenIdentifier id = R.Read<SqlTokenIdentifier>();
                if( id.TokenType == SqlTokenType.Select )
                {
                    SelectSpecification select;
                    if( !MatchSelectSpecification( out select, id ) )
                    {
                        Debug.Assert( R.IsError );
                        return false;
                    }
                    e = select;
                    return true;
                }
                if( id.TokenType == SqlTokenType.Case )
                {
                    SqlCase caseExpr;
                    if( !MatchCaseExpression( out caseExpr, id ) )
                    {
                        Debug.Assert( R.IsError );
                        return false;
                    }
                    e = caseExpr;
                    return true;
                }
                // This shortcuts the nud/led mechanism by directly handling 
                // the . or the :: as a top precedence level operator.
                ISqlIdentifier identifier;
                if( !IsIdentifier( out identifier, true, id ) ) return false;
                e = identifier;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Combines the LED (LEft Denotation): The token has something at its left (postfix or infix).
        /// </summary>
        bool ExpressionCombineLed( ref ISqlNode left )
        {
            int precedenceLevel = R.CurrentPrecedenceLevel;
            if( R.Current.TokenType == SqlTokenType.OpenPar )
            {
                // This prevents (select a)(select b) multiple statements
                // to be considered as a call.
                if( left is ISelectSpecification ) return false;
                if( left.IsToken( SqlTokenType.Cast ) )
                {
                    SqlTokenOpenPar openPar;
                    ISqlNode e;
                    SqlTokenIdentifier asToken;
                    ISqlUnifiedTypeDecl type;
                    SqlTokenClosePar closePar;
                    if( !R.IsToken( out openPar, true )
                        || !IsOneExpression( out e, true )
                        || !R.IsToken( out asToken, SqlTokenType.As, true )
                        || !IsTypeDecl( out type, true )
                        || !R.IsToken( out closePar, true ) )
                    {
                        return false;
                    }
                    left = new SqlCast( (SqlTokenIdentifier)left, openPar, e, asToken, type, closePar );
                    return true;
                }
                SqlEnclosedCommaList parameters;
                if( !IsEnclosedCommaList( out parameters ) ) return false;
                SqlOverClause over;
                if( !IsOverClause( out over ) && R.IsError ) return false;
                left = new SqlKoCall( left, parameters, over );
                return true;
            }
            if( R.Current.TokenType == SqlTokenType.Collate )
            {
                SqlTokenIdentifier collate = R.Read<SqlTokenIdentifier>();
                SqlTokenIdentifier name;
                if( !R.IsToken( out name, true ) ) return false;
                left = new SqlCollate( left, collate, name );
                return true;
            }
            if( R.Current.TokenType == SqlTokenType.Comma )
            {
                Debug.Assert( !(left is SqlCommaList) );
                var items = new List<ISqlNode>();
                items.Add( left );
                items.Add( R.Read<SqlTokenComma>() );
                for( ;;)
                {
                    ISqlNode next;
                    if( !IsExpression( out next, SqlTokenizer.PrecedenceLevel( SqlTokenType.Comma ), true ) ) return false;
                    items.Add( next );
                    SqlTokenComma comma;
                    if( !R.IsToken( out comma, false ) ) break;
                    items.Add( comma );
                }
                left = new SqlCommaList( items );
                return true;
            }
            if( (R.Current.TokenType & SqlTokenType.IsAssignOperator) != 0 )
            {
                if( !(left is ISqlIdentifier) ) return R.SetCurrentError( "Unexpected '='. Assignment must follow an identifier." );
                else
                {
                    SqlTokenTerminal assign = R.Read<SqlTokenTerminal>();
                    ISqlNode right;
                    using( R.SetAssignmentContext( false ) )
                    {
                        if( !IsExpression( out right, precedenceLevel, true ) ) return false;
                        left = new SqlAssign( (ISqlIdentifier)left, assign, right );
                    }
                }
                return true;
            }
            if( R.Current.TokenType == SqlTokenType.Is )
            {
                SqlTokenIdentifier isToken = R.Read<SqlTokenIdentifier>();
                SqlTokenIdentifier notToken;
                R.IsToken( out notToken, SqlTokenType.Not, false );
                SqlTokenIdentifier nullToken;
                if( !R.IsToken( out nullToken, SqlTokenType.Null, true ) ) return false;
                left = new SqlIsNull( left, isToken, notToken, nullToken );
                return true;
            }
            if( R.Current.TokenType == SqlTokenType.Not )
            {
                SqlTokenIdentifier notToken = R.Read<SqlTokenIdentifier>();
                if( R.Current.TokenType == SqlTokenType.Like ) return IsExprLike( ref left, notToken );
                if( R.Current.TokenType == SqlTokenType.Between ) return IsExprBetween( ref left, notToken );
                if( R.Current.TokenType == SqlTokenType.In ) return IsExprIn( ref left, notToken );
                return R.SetCurrentError( "Expected 'like', 'between' or 'in'." );
            }
            if( R.Current.TokenType == SqlTokenType.Like ) return IsExprLike( ref left, null );
            if( R.Current.TokenType == SqlTokenType.Between ) return IsExprBetween( ref left, null );
            if( R.Current.TokenType == SqlTokenType.In ) return IsExprIn( ref left, null );
            if( SqlBinaryOperator.IsValidBinaryOperator( R.Current.TokenType ) )
            {
                SqlToken cmp = R.Read<SqlToken>();
                ISqlNode right;
                if( !IsExpression( out right, precedenceLevel, true ) ) return false;
                left = new SqlBinaryOperator( left, cmp, right );
                return true;
            }
            if( R.Current.TokenType.IsSelectOperator() )
            {
                ISelectSpecification lSelect = left as ISelectSpecification;
                if( lSelect == null ) return false;
                SqlTokenIdentifier op;
                if( R.Current.TokenType == SqlTokenType.For )
                {
                    // Limits Select For operator to Brows, Xml and JSON.
                    // The other For is for cursor options...
                    if( R.RawLookup.TokenType == SqlTokenType.IdentifierTypeXml || R.RawLookup.IsUnquotedIdentifier( "browse", "json" ) )
                    {
                        op = R.Read<SqlTokenIdentifier>();
                        ISqlNode content;
                        if( !IsExpressionOrNodeList( out content, SelectPartStopper, false, true ) ) return false;
                        left = new SelectFor( lSelect, op, content );
                        return true;
                    }
                    return false;
                }
                op = R.Read<SqlTokenIdentifier>();
                if( op.TokenType == SqlTokenType.Order )
                {
                    SqlTokenIdentifier by;
                    SelectOrderByColumnList columns;
                    if( !R.IsToken( out by, SqlTokenType.By, true ) ) return false;
                    if( !IsSelectOrderByColumnList( out columns ) ) return false;

                    SelectOrderByOffset offsetClause;
                    if( IsSelectOrderByOffset( out offsetClause ) )
                    {
                        left = new SelectOrderBy( lSelect, op, by, columns, offsetClause );
                    }
                    else
                    {
                        if( R.IsError ) return false;
                        left = new SelectOrderBy( lSelect, op, by, columns );
                    }
                    return true;
                }
                SqlTokenIdentifier all = null;
                if( op.TokenType == SqlTokenType.Union ) R.IsToken( out all, SqlTokenType.All, false );
                ISqlNode right;
                if( !IsExpression( out right, precedenceLevel, true ) ) return false;
                ISelectSpecification rSelect = right as ISelectSpecification;
                if( rSelect == null ) return R.SetCurrentError( "Expected select expression." );
                left = new SelectCombineOperator( lSelect, op, all, rSelect );
                return true;
            }
            return false;
        }

        bool IsExprBetween( ref ISqlNode left, SqlTokenIdentifier notToken )
        {
            Debug.Assert( R.Current.TokenType == SqlTokenType.Between );
            SqlTokenIdentifier betweenToken = R.Read<SqlTokenIdentifier>();
            ISqlNode start;
            if( !IsExpression( out start, SqlTokenizer.PrecedenceLevel( SqlTokenType.OpComparisonLevel ), true ) ) return false;
            SqlTokenIdentifier andToken;
            if( !R.IsToken( out andToken, SqlTokenType.And, true ) ) return false;
            ISqlNode stop;
            if( !IsExpression( out stop, SqlTokenizer.PrecedenceLevel( SqlTokenType.OpComparisonLevel ), true ) ) return false;

            left = new SqlBetween( left, notToken, betweenToken, start, andToken, stop );
            return true;
        }

        bool IsExprLike( ref ISqlNode left, SqlTokenIdentifier notToken )
        {
            Debug.Assert( R.Current.TokenType == SqlTokenType.Like );
            SqlTokenIdentifier likeToken = R.Read<SqlTokenIdentifier>();
            ISqlNode pattern;
            if( !IsExpression( out pattern, SqlTokenizer.PrecedenceLevel( SqlTokenType.OpComparisonLevel ), true ) ) return false;
            SqlTokenIdentifier escapeToken;
            SqlTokenLiteralString escapeChar = null;
            if( R.IsToken( out escapeToken, SqlTokenType.Escape, false ) )
            {
                if( !R.IsToken( out escapeChar, true ) ) return false;
            }
            left = new SqlLike( left, notToken, likeToken, pattern, escapeToken, escapeChar );
            return true;
        }

        bool IsExprIn( ref ISqlNode left, SqlTokenIdentifier notToken )
        {
            Debug.Assert( R.Current.TokenType == SqlTokenType.In );
            SqlTokenIdentifier inToken = R.Read<SqlTokenIdentifier>();
            SqlEnclosedCommaList values;
            if( !IsEnclosedCommaList( out values ) ) return false;
            left = new SqlInValues( left, notToken, inToken, values );
            return true;
        }

        bool MatchCaseExpression( out SqlCase e, SqlTokenIdentifier caseToken )
        {
            e = null;
            ISqlNode exprSimple = null;
            SqlTokenIdentifier whenToken;
            if( !R.IsToken( out whenToken, SqlTokenType.When, false ) )
            {
                // Simple case.
                if( !IsExpression( out exprSimple, 0, true ) ) return false;
                if( !R.IsToken( out whenToken, SqlTokenType.When, true ) ) return false;
            }
            Debug.Assert( whenToken != null );
            var whenItems = new List<SqlCaseWhenSelector>();
            do
            {
                ISqlNode expr;
                if( !IsExpression( out expr, 0, true ) ) return false;
                SqlTokenIdentifier thenToken;
                if( !R.IsToken( out thenToken, SqlTokenType.Then, true ) ) return false;
                ISqlNode exprValue;
                if( !IsExpression( out exprValue, 0, true ) ) return false;
                whenItems.Add( new SqlCaseWhenSelector( whenToken, expr, thenToken, exprValue ) );
            }
            while( R.IsToken( out whenToken, SqlTokenType.When, false ) );
            SqlCaseWhenList whenList = new SqlCaseWhenList( whenItems );

            ISqlNode exprElse = null;
            SqlTokenIdentifier elseToken;
            if( R.IsToken( out elseToken, SqlTokenType.Else, false ) )
            {
                if( !IsExpression( out exprElse, 0, true ) ) return false;
            }
            SqlTokenIdentifier endToken;
            if( !R.IsToken( out endToken, SqlTokenType.End, true ) ) return false;

            e = new SqlCase( caseToken, exprSimple, whenList, elseToken, exprElse, endToken );
            return true;
        }

        /// <summary>
        /// Reads one and only one expression (comma stops it).
        /// </summary>
        /// <param name="e">The read expression.</param>
        /// <param name="expected">True to set an error if no expression exists.</param>
        /// <returns>True on success.</returns>
        bool IsOneExpression( out ISqlNode e, bool expected )
        {
            return IsExpression( out e, SqlTokenizer.PrecedenceLevel( SqlTokenType.Comma ), expected );
        }

        /// <summary>
        /// Reads one expression or multiple expressions separated by commas (in a <see cref="SqlCommaList"/>).
        /// </summary>
        /// <param name="e">The read expression.</param>
        /// <param name="expected">True to set an error if no expression exist.</param>
        /// <returns>True on success.</returns>
        bool IsMultiExpression( out ISqlNode e, bool expected )
        {
            return IsExpression( out e, 0, expected );
        }

        ///// <summary>
        ///// Collects tokens in an <see cref="SqlNodeList"/> until a given token is found.
        ///// </summary>
        ///// <typeparam name="T">Type of the stopper token.</typeparam>
        ///// <param name="items">An unmodeled list of nodes. Null if the stopper occurs immediately or an error occurred on the first token.</param>
        ///// <param name="stopper">Stopper eventually found. Null if the end of input or an error has been encountered.</param>
        ///// <param name="stopperDefinition">Predicate that defines the stop.</param>
        ///// <param name="eaters">
        ///// Optional functions that can transform the current token (and its followers) to any node. 
        ///// Matchers are called up to the first one that returns an item different than the Current token.
        ///// When a matcher returns null, the current token is ignored.
        ///// </param>
        ///// <returns>True if no error occurred. The stopper is null if the end of input has been encountered.</returns>
        //bool IsNodeList<T>( out SqlNodeList e, out T stopper, Predicate<T> stopperDefinition, bool atLeastOne, params Func<ISqlNode>[] eaters ) where T : SqlToken
        //{
        //    e = null;
        //    List<ISqlNode> nodes;
        //    if( !R.IsItemList( out nodes, out stopper, stopperDefinition, atLeastOne, eaters.Append( Eater<ISqlNode>( IsOneExpression ) ) ) ) return false;
        //    e = new SqlNodeList( nodes );
        //    return true;
        //}

        bool IsSqlNodeList<T>( out SqlNodeList e, out T stopper, Predicate<T> stopperDefinition = null, bool atLeastOne = false, IsFunc<ISqlNode> matcher = null ) where T : SqlToken
        {
            e = null;
            List<ISqlNode> items = new List<ISqlNode>();
            if( !R.CollectUntil( items, matcher, out stopper, stopperDefinition ) ) return false;
            if( atLeastOne && items.Count == 0 ) return R.SetCurrentError( "Expected at least one item." );
            e = new SqlNodeList( items );
            return true;
        }

        /// <summary>
        /// Reads a comma separated list of expressions (that can be unmodeled <see cref="SqlNodeList"/>).
        /// </summary>
        /// <param name="e">The list.</param>
        /// <returns>True on success.</returns>
        bool IsEnclosedCommaList( out SqlEnclosedCommaList e )
        {
            e = null;
            SqlTokenOpenPar openPar;
            SqlTokenClosePar closePar;
            List<ISqlNode> items;
            if( !IsCommaList<ISqlNode>( out openPar, out items, out closePar, true, MatchInList ) ) return false;
            e = new SqlEnclosedCommaList( openPar, items, closePar );
            return true;
        }

        bool MatchInList( out ISqlNode e, bool expected )
        {
            return IsExpressionOrNodeList( out e, ISqlItemExtension.IsCommaOrCloseParenthesisOrTerminator, false, expected );
        }

        /// <summary>
        /// Reads an expression or a <see cref="SqlNodeList"/> up to a specific token.
        /// </summary>
        /// <param name="e">Read expression.</param>
        /// <param name="closer">Predicate that detects the stopper (will NOT be added to the expression).</param>
        /// <param name="blindlyAcceptCurrentToken">True to accept the current token even if it satisfies the stopper predicate.</param>
        /// <param name="expectAtLeastOne">True to set an error if no expression nor node has been read.</param>
        /// <returns>True if an expression has successfully been found (it may be a <see cref="SqlNodeList"/>).</returns>
        bool IsExpressionOrNodeList( out ISqlNode e, Predicate<SqlToken> stopper, bool blindlyAcceptCurrentToken, bool expectAtLeastOne )
        {
            if( stopper == null ) throw new ArgumentNullException( "stopper" );
            return IsExpressionOrParOrNodeListInternal( out e, null, stopper, blindlyAcceptCurrentToken, expectAtLeastOne );
        }

        bool IsExpressionOrParOrNodeListInternal( out ISqlNode e, SqlTokenOpenPar openPar, Predicate<SqlToken> closer, bool blindlyAcceptCurrentToken, bool setErrorIfEmpty )
        {
            Debug.Assert( openPar == null || closer( SqlTokenTerminal.ClosePar ), "If we have an open parenthesis, the closer function must detect a closing parenthesis." );
            e = null;
            List<ISqlNode> exprs = new List<ISqlNode>();
            ISqlNode lastExpr = null;
            while( blindlyAcceptCurrentToken || !(R.IsErrorOrEndOfInput || closer( R.Current )) )
            {
                blindlyAcceptCurrentToken = false;
                // If it is not the closer nor the end, it may be a valid expression.
                if( IsExpression( out lastExpr, SqlTokenizer.PrecedenceLevel( SqlTokenType.Comma ), expected: false ) )
                {
                    exprs.Add( lastExpr );
                }
                else
                {
                    if( R.IsErrorOrEndOfInput ) break;
                    exprs.Add( R.Read<SqlToken>() );
                }
            }
            // If we expect something and nothing was found and no error was previously set, we set an error.
            if( setErrorIfEmpty && exprs.Count == 0 && !R.IsError ) return R.SetCurrentError( "Expected expression." );
            // If no error occurred, the block is built:
            // - if the opener is not null, with the the given opener and the found closer.
            // - if the opener is null, without any opener/closer and the closer is not consumed.
            if( !R.IsError )
            {
                Debug.Assert( closer( R.Current ) || R.IsEndOfInput, "We are on the Closer token or at the end." );
                if( openPar != null )
                {
                    // If an opener exists, we always create a SqlPar.
                    if( R.Current.TokenType == SqlTokenType.ClosePar )
                    {
                        SqlTokenClosePar closePar = R.Read<SqlTokenClosePar>();
                        e = exprs.Count == 1 
                            ? new SqlPar( openPar, lastExpr, closePar )
                            : new SqlPar( openPar, new SqlNodeList( exprs ), closePar );
                        return true;
                    }
                    else return R.SetCurrentError( "Expected ')'." );
                }
                // When no opener/closer exist and the block is empty, we do not instanciate it.
                if( exprs.Count > 0 )
                {
                    if( exprs.Count == 1 ) e = lastExpr;
                    else e = new SqlNodeList( exprs );
                }
                return true;
            }
            // An error occurred: closer was not found.
            // We let the block null... (we may here build a block with exprs and a kind of SqlExprSyntaxError at the end).
            return false;
        }

    }


}

