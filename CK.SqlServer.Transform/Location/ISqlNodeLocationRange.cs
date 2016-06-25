using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Transform
{
    /// <summary>
    /// Generalization of a range: it is always an enumerable of <see cref="SqlNodeLocationRange"/>
    /// or an actual SqlNodeLocationRange.
    /// This list does not contain null ranges and ranges are necessarily non empty and follow an ascending order.
    /// </summary>
    public interface ISqlNodeLocationRange : IEnumerable<SqlNodeLocationRange>
    {
        /// <summary>
        /// Gets the first <see cref="SqlNodeLocationRange"/>.
        /// </summary>
        SqlNodeLocationRange First { get; }

        /// <summary>
        /// Gets the number of <see cref="SqlNodeLocationRange"/> that this range contains.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Gets the last <see cref="SqlNodeLocationRange"/>.
        /// </summary>
        SqlNodeLocationRange Last { get; }
    }
}
