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
        static public IEnumerable<SqlToken> ToTokens( this IEnumerable<SqlNode> @this )
        {
            foreach( var a in @this )
            {
                SqlToken t = a as SqlToken;
                if( t != null ) yield return t;
                else foreach( var ta in ToTokens( a.AllTokens ) ) yield return ta;
            }
        }

        /// <summary>
        /// Writes an <see cref="IEnumerable"/> of <see cref="SqlNode"/> without its trivias. 
        /// Calls <see cref="SqlNode.WriteWithoutTrivias(StringBuilder)"/> on each token.
        /// </summary>
        /// <param name="this">An IEnumerable of SqlNode.</param>
        /// <param name="separator">Separator between tokens.</param>
        /// <param name="b">StringBuilder to write into.</param>
        public static StringBuilder WriteWithoutTrivias( 
            this IEnumerable<SqlNode> @this, 
            string separator, StringBuilder b )
        {
            bool one = false;
            foreach( SqlNode t in @this )
            {
                if( one ) b.Append( separator );
                one = true;
                b.Append( t.ToString() );
            }
            return b;
        }

        /// <summary>
        /// Returns a string for an <see cref="IEnumerable"/> of <see cref="SqlNode"/> without its trivias. 
        /// Calls <see cref="SqlNode.WriteWithoutTrivias(StringBuilder)"/> on each node.
        /// </summary>
        /// <param name="this">An IEnumerable of SqlNode.</param>
        /// <param name="separator">Separator between nodes.</param>
        /// <returns>Tokens without trivias.</returns>
        public static string ToStringWithoutTrivias( this IEnumerable<SqlNode> @this, string separator )
        {
            StringBuilder b = new StringBuilder();
            @this.WriteWithoutTrivias( separator, b );
            return b.ToString();
        }

    }
}
