using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace CK.SqlServer.Parser
{
    public static class SqlStatementExtension
    {

        /// <summary>
        /// Checks whether this is the GO separator.
        /// </summary>
        /// <param name="this">This statement.</param>
        /// <returns>True for GO separator otherwise false.</returns>
        public static bool IsGOSeparator( this ISqlStatement @this )
        {
            SqlStatement s = @this as SqlStatement;
            return s != null && s.Name.IsToken( SqlTokenType.Go );
        }

    }
}
