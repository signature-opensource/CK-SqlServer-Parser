using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Parser
{
    /// <summary>
    /// Marker interface for <see cref="SqlTokenDot"/>, <see cref="SqlTokenMultiDots"/> 
    /// and <see cref="SqlTokenDoubleColon"/>.
    /// </summary>
    public interface ISqlTokenIdentifierSeparator : ISqlNode
    {
    }
}
