using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Parser;

/// <summary>
/// Defines node that holds a parameter list.
/// </summary>
public interface ISqlParameterListHolder : ISqlNode
{
    /// <summary>
    /// Gets the parameters.
    /// </summary>
    SqlParameterList Parameters { get; }

    /// <summary>
    /// Sets parameters.
    /// </summary>
    /// <param name="parameters">Parameters to set.</param>
    /// <returns>A new parameters holder.</returns>
    ISqlParameterListHolder SetParameters( SqlParameterList parameters );
}
