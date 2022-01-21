using CK.Core;
using CK.SqlServer.Parser;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Transform.Transformers
{
    class UnparsedTextTransformer
    {
        readonly UnparsedInjectInfo _info;
        readonly SqlNodeScopeBuilder _scope;

        public UnparsedTextTransformer( UnparsedInjectInfo info, SqlNodeScopeBuilder scope )
        {
            _info = info;
            _scope = scope;
        }

        public bool Apply( IActivityMonitor monitor, SqlTransformHost t )
        {
            if( _info.Location.IsNodeMatchRange ) return ApplyNodeMatchRange( monitor, t );
            return t.Apply( new UnParsedTextInjectVisitor( monitor, _info ), _scope );
        }

        bool ApplyNodeMatchRange( IActivityMonitor monitor, SqlTransformHost t )
        {
            SqlNodeScopeBuilder pattern = new SqlNodeScopePatternRange( _info.Location.PatternRange );
            if( _scope != null ) pattern = new SqlNodeScopeIntersect( _scope, pattern );
            var final = new SqlNodeScopeCardinalityFilter( pattern, _info.Location.Card );
            ISqlNodeLocationRange r = t.BuildRange( monitor, final );

            //SqlNodeScopeBuilder restriction = new SqlNodeScopePatternRange( _info.Location.PatternRange );
            //restriction = new SqlNodeScopeCardinalityFilter( restriction, _info.Location.Card );
            //var scope = _scope == null ? restriction : new SqlNodeScopeIntersect( _scope, restriction );
            //ISqlNodeLocationRange r = t.BuildRange( scope );

            if( r == null || r == SqlNodeLocationRange.EmptySet )
            {
                monitor.Error( $"Range not found." );
                return false;
            }
            if( _info.ClearStarComments )
            {
                // Since cleaning trivias does not change anything to positions and 
                // and we only use the position, there is no need to 
                // recompute the ranges.
                t.Visit( new TriviaCleaner( monitor, false, true, true ), r );
            }
            if( r.Count != 1 )
            {
                foreach( var range in r ) ApplyToRangeBegEnd( t, range );
            }
            else ApplyToRangeBegEnd( t, r.First );
            t.NeedReparse = true;
            return true;
        }

        void ApplyToRangeBegEnd( SqlTransformHost t, SqlNodeLocationRange range )
        {
            ISqlNode n = null;
            if( _info.TextBefore != null )
            {
                var loc = t.CurrentNamespace.GetFullLocation( range.Beg.Position );
                n = loc.Node;
                n = n.SetTrivias( n.LeadingTrivias.Add( new SqlTrivia( SqlTokenType.None, _info.TextBefore ) ), n.TrailingTrivias );
                t.Node = loc.ChangeNode( n );
            }
            if( _info.TextAfter != null )
            {
                var loc = t.CurrentNamespace.GetFullLocation( range.End.Position-1 );
                n = loc.Node;
                n = n.SetTrivias( n.LeadingTrivias, n.TrailingTrivias.Insert( 0, new SqlTrivia( SqlTokenType.None, _info.TextAfter ) ) );
                t.Node = loc.ChangeNode( n );
            }
        }
    }


}
