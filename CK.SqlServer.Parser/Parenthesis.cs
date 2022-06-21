using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Parser
{
    /// <summary>
    /// Defines requirement for parentheses.
    /// </summary>
    public enum Parenthesis
    {
        /// <summary>
        /// Parentheses are optional.
        /// </summary>
        Optional,

        /// <summary>
        /// Parentheses are required.
        /// </summary>
        Required,

        /// <summary>
        /// Parentheses must not appear.
        /// </summary>
        Rejected
    }
}
