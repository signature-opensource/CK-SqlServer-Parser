using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Transform
{
    internal interface ISqlNodeLocationRangeInternal : ISqlNodeLocationRange
    {
        ISqlNodeLocationRangeInternal InternalSetEachNumber( int value = -1 );

        ISqlNodeLocationRangeInternal InternalSetEnd( SqlNodeLocation end );
    }
}
