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
        internal static LocationInfo GetFinderInfo( this SqlTLocationFinder @this ) => new LocationInfo( @this );

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


        public static IEnumerable<SqlNodeLocationRange> ToRanges( this IEnumerable<SqlToken> @this, SqlTCurlyPattern pattern, int offset = 0 )
        {
            if( pattern == null ) throw new ArgumentNullException( nameof(pattern) );
            yield break;
            //List<PTokenList> list = AnalyzePattern( pattern );
            //if( list.Count == 0 ) yield break;
            //foreach( )
            //return null;
        }

    }
}
