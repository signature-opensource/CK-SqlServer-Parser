using CK.Core;
using CK.SqlServer.Parser;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Transform
{
    public static class SqlTNodeExtension
    {
        /// <summary>
        /// Creates a new <see cref="LocationInfo"/>.
        /// </summary>
        /// <param name="this">This finder.</param>
        /// <param name="isAfterContext">
        /// Whether we are in an "after" context (as opposed to "before").
        /// This is when <see cref="ISqlTLocationFinder.Pattern"/> is a <see cref="ISqlHasStringValue"/> (a comment pattern)
        /// to be able to configure <see cref="SqlNodeScopeFromTriviaMatcher"/>.
        /// </param>
        /// <returns>A LocationInfo.</returns>
        internal static LocationInfo GetFinderInfo( this ISqlTLocationFinder @this, bool isAfterContext ) => new LocationInfo( @this, isAfterContext );

        public static bool MatchPartOrStatement( this SqlTNodeSimplePattern @this, ISqlNode n )
        {
            if( @this.IsMatchPart && !(n is ISqlStatementPart)
                || @this.IsMatchStatement && !(n is ISqlStatement) ) return false;
            return @this.Pattern.Match( n ) > 0;
        }

        public static int Match( this SqlTCurlyPattern @this, ISqlNode n )
        {
            var tokens = n.AllTokens.GetEnumerator();
            try
            {
                return tokens.MoveNext() ? Matcher.WindowToken.RawHeadMatch( tokens, int.MaxValue, @this ) : 0;
            }
            finally
            {
                tokens.Dispose();
            }
        }

    }
}
