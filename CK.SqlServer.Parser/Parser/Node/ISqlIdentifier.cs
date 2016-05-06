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

        /// <summary>
        /// Sets a named part in an identifier. Setting null removes the part (it must exist otherwise an exception is thrown).
        /// The indexing is one based and in reverse order (works the same as the T-Sql PARSENAME function).
        /// It is possible to set the part at an index that do not exist to extend this by one identifier (ie. idxPart can be 
        /// be equal to the <see cref="Identifiers"/>'s count plus one).
        /// </summary>
        /// <param name="idxPart">Part index: 1 = Object name, 2 = Schema name, 3 = Database name, 4 = Server name.</param>
        /// <param name="part">The part to set.</param>
        /// <returns>A new identifier.</returns>
        ISqlIdentifier SetPartName( int idxPart, string part );

    }
}
