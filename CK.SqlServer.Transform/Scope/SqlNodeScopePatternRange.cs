using CK.SqlServer.Parser;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CK.SqlServer.Transform
{

    /// <summary>
    /// Builds ranges that match a list of tokens.
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
            if( tokenCount < _pattern.Count ) return;
            int width;
            var w = new Matcher.WindowToken( _pattern.Count, e );
            do
            {
                width = w.HeadMatch( _pattern );
                Debug.Assert( width <= 0 || width == _pattern.Count );
                if( width > 0 )
                {
                    var beg = ns.GetRawLocation( pos );
                    var end = ns.GetRawLocation( pos + width );
                    if( collector == null ) collector = new List<SqlNodeLocationRange>();
                    collector.Add( new SqlNodeLocationRange( beg, end ) );
                }
                else width = 1;
                pos += width;
            }
            while( w.Shift( width ) == _pattern.Count );
        }

        protected override ISqlNodeLocationRange DoLeave( IVisitContext context )
        {
            return null;
        }

        protected override ISqlNodeLocationRange DoConclude( IVisitContextBase context )
        {
            return null;
        }

        public override string ToString() => $"like {{{_pattern.ToStringCompact()}}}";

    }


}
