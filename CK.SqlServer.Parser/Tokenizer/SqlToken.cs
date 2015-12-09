using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using CK.Core;
using System.Diagnostics;
using System.Globalization;
using System.Collections.Immutable;

namespace CK.SqlServer.Parser
{
    /// <summary>
    /// Base class for (non comment) tokens. 
    /// </summary>
    public abstract class SqlToken : SqlNode, ISqlItem
    {
        class EmptyToken : SqlToken
        {
            internal EmptyToken( ImmutableList<SqlTrivia> leading, ImmutableList<SqlTrivia> trailing ) 
                : base( leading, trailing )
            {
            }

            protected override void DoWrite( StringBuilder b ) { }
            public override string ToString() { return String.Empty; }
            protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IReadOnlyList<SqlNode> content, ImmutableList<SqlTrivia> trailing )
            {
                return new EmptyToken( leading, trailing );
            }
        }

        /// <summary>
        /// Empty token has a <see cref="SqlToken.TokenType"/> of <see cref="SqlTokenType.None"/> and empty leading and trailing trivias.
        /// </summary>
        public static readonly SqlToken Empty = new EmptyToken( null, null );

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
        }

        /// <summary>
        /// Token type. It is necessarily positive (not an error). Only <see cref="Empty"/> has <see cref="SqlTokenType.None"/> type.
        /// </summary>
        public readonly SqlTokenType TokenType;

        /// <summary>
        /// Gets an empty node list.
        /// </summary>
        public override IReadOnlyList<SqlNode> ChildrenNodes => Util.EmptyArray<SqlNode>.Empty;

        IEnumerable<SqlToken> ISqlItem.Tokens
        {
            get { return new CKReadOnlyListMono<SqlToken>( this ); }
        }

        SqlToken ISqlItem.LastOrEmptyT { get { return this; } }

        SqlToken ISqlItem.FirstOrEmptyT { get { return this; } }

        /// <summary>
        /// Empty parenthesis opener.
        /// </summary>
        static public readonly SqlTokenList<SqlTokenOpenPar> EmptyOpenPar = SqlTokenList<SqlTokenOpenPar>.Empty;

        /// <summary>
        /// Empty parenthesis closer.
        /// </summary>
        static public readonly SqlTokenList<SqlTokenClosePar> EmptyClosePar = SqlTokenList<SqlTokenClosePar>.Empty;

        /// <summary>
        /// True if the <see cref="SqlToken"/> is the terminator ; token or a <see cref="SqlTokenType.IdentifierReservedStatement"/>.
        /// </summary>
        /// <param name="t">Token to test.</param>
        /// <returns>Whether the token is the statement terminator or the possible start of a new statement.</returns>
        static public bool IsTerminatorOrPossibleStartStatement( SqlToken t )
        {
            if( t == null ) throw new ArgumentNullException( "t" );
            return t.TokenType == SqlTokenType.SemiColon
                    || (t.TokenType & SqlTokenType.IdentifierTypeMask) == SqlTokenType.IdentifierStandardStatement
                    || (t.TokenType & SqlTokenType.IdentifierTypeMask) == SqlTokenType.IdentifierReservedStatement;
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
