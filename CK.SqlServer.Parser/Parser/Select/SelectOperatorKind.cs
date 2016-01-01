using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Parser
{
    public enum SelectOperatorKind
    {
        /// <summary>
        /// The initial type, supported by <see cref="SelectSpec"/>.
        /// </summary>
        None,
        /// <summary>
        /// Supported by <see cref="SelectCombine"/>.
        /// </summary>
        UnionDistinct,
        /// <summary>
        /// Supported by <see cref="SelectCombine"/>.
        /// </summary>
        UnionAll,
        /// <summary>
        /// Supported by <see cref="SelectCombine"/>.
        /// </summary>
        Except,
        /// <summary>
        /// Supported by <see cref="SelectCombine"/>.
        /// </summary>
        Intersect,
        /// <summary>
        /// Supported by <see cref="SelectFor"/>.
        /// </summary>
        ForXml,
        /// <summary>
        /// Supported by <see cref="SelectFor"/>.
        /// </summary>
        ForBrowse,
        /// <summary>
        /// Supported by <see cref="SelectFor"/>.
        /// </summary>
        ForJSON,
        /// <summary>
        /// Supported by <see cref="SelectFor"/>.
        /// </summary>
        ForSystemTime,
        /// <summary>
        /// Supported by <see cref="SelectOrderBy"/>.
        /// </summary>
        OrderBy,
         /// <summary>
         /// Supported by <see cref="SelectOption"/>.
         /// </summary>
        Option
    }
}
