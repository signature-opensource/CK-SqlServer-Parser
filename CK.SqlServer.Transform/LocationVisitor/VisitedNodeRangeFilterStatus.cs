using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Transform
{
    [Flags]
    public enum VisitedNodeRangeFilterStatus
    {
        None = 0,
        FIntersecting = 1,
        FBegBefore = 2,
        FBegAfter = 4,
        FEndBefore = 8,
        FEndAfter = 16
    }

}
