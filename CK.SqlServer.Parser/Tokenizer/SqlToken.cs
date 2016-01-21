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

        /// <summary>
        /// Gets an empty node list.
        /// </summary>
        public override IReadOnlyList<ISqlNode> ChildrenNodes => Util.EmptyArray<SqlNode>.Empty;

        public override bool IsToken( SqlTokenType t ) => TokenType == t;

        #region IEnumerable<SqlToken> AllTokens auto implementation
        public override IEnumerable<SqlToken> AllTokens => this;

        IEnumerator<SqlToken> IEnumerable<SqlToken>.GetEnumerator()
        {
            return new CKEnumeratorMono<SqlToken>( this );
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new CKEnumeratorMono<SqlToken>( this );
        }
        #endregion


        /// <summary>
        /// True if the <see cref="SqlToken"/> is the terminator statement ';'.
        /// </summary>
        /// <param name="t">Token to test.</param>
        /// <returns>Whether the token is the statement terminator.</returns>
        static public bool IsTerminator( SqlToken t )
        {
            Debug.Assert( t != null );
            return t.TokenType == SqlTokenType.SemiColon;
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
            return c == '@' || c == '#' || c == '$' || c == '_' || Char.IsLetter( (char)c );
        }

        internal static bool IsIdentifierChar( int c )
        {
            return IsIdentifierStartChar( c ) || Char.IsDigit( (char)c );
        }

        /// <summary>
        /// Tests whether an identifier must be quoted (it is empty, starts with @, or $ or contains a character that is not valid).
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
    }

}
