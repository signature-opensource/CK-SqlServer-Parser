using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Transform
{
    public static class VisitedNodeRangeFilterStatusExtension
    {
        public static bool IsBegBefore( this VisitedNodeRangeFilterStatus @this ) => (@this & VisitedNodeRangeFilterStatus.FBegBefore) != 0;
        public static bool IsBegAfter( this VisitedNodeRangeFilterStatus @this ) => (@this & VisitedNodeRangeFilterStatus.FBegAfter) != 0;
        public static bool IsFilteredRangeBeg( this VisitedNodeRangeFilterStatus @this ) => (@this & (VisitedNodeRangeFilterStatus.FBegAfter | VisitedNodeRangeFilterStatus.FBegBefore)) == 0;
        public static bool IsEndBefore( this VisitedNodeRangeFilterStatus @this ) => (@this & VisitedNodeRangeFilterStatus.FEndBefore) != 0;
        public static bool IsEndAfter( this VisitedNodeRangeFilterStatus @this ) => (@this & VisitedNodeRangeFilterStatus.FEndAfter) != 0;
        public static bool IsFilteredRangeEnd( this VisitedNodeRangeFilterStatus @this ) => (@this & (VisitedNodeRangeFilterStatus.FEndAfter | VisitedNodeRangeFilterStatus.FEndBefore)) == 0;
        public static bool IsExactFilteredRange( this VisitedNodeRangeFilterStatus @this ) => @this == VisitedNodeRangeFilterStatus.FIntersecting;
        public static bool IsIncludedInFilteredRange( this VisitedNodeRangeFilterStatus @this ) => !IsBegBefore( @this ) && !IsEndAfter( @this );
    }
}
