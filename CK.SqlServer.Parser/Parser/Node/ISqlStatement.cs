using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Parser
{
    public interface ISqlStatement : ISqlNode
    {
        SqlTokenTerminal StatementTerminator { get; }
    }
}
