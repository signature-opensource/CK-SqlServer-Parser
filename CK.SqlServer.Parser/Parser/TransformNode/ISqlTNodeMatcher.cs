using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Parser
{
    public interface ISqlTNodeMatcher : ISqlNode
    {
        bool Match( ISqlNode n );
    }
}
