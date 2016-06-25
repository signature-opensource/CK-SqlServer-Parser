using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Parser
{
    /// <summary>
    /// Marker interface that defines all <see cref="ISqlStatement"/>
    /// but also parts that we consider "as" statements to ease parse
    /// tree operations.
    /// </summary>
    public interface ISqlStatementPart : ISqlNode
    {
    }
}
