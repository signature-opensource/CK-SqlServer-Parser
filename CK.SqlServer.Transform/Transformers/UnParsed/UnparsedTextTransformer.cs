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

        public bool Apply( SqlNodeTransformer t )
        {
            if( _info.Location.IsNodeMatchRange ) return ApplyNodeMatchRange( t );
            return t.Apply( new UnParsedTextInjecVisitor( _info ), _scope );
        }

        bool ApplyNodeMatchRange( SqlNodeTransformer t )
        {
            SqlNodeScopeBuilder restriction = new SqlNodeScopeDepthPredicate( _info.Location.PatternRangeMatcher );
            restriction = new SqlNodeScopeCardinalityFilter( restriction, _info.Location.Card ); 
            var scope = _scope == null ? restriction : new SqlNodeScopeIntersect( _scope, restriction );
            ISqlNodeLocationRange r = t.BuildRange( scope );
            if( r == null || r == SqlNodeLocationRange.EmptySet )
            {
                t.Monitor.Error().Send( $"Range not found." );
                return false;
            }
            if( _info.Location.Card.ExpectedMatchCount != 0 )
            {
                if( r.Count > _info.Location.Card.ExpectedMatchCount )
                {
                    t.Monitor.Error().Send( $"Too many ranges match: expected {_info.Location.Card.ExpectedMatchCount} but found {r.Count}." );
                    return false;
                }
                else if( r.Count < _info.Location.Card.ExpectedMatchCount )
                {
                    t.Monitor.Error().Send( $"Missing ranges: expected {_info.Location.Card.ExpectedMatchCount} but found {r.Count}." );
                    return false;
                }
            }
            if( _info.ClearStarComments )
            {
                // Since cleanig trivias does not change anything to positions and 
                // and we only use the position, there is no need to 
                // recompute the ranges.
                t.Visit( new TriviaCleaner( false, true, true ), r );
            }
            if( _info.Location.Card.All && r.Count != 1 )
            {
                foreach( var range in r ) ApplyToRangeBegEnd( t, range );
            }
            else
            {
                int index = _info.Location.Card.FromFirst ? _info.Location.Card.Offset : r.Count - _info.Location.Card.Offset - 1;
                ApplyToRangeBegEnd( t, r.ElementAt( index ) );
            }
            t.NeedReparse = true;
            return true;
        }

        void ApplyToRangeBegEnd( SqlNodeTransformer t, SqlNodeLocationRange range )
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
