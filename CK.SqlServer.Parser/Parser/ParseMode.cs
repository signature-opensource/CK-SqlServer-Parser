using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Parser
{
    /// <summary>
    /// Defines parsing methods exposed by the <see cref="SqlAnalyser"/>.
    /// This is used by <see cref="SqlAnalyser.Parse(ParseMode)"/> instance 
    /// method and <see cref="SqlAnalyser.Parse(out ISqlNode, ParseMode, string)"/> static method.
    /// </summary>
    public enum ParseMode
    {
        /// <summary>
        /// One expression only.
        /// </summary>
        OneExpression,
        /// <summary>
        /// An extended expression is one expression or a list (a <see cref="SqlNodeList"/>) of 
        /// tokens or expressions.
        /// A comma or a closing parenthesis stops this.
        /// </summary>
        ExtendedExpression,
        /// <summary>
        /// Any expression can be an extended expression or a comma separated list of 
        /// extended expression.
        /// </summary>
        AnyExpression,
        /// <summary>
        /// A named statement.
        /// </summary>
        NamedStatement,
        /// <summary>
        /// An extended statement can be a named statement or an expression (typically a select).
        /// </summary>
        ExtendedStatement,
        /// <summary>
        /// Parses all possible items.
        /// </summary>
        AllStatements
   }
}
