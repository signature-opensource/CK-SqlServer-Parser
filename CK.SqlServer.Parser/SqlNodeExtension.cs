using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace CK.SqlServer.Parser
{
    public static class SqlNodeExtension
    {
        /// <summary>
        /// Gets a flattened list of <see cref="SqlToken"/>.
        /// </summary>
        /// <param name="@this">This enumerable of SqlNode.</param>
        /// <returns>The flattened list of tokens.</returns>
        static public IEnumerable<SqlToken> ToTokens( this IEnumerable<ISqlNode> @this )
        {
            foreach( var a in @this )
            {
                SqlToken t = a as SqlToken;
                if( t != null ) yield return t;
                else foreach( var ta in ToTokens( a.AllTokens ) ) yield return ta;
            }
        }

        public static string ToStringCompact( this IEnumerable<ISqlNode> @this )
        {
            return Write( @this, SqlTextWriter.CreateOneLineCompact() ).ToString();
        }

        public static ISqlTextWriter Write( this IEnumerable<ISqlNode> @this, ISqlTextWriter w )
        {
            foreach( var n in @this ) n.Write( w );
            return w;
        }

    }
}
