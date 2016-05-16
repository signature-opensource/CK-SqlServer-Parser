using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Parser
{
    /// <summary>
    /// Defines node that can be enclosed, typically with <see cref="SqlTokenOpenPar"/>/<see cref="SqlTokenClosePar"/>.
    /// </summary>
    public interface ISqlEnclosable : ISqlNode
    {
        /// <summary>
        /// Gets whether this node is enclosed or not.
        /// </summary>
        bool IsEnclosed { get; }
    }
}
