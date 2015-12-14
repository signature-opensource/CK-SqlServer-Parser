using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Parser
{
    public interface ISqlExprCursor
    {
        bool IsSql92Syntax { get; }

        SqlTokenIdentifier CursorT { get; }

        ISelectSpecification Select { get; }

    }
}
