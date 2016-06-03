using CK.SqlServer.Parser;
using System;
using System.Collections.Generic;
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
            var patterns = @this.GetEnumerator();
            try
            {
                if( !tokens.MoveNext() || !patterns.MoveNext() ) return 0;
                int width = 0;
                for( ;;)
                {
                    if( patterns.Current.TokenType == SqlTokenType.QuestionMark
                        || tokens.Current.TokenEquals( patterns.Current ) )
                    {
                        ++width;
                        if( !patterns.MoveNext() ) return width;
                        if( !tokens.MoveNext() ) return -width;
                    }
                    else return -width;
                }
            }
            finally
            {
                tokens.Dispose();
                patterns.Dispose();
            }
        }
    }
}
