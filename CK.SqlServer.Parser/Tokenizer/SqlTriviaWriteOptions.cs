using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Parser
{
    /// <summary>
    /// Drives how <see cref="SqlTrivia.Write"/> works.
    /// </summary>
    public enum SqlTriviaWriteOption
    {
        /// <summary>
        /// Trivia is fully written.
        /// </summary>
        None,

        /// <summary>
        /// Trivia is replaced by one white space (when <see cref="SqlTrivia.IsEmpty"/> is false).
        /// </summary>
        OneSpace
    }
}
