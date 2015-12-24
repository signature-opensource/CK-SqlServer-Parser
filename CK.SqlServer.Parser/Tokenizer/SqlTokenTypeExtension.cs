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
        /// True if the <see cref="SqlNode"/> is a select operator: <see cref="SqlTokenType.Union"/>, <see cref="SqlTokenType.Except"/>, 
        /// <see cref="SqlTokenType.Intersect"/>, <see cref="SqlTokenType.Order"/> and <see cref="SqlTokenType.For"/>.
        /// </summary>
        /// <param name="type">Token type.</param>
        /// <returns>Whether the token is a select operator.</returns>
        static public bool IsSelectOperator( this SqlTokenType type )
        {
            return type == SqlTokenType.Union
                    || type == SqlTokenType.Except
                    || type == SqlTokenType.Intersect
                    || type == SqlTokenType.Order
                    || type == SqlTokenType.For;
        }

        /// <summary>
        /// True if the token is a @variable (or @@SystemFunction like @@RowCount) or a 
        /// literal value ('string' or 0x5454 number for instance).
        /// </summary>
        /// <param name="type">Token to test.</param>
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
