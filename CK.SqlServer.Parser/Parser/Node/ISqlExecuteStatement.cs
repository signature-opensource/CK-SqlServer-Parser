using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Parser
{
    /// <summary>
    /// Abstraction of an exec statement: either a <see cref="SqlExecuteStringStatement"/> or a <see cref="SqlExecuteStatement"/>.
    /// </summary>
    public interface ISqlExecuteStatement : ISqlNamedStatement
    {
        /// <summary>
        /// Gets whether this is a <see cref="SqlExecuteStringStatement"/>.
        /// </summary>
        bool IsExecuteString { get; }

    }
}
