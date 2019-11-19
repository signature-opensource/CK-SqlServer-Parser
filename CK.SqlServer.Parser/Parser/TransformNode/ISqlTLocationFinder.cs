using System;
using System.Collections.Generic;
using System.Text;

namespace CK.SqlServer.Parser
{
    public interface ISqlTLocationFinder : ISqlNode
    {
        /// <summary>
        /// Gets a <see cref="ISqlHasStringValue"/> or a <see cref="SqlTNodeSimplePattern"/>.
        /// </summary>
        ISqlNode Pattern { get; }

        /// <summary>
        /// Gets the normalized cardinality.
        /// </summary>
        LocationCardinalityInfo GetCardinality();
    }
}
