using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Transform
{
    /// <summary>
    /// Describes the <see cref="IVisitContext.RangeFilterStatus" />.
    /// </summary>
    [Flags]
    public enum VisitedNodeRangeFilterStatus
    {
        /// <summary>
        /// Currently visited node does not intersect the filter.
        /// </summary>
        None = 0,

        /// <summary>
        /// Currently visited node intersects the filter.
        /// When this bit is the only one set, it means that the visited node exactly covers
        /// the filtered range.
        /// </summary>
        FIntersecting = 1,

        /// <summary>
        /// Currently visited node starts before the filtered range.
        /// </summary>
        FBegBefore = 2,

        /// <summary>
        /// Currently visited node starts after the filtered range.
        /// </summary>
        FBegAfter = 4,

        /// <summary>
        /// Currently visited node ends before the filtered range.
        /// </summary>
        FEndBefore = 8,

        /// <summary>
        /// Currently visited node ends after the filtered range.
        /// </summary>
        FEndAfter = 16
    }

}
