using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Parser
{
    /// <summary>
    /// Defines node that can be identified by a <see cref="FullName"/>.
    /// </summary>
    public interface ISqlFullNameHolder : ISqlNode
    {
        /// <summary>
        /// Gets the <see cref="ISqlIdentifier"/> that identifies this object.
        /// This can be null for objects that can have no name like <see cref="SqlTransformer"/>.
        /// </summary>
        ISqlIdentifier FullName { get; }
    }
}
