using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Parser
{
    /// <summary>
    /// Defines the operator that can be used to combine or decorate select.
    /// </summary>
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
        /// Decorator: supports order by, for [xml|browse|json|system_time] and option.
        /// </summary>
        Decorator,
    }
}
