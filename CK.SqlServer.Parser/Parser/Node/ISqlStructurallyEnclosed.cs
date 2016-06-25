using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Parser
{
    /// <summary>
    /// Defines node that are always enclosed, typically with <see cref="SqlTokenOpenPar"/>/<see cref="SqlTokenClosePar"/>.
    /// </summary>
    public interface ISqlStructurallyEnclosed : ISqlNode
    {
    }
}
