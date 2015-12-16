using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Parser
{
    public static class ISqlItemExtension
    {

        /// <summary>
        /// True if the <see cref="SqlNode"/> is an unquoted identifier with a given name.
        /// Comparison is <see cref="StringComparison.OrdinalIgnoreCase"/>.
        /// </summary>
        /// <param name="t">Potential unquoted identifier.</param>
        /// <param name="name">Name of the unquoted identifier.</param>
        /// <returns>Whether the token is the named one.</returns>
        static public bool IsUnquotedIdentifier( this ISqlNode t, string name )
        {
            SqlTokenIdentifier id = t as SqlTokenIdentifier;
            return id != null && !id.IsQuoted && id.NameEquals( name );
        }

        /// <summary>
        /// True if the <see cref="SqlNode"/> is an unquoted identifier with a given name or an alternate one.
        /// </summary>
        /// <param name="t">Potential unquoted identifier.</param>
        /// <param name="name">Name of the unquoted identifier.</param>
        /// <param name="altName">Alternate name of the unquoted identifier.</param>
        /// <returns>Whether the token the is named one.</returns>
        static public bool IsUnquotedIdentifier( this ISqlNode t, string name, string altName )
        {
            SqlTokenIdentifier id = t as SqlTokenIdentifier;
            return id != null && !id.IsQuoted && (id.NameEquals( name ) || id.NameEquals( altName ));
        }

        /// <summary>
        /// True if the <see cref="SqlNode"/> is a dot token.
        /// </summary>
        /// <param name="t">Potential dot token.</param>
        /// <returns>Whether the token is a dot.</returns>
        static public bool IsDotSeparator( this ISqlNode t )
        {
            return (t is SqlToken) && ((SqlToken)t).TokenType == SqlTokenType.Dot;
        }

        /// <summary>
        /// True if the <see cref="SqlNode"/> is a litteral token.
        /// </summary>
        /// <param name="t">Potential literal token.</param>
        /// <returns>Whether the token is a literal.</returns>
        static public bool IsLiteralToken( this ISqlNode t )
        {
            return (t is SqlToken) && (((SqlToken)t).TokenType & SqlTokenType.LitteralMask) != 0;
        }

        /// <summary>
        /// True if the <see cref="SqlNode"/> is a <see cref="SqlTokenType.Dot">dot</see> or a <see cref="SqlTokenType.DoubleColons">double colon</see> token.
        /// </summary>
        /// <param name="t">Token to test.</param>
        /// <returns>Whether the token is a dot or double colon token.</returns>
        static public bool IsDotOrDoubleColonSeparator( this ISqlNode t )
        {
            SqlToken token = t as SqlToken;
            return token != null && (token.TokenType == SqlTokenType.Dot || token.TokenType == SqlTokenType.DoubleColons);
        }

        /// <summary>
        /// True if the <see cref="SqlNode"/> is a comma or a closing parenthesis or a ; token (this ends an element in a list).
        /// </summary>
        /// <param name="t">Potential comma, closing parenthesis or semicolon token.</param>
        /// <returns>Whether the token is a comma or a closing parenthesis or the statement terminator.</returns>
        static public bool IsCommaOrCloseParenthesisOrTerminator( this ISqlNode t )
        {
            SqlToken token = t as SqlToken;
            return token != null && (token.TokenType == SqlTokenType.EndOfInput || token.TokenType == SqlTokenType.Comma || token.TokenType == SqlTokenType.ClosePar || token.TokenType == SqlTokenType.SemiColon);
        }

        /// <summary>
        /// True if the <see cref="SqlNode"/> is a closing parenthesis or a ; token (this ends an element in a list).
        /// </summary>
        /// <param name="t">Closing parenthesis or semicolon token.</param>
        /// <returns>Whether the token is closing parenthesis or the statement terminator.</returns>
        static public bool IsCloseParenthesisOrTerminator( this ISqlNode t )
        {
            SqlToken token = t as SqlToken;
            return token != null && (token.TokenType == SqlTokenType.ClosePar || token.TokenType == SqlTokenType.SemiColon);
        }

        /// <summary>
        /// True if the <see cref="SqlNode"/> is a closing parenthesis, a terminator ; token or a <see cref="SqlTokenType.IdentifierReservedStatement"/>.
        /// </summary>
        /// <param name="t">Closing parenthesis or semicolon token.</param>
        /// <returns>Whether the token is closing parenthesis or the statement terminator.</returns>
        static public bool IsCloseParenthesisOrTerminatorOrPossibleStartStatement( this ISqlNode t )
        {
            SqlToken token = t as SqlToken;
            return token != null
                && (token.TokenType == SqlTokenType.ClosePar
                    || token.TokenType == SqlTokenType.SemiColon
                    || (token.TokenType & SqlTokenType.IdentifierTypeMask) == SqlTokenType.IdentifierStandardStatement
                    || (token.TokenType & SqlTokenType.IdentifierTypeMask) == SqlTokenType.IdentifierReservedStatement);
        }


    }
}
