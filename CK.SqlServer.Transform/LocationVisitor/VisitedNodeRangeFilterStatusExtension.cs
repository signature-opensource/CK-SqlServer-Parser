using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Transform
{
    /// <summary>
    /// Extends <see cref="VisitedNodeRangeFilterStatus"/> with (hopefully) easier to understand methods.
    /// </summary>
    public static class VisitedNodeRangeFilterStatusExtension
    {
        /// <summary>
        /// Currently visited node starts before the current <see cref="IVisitContextBase.RangeFilter"/>.
        /// </summary>
        /// <param name="this">This filter status.</param>
        /// <returns>Whether the visited node starts before the filtered range.</returns>
        public static bool IsBegBefore( this VisitedNodeRangeFilterStatus @this ) => (@this & VisitedNodeRangeFilterStatus.FBegBefore) != 0;

        /// <summary>
        /// Currently visited node starts after the current <see cref="IVisitContextBase.RangeFilter"/>.
        /// </summary>
        /// <param name="this">This filter status.</param>
        /// <returns>Whether the visited node starts after the filtered range.</returns>
        public static bool IsBegAfter( this VisitedNodeRangeFilterStatus @this ) => (@this & VisitedNodeRangeFilterStatus.FBegAfter) != 0;

        /// <summary>
        /// Currently visited node is the start of the current <see cref="IVisitContextBase.RangeFilter"/>.
        /// </summary>
        /// <param name="this">This filter status.</param>
        /// <returns>Whether the visited node is the start of the filtered range.</returns>
        public static bool IsFilteredRangeBeg( this VisitedNodeRangeFilterStatus @this ) => (@this & (VisitedNodeRangeFilterStatus.FBegAfter | VisitedNodeRangeFilterStatus.FBegBefore)) == 0;

        /// <summary>
        /// Currently visited node ends before the current <see cref="IVisitContextBase.RangeFilter"/>.
        /// </summary>
        /// <param name="this">This filter status.</param>
        /// <returns>Whether the visited node ends before the filtered range.</returns>
        public static bool IsEndBefore( this VisitedNodeRangeFilterStatus @this ) => (@this & VisitedNodeRangeFilterStatus.FEndBefore) != 0;

        /// <summary>
        /// Currently visited node ends after the current <see cref="IVisitContextBase.RangeFilter"/>.
        /// </summary>
        /// <param name="this">This filter status.</param>
        /// <returns>Whether the visited node ends after the filtered range.</returns>
        public static bool IsEndAfter( this VisitedNodeRangeFilterStatus @this ) => (@this & VisitedNodeRangeFilterStatus.FEndAfter) != 0;

        /// <summary>
        /// Currently visited node is the end of the current <see cref="IVisitContextBase.RangeFilter"/>.
        /// </summary>
        /// <param name="this">This filter status.</param>
        /// <returns>Whether the visited node is the end of the filtered range.</returns>
        public static bool IsFilteredRangeEnd( this VisitedNodeRangeFilterStatus @this ) => (@this & (VisitedNodeRangeFilterStatus.FEndAfter | VisitedNodeRangeFilterStatus.FEndBefore)) == 0;

        /// <summary>
        /// Currently visited node exactly covers the current <see cref="IVisitContextBase.RangeFilter"/>.
        /// </summary>
        /// <param name="this">This filter status.</param>
        /// <returns>Whether the visited node is exactly the filtered range.</returns>
        public static bool IsExactFilteredRange( this VisitedNodeRangeFilterStatus @this ) => @this == VisitedNodeRangeFilterStatus.FIntersecting;

        /// <summary>
        /// Currently visited node is contained in the current <see cref="IVisitContextBase.RangeFilter"/>.
        /// </summary>
        /// <param name="this">This filter status.</param>
        /// <returns>Whether the visited node is contained in the filtered range.</returns>
        public static bool IsIncludedInFilteredRange( this VisitedNodeRangeFilterStatus @this ) => !IsBegBefore( @this ) && !IsEndAfter( @this );
    }
}
