using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Parser
{
    public static class SqlTokenTypeExtension
    {
        /// <summary>
        /// True if this is a select operator: <see cref="SqlTokenType.Union"/>, <see cref="SqlTokenType.Except"/>, 
        /// <see cref="SqlTokenType.Intersect"/>, <see cref="SqlTokenType.Order"/>, <see cref="SqlTokenType.For"/>
        /// and <see cref="SqlTokenType.Option"/>.
        /// </summary>
        /// <param name="type">Token type.</param>
        /// <returns>Whether the token is a select operator.</returns>
        static public bool IsSelectOperator( this SqlTokenType type )
        {
            return type == SqlTokenType.Union
                    || type == SqlTokenType.Except
                    || type == SqlTokenType.Intersect
                    || type == SqlTokenType.Order
                    || type == SqlTokenType.For 
                    || type == SqlTokenType.Option;
        }

        /// <summary>
        /// True if the token is valid as an alias for column name:
        /// it is a string, a unicode string or an identifier that is not reserved 
        /// nor special but can be a variable name to support @var = definition 
        /// syntax in select.
        /// </summary>
        /// <param name="type">Token type to test.</param>
        /// <param name="allowVariableName">True </param>
        /// <returns>True if this is a valid column name alias.</returns>
        static public bool IsValidColumnAliasNameOrVariable( this SqlTokenType type )
        {
            return type == SqlTokenType.IdentifierVariable
                    || type == SqlTokenType.String
                    || type == SqlTokenType.UnicodeString
                    || (type & SqlTokenType.IdentifierTypeMask) == SqlTokenType.IdentifierStandard
                    || (type & SqlTokenType.IdentifierTypeMask) == SqlTokenType.IdentifierStandardStatement
                    || (type & SqlTokenType.IdentifierTypeMask) == SqlTokenType.IdentifierQuoted
                    || (type & SqlTokenType.IdentifierTypeMask) == SqlTokenType.IdentifierQuotedBracket
                    || (type & SqlTokenType.IdentifierTypeMask) == SqlTokenType.IdentifierDbType;
        }

        /// <summary>
        /// True if the token is valid as an alias for column name:
        /// it is a string, a unicode string or an identifier that is not reserved 
        /// nor special nor is a variable name.
        /// </summary>
        /// <param name="type">Token type to test.</param>
        /// <param name="allowVariableName">True to authorize variable name (ie. to 
        /// support @var = definition syntax in select).</param>
        /// <returns>True if this is a valid column name alias.</returns>
        static public bool IsValidColumnAliasName( this SqlTokenType type )
        {
            return type == SqlTokenType.String
                    || type == SqlTokenType.UnicodeString
                    || (type & SqlTokenType.IdentifierTypeMask) == SqlTokenType.IdentifierStandard
                    || (type & SqlTokenType.IdentifierTypeMask) == SqlTokenType.IdentifierStandardStatement
                    || (type & SqlTokenType.IdentifierTypeMask) == SqlTokenType.IdentifierQuoted
                    || (type & SqlTokenType.IdentifierTypeMask) == SqlTokenType.IdentifierQuotedBracket
                    || (type & SqlTokenType.IdentifierTypeMask) == SqlTokenType.IdentifierDbType;
        }

        /// <summary>
        /// True if the token is a @variable (or @@SystemFunction like @@RowCount) or a 
        /// literal value ('string' or 0x5454 number for instance).
        /// </summary>
        /// <param name="type">Token type to test.</param>
        /// <returns>True for a variable or a literal.</returns>
        static public bool IsVariableNameOrLiteral( this SqlTokenType type )
        {
            return type == SqlTokenType.IdentifierVariable 
                    || (type > 0 && (type & SqlTokenType.LitteralMask) != 0);
        }

        /// <summary>
        /// True if the token is a @variable (or @@SystemFunction like @@RowCount) or a 
        /// literal value ('string' or 0x5454 number for instance) or null.
        /// </summary>
        /// <param name="type">Token to test.</param>
        /// <returns>True for a variable, a literal or null.</returns>
        static public bool IsVariableNameOrLiteralOrNull( SqlTokenType type )
        {
            return type == SqlTokenType.IdentifierVariable 
                    || type == SqlTokenType.Null 
                    || (type > 0 && (type & SqlTokenType.LitteralMask) != 0);
        }

        /// <summary>
        /// True if this <see cref="SqlTokenType"/> denotes a reserved keyword (select, create, declare, etc.)
        /// or a standard identifer that starts a statement (throw, get, move, etc.).
        /// </summary>
        /// <param name="this">Token to test.</param>
        static public bool IsStartStatement( this SqlTokenType type )
        {
            return type > 0
                    && (type & SqlTokenType.IdentifierTypeMask) == SqlTokenType.IdentifierStandardStatement
                        || (type & SqlTokenType.IdentifierTypeMask) == SqlTokenType.IdentifierReservedStatement;
        }

        /// <summary>
        /// True if this <see cref="SqlTokenType"/> denotes a special identifier ($action, * in select * from..., etc).
        /// </summary>
        /// <param name="this">Token to test.</param>
        static public bool IsIdentifierSpecial( this SqlTokenType type )
        {
            return type > 0
                    && (type & SqlTokenType.IdentifierTypeMask) == SqlTokenType.IdentifierSpecial;
        }

        /// <summary>
        /// True if this <see cref="SqlTokenType"/> denotes a reserved keyword.
        /// </summary>
        /// <param name="this">Token to test.</param>
        static public bool IsReservedKeyword( this SqlTokenType type )
        {
            return type > 0 
                    && (type & SqlTokenType.IdentifierTypeMask) == SqlTokenType.IdentifierReserved
                        || (type & SqlTokenType.IdentifierTypeMask) == SqlTokenType.IdentifierReservedStatement;
        }

    }
}
