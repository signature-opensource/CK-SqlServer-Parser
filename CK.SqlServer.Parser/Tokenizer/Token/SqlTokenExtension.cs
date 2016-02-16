using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Parser
{
    public static class SqlTokenExtension
    {

        /// <summary>
        /// Extracts a named part of an identifier: mimics T-Sql PARSENAME function.
        /// Null is returned if there is no such part.
        /// </summary>
        /// <param name="this">This identifier.</param>
        /// <param name="idxPart">Part index: 1 = Object name, 2 = Schema name, 3 = Database name, 4 = Server name.</param>
        /// <returns>Null or the part name as a string.</returns>
        public static string GetPartName( this ISqlIdentifier @this, int idxPart )
        {
            if( idxPart <= 0 || idxPart > 4 ) throw new ArgumentException( "Must be between 1 and 4.", nameof( idxPart ) );
            IReadOnlyList<ISqlIdentifier> ids = @this.Identifiers; 
            int idx = ids.Count - idxPart;
            return idx >= 0 ? ((SqlTokenIdentifier)ids[idx]).Name : null;
        }

    }
}
