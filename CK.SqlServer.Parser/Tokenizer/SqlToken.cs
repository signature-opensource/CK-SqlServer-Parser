using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using CK.Core;
using System.Diagnostics;
using System.Globalization;
using System.Collections.Immutable;
using System.Collections;

namespace CK.SqlServer.Parser
{
    /// <summary>
    /// Base class for (non comment) tokens. 
    /// </summary>
    public abstract class SqlToken : SqlNode, IEnumerable<SqlToken>
    {
        /// <summary>
        /// Private empty ctor for the EmptyToken.
        /// </summary>
        SqlToken( ImmutableList<SqlTrivia> leading, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            Debug.Assert( TokenType == SqlTokenType.None );
        }

        /// <summary>
        /// Initializes a new <see cref="SqlToken"/>. <paramref name="tokenType"/> must be strictly positive (not an error) and not <see cref="SqlTokenType.IsComment"/>.
        /// When null, trivias are safely sets to an empty readonly list of <see cref="SqlTrivia"/>.
        /// </summary>
        /// <param name="tokenType">Type of the token.</param>
        /// <param name="leading">Leading trivias if any.</param>
        /// <param name="trailing">Trailing trivias if any.</param>
        public SqlToken( SqlTokenType tokenType, ImmutableList<SqlTrivia> leading = null, ImmutableList<SqlTrivia> trailing = null )
            : base( leading, trailing )
        {
            if( tokenType > 0 && ((tokenType & SqlTokenType.TokenDiscriminatorMask) == 0 || (tokenType&SqlTokenType.IsComment) !=0) ) throw new ArgumentException( "Invalid token type." );
            TokenType = tokenType;
            SqlKeyword.CheckTokenTypeStringMapping( tokenType );
        }

        /// <summary>
        /// Token type. It is necessarily positive (not an error). Only <see cref="Empty"/> has <see cref="SqlTokenType.None"/> type.
        /// </summary>
        public readonly SqlTokenType TokenType;

        public override sealed IEnumerable<SqlTrivia> FullLeadingTrivias => LeadingTrivias;

        public override sealed IEnumerable<SqlTrivia> FullTrailingTrivias => TrailingTrivias;

        public override sealed IEnumerable<ISqlNode> LeadingNodes => Util.Array.Empty<ISqlNode>();

        public override sealed IEnumerable<ISqlNode> TrailingNodes => Util.Array.Empty<ISqlNode>();

        /// <summary>
        /// Gets always 1: the width of a token.
        /// </summary>
        public override sealed int Width => 1;

        /// <summary>
        /// Tests token value equality: the reference equality still applies to tokens.
        /// </summary>
        /// <param name="t">Token to compare to.</param>
        /// <returns>True if the this token is equal to the other one in terms of value.</returns>
        public abstract bool TokenEquals( SqlToken t );

        /// <summary>
        /// Gets an empty node list.
        /// </summary>
        public override sealed IReadOnlyList<ISqlNode> ChildrenNodes => Util.Array.Empty<ISqlNode>();

        public override sealed IList<ISqlNode> GetRawContent() => Util.Array.Empty<ISqlNode>();

        public override sealed bool IsToken( SqlTokenType t ) => TokenType == t;

        #region IEnumerable<SqlToken> AllTokens auto implementation
        public override sealed IEnumerable<SqlToken> AllTokens => this;

        IEnumerator<SqlToken> IEnumerable<SqlToken>.GetEnumerator() => new CKEnumeratorMono<SqlToken>( this );

        IEnumerator IEnumerable.GetEnumerator() => new CKEnumeratorMono<SqlToken>( this );

        #endregion

        /// <summary>
        /// True if the <see cref="SqlToken"/> is the terminator statement ';' or the end of input.
        /// </summary>
        /// <param name="t">Token to test.</param>
        /// <returns>Whether the token is the statement terminator or the end of imput.</returns>
        static public bool IsTerminatorOrEndOfInput( SqlToken t )
        {
            Debug.Assert( t != null );
            return t.TokenType == SqlTokenType.EndOfInput || t.TokenType == SqlTokenType.SemiColon;
        }

        /// <summary>
        /// True if the <see cref="SqlToken"/> is an open parenthesis or an 
        /// identifier that starts a statement (<see cref="SqlTokenTypeExtension.IsStartStatement(SqlTokenType)"/>.
        /// </summary>
        /// <param name="t">Token to test.</param>
        /// <returns>Whether the token is a possible start of a new statement.</returns>
        static public bool IsLimitedStatementStopper( SqlToken t )
        {
            if( t == null ) throw new ArgumentNullException( "t" );
            return t.TokenType == SqlTokenType.OpenPar
                    || t.TokenType.IsStartStatement();
        }

        /// <summary>
        /// True if the <see cref="SqlToken"/> is a <see cref="IsEndOfExtendedExpression"/>
        /// or a <see cref="IsLimitedStatementStopper"/>.
        /// </summary>
        /// <param name="t">Token to test.</param>
        /// <returns>Whether the token is a possible start of a new statement.</returns>
        static public bool IsStatementStopper( SqlToken t )
        {
            return IsEndOfExtendedExpression( t ) || IsLimitedStatementStopper( t );
        }

