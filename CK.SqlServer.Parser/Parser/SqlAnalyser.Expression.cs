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
            Debug.Assert( SqlTokenizer.PrecedenceLevel( SqlTokenType.OpNotRightLevel ) == 10 );
            while( !R.IsErrorOrEndOfInput
                    && ((R.Current.TokenType == SqlTokenType.Not && rightBindingPower < 10)
                        ||
                        (R.Current.TokenType != SqlTokenType.Not && SqlTokenizer.PrecedenceLevel( R.Current.TokenType ) > rightBindingPower)
                        ||
                        // Ugly hack: AT TIME must act as a highly prioritized operator.
                        // Since AT is not a reseved keyword, it is a valid comumn name (the collate operator is keyword):
                        //
                        //      select name at from sys.tables; -- OK
                        //      select name collate from sys.tables; -- !Invalid syntax.
                        //
                        (R.Current.TokenType == SqlTokenType.At && R.RawLookup.TokenType == SqlTokenType.TimeDbType) ) )
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
                SqlToken op = R.Read<SqlToken>();
                ISqlNode right;
                if( (right = IsExpression( SqlTokenizer.PrecedenceLevel( op.TokenType ), true )) == null ) return null;
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
                SqlTokenOpenPar opener = R.Read<SqlTokenOpenPar>();
                var content = IsAnyExpression( true );
                if( content == null ) return null;
                SqlTokenClosePar closer;
                if( !R.IsToken( out closer, true ) ) return null;
                // Lifts a SqlEnclosable that is not enclosed instead of a (Encloseable).
                SqlEnclosableCommaList enclosed = content as SqlEnclosableCommaList;
                if( enclosed != null && !enclosed.IsEnclosed )
                {
                    return enclosed.Enclose( opener, closer );
                }
                return new SqlPar( opener, content, closer );
            }
            ISqlNode stdTypeOrCharCall = IsSqlDbTypeDecl( true );
            if( stdTypeOrCharCall != null ) return stdTypeOrCharCall;
            if( (R.Current.TokenType & SqlTokenType.IsIdentifier) != 0 )
            {
                SqlTokenIdentifier id = R.Read<SqlTokenIdentifier>();
                if( id.TokenType == SqlTokenType.Select )
                {
                    return MatchSelectSpecification( id );
                }
                if( id.TokenType == SqlTokenType.Insert )
                {
                    return MatchInsertStatement( id );
                }
                if( id.TokenType == SqlTokenType.Merge )
                {
                    return MatchMergeStatement( id );
                }
                if( id.TokenType == SqlTokenType.Delete )
                {
                    return MatchUpdateOrDeleteStatement( id );
                }
                if( id.TokenType == SqlTokenType.Update )
                {
                    // This handles the "if Update()" in triggers: this will be a KoCall.
                    if( R.Current.TokenType == SqlTokenType.OpenPar ) return id;
                    return MatchUpdateOrDeleteStatement( id );
                }
                if( id.TokenType == SqlTokenType.Case )
                {
                    return MatchCaseExpression( id );
                }
                if( id.TokenType == SqlTokenType.Next )
                {
                    SqlTokenIdentifier valueT;
                    if( R.IsToken( out valueT, SqlTokenType.Value, false ) )
                    {
                        SqlTokenIdentifier forT;
                        if( !R.IsToken( out forT, SqlTokenType.For, true ) ) return null;
                        ISqlIdentifier seqName = IsIdentifier( true );
                        if( seqName == null ) return null;
                        return new SqlNextValueFor( id, valueT, forT, seqName );
                    }
                }
                if( id.TokenType == SqlTokenType.Values )
                {
                    using( R.OpenParenthesis() )
                    {
                        SqlMultiCommaList values = IsCommaList( 0, IsEnclosedCommaList, i => new SqlMultiCommaList( i ) );
                        if( values == null ) return null;
                        if( values.Count > 0 ) return new SqlTableValues( id, values );
                    }
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
            int precedenceLevel = SqlTokenizer.PrecedenceLevel( R.Current.TokenType );
            if( R.Current.TokenType == SqlTokenType.Comma )
            {
                Debug.Assert( !(left is SqlEnclosableCommaList) );
                List<ISqlNode> items = new List<ISqlNode>();
                items.Add( left );
                SqlTokenComma comma;
                while( R.IsToken( out comma, false ) )
                {
                    items.Add( comma );
                    ISqlNode i = IsExtendedExpression( true );
                    if( i == null ) return false;
                    items.Add( i );
                }
                left = new SqlEnclosableCommaList( null, items, null );
                return true;
            }
            if( R.Current.TokenType == SqlTokenType.OpenPar )
            {
                if( !(left is ISqlIdentifier) ) return false;
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
                SqlEnclosedCommaList parameters = IsEnclosedCommaList( true );
                if( parameters == null ) return false;
                if( left.IsToken( SqlTokenType.OpenJSON ) )
                {
                    SqlWithParOptions options = IsIdentifierPrefixedCommaList( false, SqlTokenType.With, 1, IsExtendedExpression, ( p, o, i, c ) => new SqlWithParOptions( p, o, i, c ) );
                    if( R.IsError ) return false;
                    left = new SqlOpenJSON( (SqlTokenIdentifier)left, parameters, options );
                    return true;
                }
                if( left.IsToken( SqlTokenType.OpenXml ) )
                {
                    SqlTokenIdentifier withT;
                    ISqlIdentifier schemaTable = null;
                    SqlEnclosedCommaList schemaDefintion = null;
                    if( R.IsToken( out withT, SqlTokenType.With, false ) )
                    {
                        if( (schemaTable = IsIdentifier( false )) == null 
                            && (schemaDefintion = IsEnclosedCommaList( true ) ) == null )
                        {
                            return false;
                        }
                    }
                    left = new SqlOpenXml( (SqlTokenIdentifier)left, parameters, withT, schemaDefintion, schemaTable );
                    return true;
                }
                SqlWithinGroup withinGroup = IsWithinGroup( false );
                if( R.IsError ) return false;
                SqlOverClause over = IsOverClause( false );
                if( R.IsError ) return false;
                left = new SqlKoCall( left, parameters, withinGroup, over );
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
            if( R.Current.TokenType == SqlTokenType.At )
            {
                SqlTokenIdentifier atT = R.Read<SqlTokenIdentifier>();
                SqlTokenIdentifier timeT, zoneT;
                if( !R.IsToken( out timeT, SqlTokenType.TimeDbType, true )
                    || !R.IsToken( out zoneT, SqlTokenType.Zone, true ) ) return false;
                ISqlNode timeZone;
                if( (timeZone = IsExpression( precedenceLevel, true )) == null ) return false;
                left = new SqlAtTimeZone( left, atT, timeT, zoneT, timeZone );
                return true;
            }
            if( (R.Current.TokenType & SqlTokenType.IsAssignOperator) != 0 )
            {
                SqlTokenTerminal assign = R.Read<SqlTokenTerminal>();
                ISqlNode right;
                using( R.SetAssignmentContext( false ) )
                {
                    if( (right = IsExpression( precedenceLevel, true )) == null ) return false;
                    left = new SqlAssign( left, assign, right );
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
                // Allows not XXX without complaining ("not null", "not matched", etc.).
                // Not will be handled as a NUD (and should generate a Unary operator).
                if( R.RawLookup.TokenType == SqlTokenType.Like 
                    || R.RawLookup.TokenType == SqlTokenType.Between 
                    || R.RawLookup.TokenType == SqlTokenType.In )
                {
                    SqlTokenIdentifier notToken = R.Read<SqlTokenIdentifier>();
                    if( R.Current.TokenType == SqlTokenType.Like ) return IsExprLike( ref left, notToken );
                    if( R.Current.TokenType == SqlTokenType.Between ) return IsExprBetween( ref left, notToken );
                    if( R.Current.TokenType == SqlTokenType.In ) return IsExprIn( ref left, notToken );
                }
                return false;
            }
            if( R.Current.TokenType == SqlTokenType.Like ) return IsExprLike( ref left, null );
            if( R.Current.TokenType == SqlTokenType.Between ) return IsExprBetween( ref left, null );
            if( R.Current.TokenType == SqlTokenType.In ) return IsExprIn( ref left, null );
            if( SqlBinaryOperator.IsValidBinaryOperator( R.Current.TokenType ) )
            {
                SqlToken cmp = R.Read<SqlToken>();
                ISqlNode right;
                if( (right = IsExpression( precedenceLevel, true )) == null ) return false;
                left = new SqlBinaryOperator( left, cmp, right );
                return true;
            }
            if( R.Current.TokenType.IsSelectOperator() )
            {
                ISelectSpecification lSelect = left.UnPar as ISelectSpecification;
                if( lSelect == null 
                    || (R.Current.TokenType == SqlTokenType.For && !R.RawLookup.TokenType.IsSelectForTargetType() ) )
                {
                    return false;
                }
                if( R.Current.TokenType == SqlTokenType.Order
                    || R.Current.TokenType == SqlTokenType.For
                    || R.Current.TokenType == SqlTokenType.Option )
                {
                    SelectOrderBy orderBy;
                    SelectFor forClause;
                    SqlOptionParOptions option;
                    orderBy = IsSelectOrderBy( false );
                    if( R.IsError ) return false;
                    forClause = IsSelectFor( false );
                    if( R.IsError ) return false;
                    option = IsIdentifierPrefixedCommaList( false, SqlTokenType.Option, 1, IsExtendedExpression, ( p, o, i, c ) => new SqlOptionParOptions( p, o, i, c ) );
                    left = new SelectDecorator( left, orderBy, forClause, option );
                    return true;
                }
                SqlTokenIdentifier op = R.Read<SqlTokenIdentifier>();
                SqlTokenIdentifier all = null;
                if( op.TokenType == SqlTokenType.Union ) R.IsToken( out all, SqlTokenType.All, false );
                ISqlNode right;
                if( (right = IsExpression( precedenceLevel, true )) == null ) return false;
                ISelectSpecification rSelect = right.UnPar as ISelectSpecification;
                if( rSelect == null ) return R.SetCurrentError( "Expected select expression." );
                left = new SelectCombine( left, op, all, right );
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
            if( (pattern = IsExpression( SqlTokenizer.PrecedenceLevel( SqlTokenType.OpComparisonLevel ), true )) == null ) return false;
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
            SqlEnclosedCommaList values = IsEnclosedCommaList( true );
            if( values == null ) return false;
            left = new SqlInValues( left, notToken, inToken, values );
            return true;
        }

        SqlCase MatchCaseExpression( SqlTokenIdentifier caseToken )
        {
            ISqlNode exprSimple = null;
            SqlTokenIdentifier whenToken;
            if( !R.IsToken( out whenToken, SqlTokenType.When, false ) )
            {
                // Simple case.
                if( (exprSimple = IsOneExpression( true )) == null ) return null;
                if( !R.IsToken( out whenToken, SqlTokenType.When, true ) ) return null;
            }
            Debug.Assert( whenToken != null );
            var whenItems = new List<SqlCaseWhenSelector>();
            do
            {
                ISqlNode expr;
                if( (expr = IsOneExpression( true )) == null ) return null;
                SqlTokenIdentifier thenToken;
                if( !R.IsToken( out thenToken, SqlTokenType.Then, true ) ) return null;
                ISqlNode exprValue;
                if( (exprValue = IsOneExpression( true )) == null ) return null;
                whenItems.Add( new SqlCaseWhenSelector( whenToken, expr, thenToken, exprValue ) );
            }
            while( R.IsToken( out whenToken, SqlTokenType.When, false ) );
            SqlCaseWhenList whenList = new SqlCaseWhenList( whenItems );

            ISqlNode exprElse = null;
            SqlTokenIdentifier elseToken;
            if( R.IsToken( out elseToken, SqlTokenType.Else, false ) )
            {
                if( (exprElse = IsOneExpression( true )) == null ) return null;
            }
            SqlTokenIdentifier endToken;
            if( !R.IsToken( out endToken, SqlTokenType.End, true ) ) return null;

            return new SqlCase( caseToken, exprSimple, whenList, elseToken, exprElse, endToken );
        }


        /// <summary>
        /// Reads one and only one expression (comma stops it).
        /// </summary>
        /// <param name="expected">True to set an error if no expression exist.</param>
        /// <returns>One expression or null.</returns>
        public ISqlNode IsOneExpression( bool expected )
        {
            return IsExpression( SqlTokenizer.PrecedenceLevel( SqlTokenType.Comma ), expected );
        }

        public ISqlNode IsExtendedExpression( bool expected )
        {
            return IsExtendedExpression( expected, true );
        }

        /// <summary>
        /// An extended expression is one expression or a list (a <see cref="SqlNodeList"/>) of 
        /// tokens or expressions.
        /// A comma or a closing parenthesis stops this.
        /// </summary>
        /// <param name="expected">True to set an error if no expression exist.</param>
        /// <param name="allowLeadingStatement">True to accept a first statement.</param>
        /// <returns>One expression, a <see cref="SqlNodeList"/> or null.</returns>
        public ISqlNode IsExtendedExpression( bool expected, bool allowLeadingStatement )
        {
            bool stopOnStatement = !allowLeadingStatement;  //!expected && R.ParenthesisDepth == 0;
            List<ISqlNode> items = new List<ISqlNode>();
            while( !R.IsErrorOrEndOfInput 
                && !SqlToken.IsEndOfExtendedExpression( R.Current )
                && !(stopOnStatement && SqlToken.IsLimitedStatementStopper( R.Current )) )
            {
                ISqlNode item = IsOneExpression( expected );
                stopOnStatement = R.ParenthesisDepth == 0;
                expected = false;
                if( item != null ) items.Add( item );
                else
                {
                    if( R.IsError ) break;
                    items.Add( R.Current );
                    R.MoveNext();
                }
            }
            if( items.Count == 0 )
            {
                if( expected ) R.SetCurrentError( "Expected Extended Expression." );
                return null;
            }
            return items.Count == 1 ? items[0] : new SqlNodeList( items );
        }

        /// <summary>
        /// Any expression can be an extended expression or a comma separated list of 
        /// extended expression optionally enclosed in parenthesis (<see cref="SqlEnclosableCommaList"/>).
        /// </summary>
        /// <param name="expected">True to set an error if no expression exist.</param>
        /// <param name="allowLeadingStatement">True to accept the first statement, false to not consider it.</param>
        /// <returns>Any expression or null.</returns>
        ISqlNode IsAnyExpression( bool expected, bool allowLeadingStatement )
        {
            ISqlNode item = IsExtendedExpression( expected, allowLeadingStatement );
            if( item == null ) return null;
            SqlTokenComma comma;
            if( R.IsToken( out comma, false ) )
            {
                List<ISqlNode> items = new List<ISqlNode>();
                items.Add( item );
                do
                {
                    items.Add( comma );
                    if( (item = IsExtendedExpression( true )) == null )
                    {
                        if( !R.IsError ) R.SetCurrentError( "Expected extended expression." );
                        return null;
                    }
                    items.Add( item );
                }
                while( R.IsToken( out comma, false ) );
                return new SqlEnclosableCommaList( null, items, null );
            }
            return item;
        }

        /// <summary>
        /// Any expression can be an extended expression or a comma separated list of 
        /// extended expression optionally enclosed in parenthesis (<see cref="SqlEnclosableCommaList"/>).
        /// </summary>
        /// <param name="expected">True to set an error if no expression exist.</param>
        /// <returns>Any expression or null.</returns>
        ISqlNode IsAnyExpression( bool expected )
        {
            return IsAnyExpression( expected, true );
        }

        //=========================================
        //=========================================
        //=========================================
        //=========================================
        //=========================================

        ISqlNode InternalParseNaouakInSelect()
        {
            List<ISqlNode> items = new List<ISqlNode>();
            int initPar = R.ParenthesisDepth;
            Predicate<SqlToken> stop = t => R.ParenthesisDepth == initPar && SelectPartStopper( t );
            if( !R.CollectUntil( items, NaouakPart, stop ) ) return null;
            if( items.Count == 0 )
            {
                R.SetCurrentError( "Extended expression expected." );
                return null;
            }
            return items.Count == 1 ? items[0] : new SqlNodeList( items );
        }

        ISqlNode NaouakPart( bool expected )
        {
            Debug.Assert( !expected );
            ISqlNode n = IsOneExpression( false );
            if( n == null ) return n;
            if( R.Current.TokenType == SqlTokenType.For && R.RawLookup.TokenType == SqlTokenType.SystemTime )
            {
                return new SqlNodeList( n, R.Read<SqlToken>() );
            }
            return n;
        }

    }


}

