using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Transform
{
    internal interface ISqlNodeLocationRangeInternal : ISqlNodeLocationRange
    {
        ISqlNodeLocationRangeInternal InternalSetEnd( SqlNodeLocation end );
    }
}
