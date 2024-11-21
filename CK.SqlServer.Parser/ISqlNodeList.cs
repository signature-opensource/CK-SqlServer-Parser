using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace CK.SqlServer.Parser;

/// <summary>
/// Defines a read only list of <see cref="ISqlNode"/> that is itself a <see cref="ISqlNode"/>.
/// </summary>
public interface ISqlNodeList<T> : ISqlNode, IReadOnlyList<T> where T : ISqlNode
{
}
