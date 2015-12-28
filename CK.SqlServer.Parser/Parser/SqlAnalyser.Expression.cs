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
            if( R.IsErrorOrEndOfInput || (e = IsExpressionNud()) == null )
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

        ISqlNode IsExpression( int rightBindingPower, bool expected )
        {
            ISqlNode e = null;
            if( R.IsErrorOrEndOfInput || (e = IsExpressionNud()) == null )
            {
                if( expected ) R.SetCurrentError( "Expected expression." );
                return null;
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
            return R.IsError ? null : e;
        }

        /// <summary>
        /// Handles NUD (NUll left Denotation): the token has nothing to its left (it is a prefix).
        /// </summary>
        /// <returns>The expression on success, otherwise null.</returns>
        ISqlNode IsExpressionNud()
        {
            Debug.Assert( !R.IsErrorOrEndOfInput );
            // Handles strings and numbers.
            if( (R.Current.TokenType & SqlTokenType.LitteralMask) != 0 )
            {
                return R.Read<SqlTokenBaseLiteral>();
            }
            if( R.Current.TokenType == SqlTokenType.Minus
                || R.Current.TokenType == SqlTokenType.Plus
                || R.Current.TokenType == SqlTokenType.BitwiseNot
                || R.Current.TokenType == SqlTokenType.Not )
            {
                int precedenceLevel = R.CurrentPrecedenceLevel;
                SqlToken op = R.Read<SqlToken>();
                ISqlNode right;
                if( !IsExpression( out right, precedenceLevel, true ) ) return null;
                return new SqlUnaryOperator( op, right );
            }
            if( R.Current.TokenType == SqlTokenType.Mult )
            {
                // Handles a Nud * as a identifier.
                // Actual syntax prohibits *.part or *::part but for
                // coherency, we consider this as a valid construct.
                var star = R.Read<SqlToken>();
                var starT = new SqlTokenIdentifier( SqlTokenType.IdentifierStar, "*", star.LeadingTrivias, star.TrailingTrivias );
                return IsIdentifier( true, starT );
            }
            if( R.Current.TokenType == SqlTokenType.OpenPar )
            {
                return IsAnyExpression( true );
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
                        return null;
                    }
                    return select;
                }
                if( id.TokenType == SqlTokenType.Case )
                {
                    SqlCase caseExpr;
                    if( !MatchCaseExpression( out caseExpr, id ) )
                    {
                        Debug.Assert( R.IsError );
                        return null;
                    }
                    return caseExpr;
                }
                // This shortcuts the nud/led mechanism by directly handling 
                // the . or the :: as a top precedence level operator.
                return IsIdentifier( true, id );
            }
            return null;
        }

        /// <summary>
        /// Combines the LED (LEft Denotation): The token has something at its left (postfix or infix).
        /// </summary>
        bool ExpressionCombineLed( ref ISqlNode left )
        {
            Debug.Assert( R.Current.TokenType != SqlTokenType.Comma, "Comma is not an operator." );
            int precedenceLevel = R.CurrentPrecedenceLevel;
            if( R.Current.TokenType == SqlTokenType.OpenPar )
            {
                // This prevents (select a)(select b) multiple statements
                // to be considered as a call.
                if( left.UnPar is ISelectSpecification ) return false;
                if( left.IsToken( SqlTokenType.Cast ) )
                {
                    SqlTokenOpenPar openPar;
                    ISqlNode e;
                    SqlTokenIdentifier asToken;
                    ISqlUnifiedTypeDecl type;
                    SqlTokenClosePar closePar;
                    if( !R.IsToken( out openPar, true )
                        || (e = IsOneExpression( true )) == null
                        || !R.IsToken( out asToken, SqlTokenType.As, true )
                        || (type = IsTypeDecl( true )) == null
                        || !R.IsToken( out closePar, true ) )
                    {
                        return false;
                    }
                    left = new SqlCast( (SqlTokenIdentifier)left, openPar, e, asToken, type, closePar );
                    return true;
                }
                SqlEnclosedCommaList parameters = IsEnclosedCommaList( true, Parenthesis.Required );
                if( parameters == null ) return false;
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
                ISelectSpecification lSelect = left.UnPar as ISelectSpecification;
                if( lSelect == null ) return false;
                SqlTokenIdentifier op;
                if( R.Current.TokenType == SqlTokenType.For )
                {
                    // Limits Select For operator to 'Browse', 'Xml' and 'JSON'.
                    // The other For is for cursor options...
                    if( R.RawLookup.TokenType == SqlTokenType.IdentifierTypeXml
                        || R.RawLookup.TokenType == SqlTokenType.Browse
                        || R.RawLookup.TokenType == SqlTokenType.Json )

                    {
                        op = R.Read<SqlTokenIdentifier>();
                        ISqlNode content = InternalIsExtendedExpression( true, SelectPartStopper );
                        if( content == null ) return false;
                        left = new SelectFor( lSelect, op, content );
                        return true;
                    }
                    return false;
                }
                op = R.Read<SqlTokenIdentifier>();
                if( op.TokenType == SqlTokenType.Order )
                {
                    SqlTokenIdentifier by;
                    SqlOrderByList columns;
                    if( !R.IsToken( out by, SqlTokenType.By, true ) ) return false;
                    columns = IsOrderByList();
                    if( columns == null ) return false;

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
            ISqlNode start = IsExpression( SqlTokenizer.PrecedenceLevel( SqlTokenType.OpComparisonLevel ), true );
            if( start == null ) return false;
            SqlTokenIdentifier andToken;
            if( !R.IsToken( out andToken, SqlTokenType.And, true ) ) return false;
            ISqlNode stop = IsExpression( SqlTokenizer.PrecedenceLevel( SqlTokenType.OpComparisonLevel ), true );
            if( stop == null ) return false;
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
            SqlEnclosedCommaList values = IsEnclosedCommaList( true, Parenthesis.Required );
            if( values == null ) return false;
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
        /// <param name="expected">True to set an error if no expression exist.</param>
        /// <returns>One expression or null.</returns>
        public ISqlNode IsOneExpression( bool expected )
        {
            return IsExpression( 0, expected );
        }

        /// <summary>
        /// An extended expression is one expression or a list (a <see cref="SqlNodeList"/>) of 
        /// tokens or expressions.
        /// A comma or a closing parenthesis stops this.
        /// </summary>
        /// <param name="expected">True to set an error if no expression exist.</param>
        /// <returns>One expression, a <see cref="SqlNodeList"/> or null.</returns>
        public ISqlNode IsExtendedExpression( bool expected )
        {
            return InternalIsExtendedExpression( expected, SqlToken.IsCommaOrCloseParenthesis );
        }

        /// <summary>
        /// An extended expression for statement is one expression or a list (a <see cref="SqlNodeList"/>) of 
        /// tokens or expressions.
        /// A comma, the statement terminator or a possible start statement stops this.
        /// </summary>
        /// <param name="expected">True to set an error if no expression exist.</param>
        /// <returns>One expression or a <see cref="SqlNodeList"/> or null.</returns>
        public ISqlNode IsExtendedExpressionForStatement( bool expected )
        {
            return InternalIsExtendedExpression( expected, SqlToken.IsCommaOrTerminatorOrPossibleStartStatement );
        }

        ISqlNode InternalIsExtendedExpression( bool expected, Predicate<SqlToken> stopperDefinition )
        {
            List<ISqlNode> items = new List<ISqlNode>();
            if( !R.CollectUntil( items, IsOneExpression, stopperDefinition ) ) return null;
            if( items.Count == 0 )
            {
                if( expected ) R.SetCurrentError( "Extended expression expected." );
                return null;
            }
            return items.Count == 1 ? items[0] : new SqlNodeList( items );
        }

        /// <summary>
        /// Any expression can be an extended expression or a comma separated list of 
        /// extended expression.
        /// </summary>
        /// <param name="expected">True to set an error if no expression exist.</param>
        /// <returns>One expression or a <see cref="SqlNodeList"/> or null.</returns>
        ISqlNode IsAnyExpression( bool expected )
        {
            return InternalIsAnyExpression( expected, IsExtendedExpression );
        }

        /// <summary>
        /// Any expression for statement can be an extended expression for statement or a 
        /// comma separated list of extended expression for statement.
        /// </summary>
        /// <param name="expected">True to set an error if no expression exist.</param>
        /// <returns>One expression or a <see cref="SqlNodeList"/> or null.</returns>
        ISqlNode IsAnyExpressionForStatement( bool expected )
        {
            return InternalIsAnyExpression( expected, IsExtendedExpressionForStatement );
        }

        ISqlNode InternalIsAnyExpression( bool expected, Func<bool,ISqlNode> matcher )
        {
            SqlTokenOpenPar openPar;
            SqlTokenClosePar closePar;
            List<ISqlNode> items = new List<ISqlNode>();
            if( !R.CollectCommaList( items, out openPar, out closePar, matcher ) ) return null;
            if( openPar == null && items.Count == 0 )
            {
                if( expected ) R.SetCurrentError( "Expected '{0}'.", matcher.Method.Name );
                return null;
            }
            ISqlNode e;
            if( items.Count == 0 ) e = SqlNodeList.Empty;
            else if( items.Count == 1 ) e = items[0];
            else e = new SqlCommaList( items );
            return openPar != null ? new SqlPar( openPar, e, closePar ) : e;
        }

    }


}