        /// <summary>
        /// True if the <see cref="SqlToken"/> is a closing parenthesis, a terminator ; token or a <see cref="SqlTokenType.IdentifierReservedStatement"/>.
        /// </summary>
        /// <param name="t">Token to test.</param>
        /// <returns>Whether the token is closing parenthesis or the statement terminator.</returns>
        static public bool IsCloseParenthesisOrTerminatorOrPossibleStartStatement( SqlToken t )
        {
            if( t == null ) throw new ArgumentNullException( "t" );
            return t.TokenType == SqlTokenType.ClosePar
                    || t.TokenType == SqlTokenType.SemiColon
                    || t.TokenType.IsStartStatement();
        }

        /// <summary>
        /// True if the <see cref="SqlToken"/> is the end of the input, a comma, a closing parenthesis 
        /// a Go or a semicolon (this ends an element in an extended expression).
        /// </summary>
        /// <param name="t">Potential end of input, comma, closing parenthesis or semicolon.</param>
        /// <returns>Whether the token ends an extended expression.</returns>
        static public bool IsEndOfExtendedExpression( SqlToken t )
        {
            if( t == null ) throw new ArgumentNullException( "t" );
            return t.TokenType == SqlTokenType.EndOfInput
                        || t.TokenType == SqlTokenType.SemiColon
                        || t.TokenType == SqlTokenType.Go
                        || t.TokenType == SqlTokenType.Comma
                        || t.TokenType == SqlTokenType.ClosePar;
        }


        internal static bool IsIdentifierStartChar( int c )
        {
            return c == '@' || c == '#' || c == '$' || c == '_' || Char.IsLetter( (char)c ) || c == '§' ; // SqlDynFragment supports
        }

        internal static bool IsIdentifierChar( int c )
        {
            return IsIdentifierStartChar( c ) || Char.IsDigit( (char)c );
        }

        /// <summary>
        /// Tests whether an identifier must be quoted (it is empty, starts with @, or $ or contains a character that is not valid).
        /// This DOES NOT consider <see cref="SqlKeyword.IsReservedKeyword(string, out SqlTokenType)"/>.
        /// </summary>
        /// <param name="identifier">Identifier to test.</param>
        /// <returns>True if the identifier can be used without surrounding quotes.</returns>
        static public bool IsQuoteRequired( string identifier )
        {
            if( identifier == null ) throw new ArgumentNullException( "identifier" );
            if( identifier.Length > 0 )
            {
                char c = identifier[0];
                if( c != '@' && c != '$' && IsIdentifierStartChar( c ) )
                {
                    int i = 1;
                    while( i < identifier.Length )
                        if( !IsIdentifierChar( identifier[i++] ) ) break;
                    if( i == identifier.Length ) return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Computes whether a withe space (a separator) is required between two tokens.
        /// </summary>
        /// <param name="left">The left token.</param>
        /// <param name="right">The right token.</param>
        /// <returns>True if a separator is required.</returns>
        static public bool RequiresSeparatorBetween( SqlTokenType left, SqlTokenType right )
        {
            bool isLeftSep = left == SqlTokenType.None
                                || (left & SqlTokenType.RawActualSeparatorMask) != 0
                                || left.IsQuotedIdentifier()
                                || left == SqlTokenType.IdentifierStar;
            if( isLeftSep ) return false;
            Debug.Assert( ((left & SqlTokenType.TokenDiscriminatorMask) & ~(SqlTokenType.IsIdentifier | SqlTokenType.IsString | SqlTokenType.IsNumber)) == 0 );
            bool isRightSep = left == SqlTokenType.None
                                || (right & SqlTokenType.RawActualSeparatorMask) != 0
                                || right.IsQuotedIdentifier()
                                || right == SqlTokenType.IdentifierStar;
            if( isRightSep ) return false;
            Debug.Assert( ((right & SqlTokenType.TokenDiscriminatorMask) & ~(SqlTokenType.IsIdentifier | SqlTokenType.IsString | SqlTokenType.IsNumber)) == 0 );
            // If left is a N'Unicode' or 'ansi' string, we need a separator only if 
            // it is followed by a 'ansi' string.
            if( (left & SqlTokenType.IsString) != 0 )
            {
                return right == SqlTokenType.String;
            }
            // left is now a number or an identifier (and right is a number, an identifier or a string).
            if( (left & SqlTokenType.IsNumber) != 0 )
            {
                // Only when another number follows should we add a separator.
                // All those are valid:
                //  select 2N'jj'from[CK].tUser;
                //  select 2'jj'from[CK].tUser;
                //  select 2from[CK].tUser;
                return right == SqlTokenType.IsNumber;
            }
            // left is a non quoted identifer, if right is a number, an identifier or a N'unicode' string, a separator
            // is required. Only if right is a 'ansi' string can we remove it.
            return right != SqlTokenType.String;
        }

    }

}
