using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Parser;

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
    /// A statement can be a select or any sql statement (actually any totally unknown sequence
    /// will be returned as a <see cref="SqlUnnamedStatement"/>). 
    /// GO is considered a statement.
    /// </summary>
    Statement,

    /// <summary>
    /// A transform statement (replace, inject, in, etc.).
    /// </summary>
    TransformStatement,

    /// <summary>
    /// Parses one (the only <see cref="ISqlStatement"/> is returned) or 
    /// multiple statements (into a raw <see cref="SqlNodeList"/>).
    /// </summary>
    OneOrMoreStatements,

    /// <summary>
    /// Parses a <see cref="SqlStatementList"/>.
    /// </summary>
    Script
}
