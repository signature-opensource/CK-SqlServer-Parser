using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Core;
using System.Collections.Immutable;

namespace CK.SqlServer.Parser
{
    /// <summary>
    /// Unifies <see cref="SqlTokenIdentifier"/> and <see cref="SqlMultiIdentifier"/>.
    /// </summary>
    public interface ISqlIdentifier : ISqlNode
    {
        /// <summary>
        /// Gets the <see cref="SqlTokenIdentifier"/> that composes this identifier.
        /// </summary>
        IReadOnlyList<ISqlIdentifier> Identifiers { get; }

        /// <summary>
        /// Gets whether this identifier is a variable.
        /// </summary>
        bool IsVariable { get; }

        /// <summary>
        /// Gets whether this identifier is an OpenDataSource function.
        /// </summary>
        bool IsOpenDataSouce { get; }

    }
}
