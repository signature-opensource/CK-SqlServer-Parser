using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Parser
{
    /// <summary>
    /// Defines actual statements. 
    /// The <see cref="StatementTerminator"/> is optional.
    /// </summary>
    public interface ISqlStatement : ISqlStatementPart
    {
        SqlTokenTerminal StatementTerminator { get; }
    }
}
