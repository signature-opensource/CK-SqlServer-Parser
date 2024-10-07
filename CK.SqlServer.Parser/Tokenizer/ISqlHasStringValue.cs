using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Parser;

/// <summary>
/// Unifies the [] and "" quoted identifiers and '' litteral strings.
/// All of them can contain multi-line strings, only the escape character differ.
/// </summary>
public interface ISqlHasStringValue : ISqlNode
{
    /// <summary>
    /// Gets the unescaped string value.
    /// </summary>
    string Value { get; }
}
