using CK.SqlServer.Parser;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Transform
{

    /// <summary>
    /// Builds scopes based on a node predicate.
    /// </summary>
    public sealed class SqlNodeScopePatternRange : SqlNodeScopeBuilder
    {
        readonly IReadOnlyList<SqlToken> _pattern;

        public SqlNodeScopePatternRange( IReadOnlyList<SqlToken> pattern )
        {
            if( pattern == null ) throw new ArgumentNullException( nameof( pattern ) );
            _pattern = pattern;
        }

        protected override void DoReset()
        {
        }

        protected override ISqlNodeLocationRange DoEnter( IVisitContext context )
        {
            if( context.Depth != 0 || _pattern.Count == 0 ) return null;
            List<SqlNodeLocationRange> collector = null;
            using( var allTokens = context.VisitedNode.AllTokens.GetEnumerator() )
            {
                int pos = 0;
                if( context.RangeFilter == null ) Matches( context.LocationManager, allTokens, ref pos, context.VisitedNode.Width, ref collector );
                else
                {
                    bool end = false;
                    foreach( SqlNodeLocationRange r in context.RangeFilter.MergeContiguous() )
                    {
                        while( pos < r.Beg.Position )
                        {
                            if( (end = !allTokens.MoveNext()) ) break;
                            ++pos;
                        }
                        if( !end )
                        {
                            int tokenCount = r.End.Position - r.Beg.Position;
                            Matches( context.LocationManager, allTokens, ref pos, tokenCount, ref collector );
                        }
                    }
                }
            }
            return collector != null
                    ? SqlNodeLocationRange.Create( collector, collector.Count, false )
                    : null;
        }

        void Matches( ISqlNodeLocationManager ns, IEnumerator<SqlToken> e, ref int pos, int tokenCount, ref List<SqlNodeLocationRange> collector )
        {
            int remainder = tokenCount - _pattern.Count;
            if( remainder < 0 ) return;
            var w = new Matcher.WindowToken( _pattern.Count, e );
            do
            {
                int idx = w.HeadMatch( _pattern );
                Debug.Assert( idx <= 0 || idx == _pattern.Count );
                if( idx > 0 )
                {
                    var beg = ns.GetRawLocation( pos );
                    pos += idx;
                    var end = ns.GetRawLocation( pos );
                    if( collector == null ) collector = new List<SqlNodeLocationRange>();
                    collector.Add( new SqlNodeLocationRange( beg, end ) );
                }
                ++pos;
                --remainder;
                w.Shift( 1 );
            }
            while( remainder > 0 );

        }

        protected override ISqlNodeLocationRange DoLeave( IVisitContext context )
        {
            return null;
        }

        protected override ISqlNodeLocationRange DoConclude( IVisitContextBase context )
        {
            return null;
        }
    }


}
