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
    /// <summary>
    /// Checks the number of ranges.
    /// </summary>
    public sealed class SqlNodeScopeCardinalityFilter : SqlNodeScopeBuilder
    {
        readonly SqlNodeScopeBuilder _inner;
        readonly LocationCardinalityInfo _info;
        readonly FIFOBuffer<SqlNodeLocationRange> _lastBuffer;
        int _matchCount;
        bool _hasError;

        public SqlNodeScopeCardinalityFilter( SqlNodeScopeBuilder inner, LocationCardinalityInfo info )
        {
            if( inner == null ) throw new ArgumentNullException( nameof( inner ) );
            _inner = inner;
            _info = info;
            if( !_info.FromFirst ) _lastBuffer = new FIFOBuffer<SqlNodeLocationRange>( _info.Offset + 1 );
       }

        protected override void DoReset()
        {
            _inner.Reset();
            _matchCount = 0;
            _hasError = false;
        }

        protected override ISqlNodeLocationRange DoEnter( IVisitContext context )
        {
            return Handle( _inner.Enter( context ), context );
        }

        protected override ISqlNodeLocationRange DoLeave( IVisitContext context )
        {
            return Handle( _inner.Leave( context ), context );
        }

        protected override ISqlNodeLocationRange DoConclude( IVisitContextBase context )
        {
            var r = Handle( _inner.Conclude( context ), context );
            if( !_hasError )
            {
                if( _matchCount < _info.ExpectedMatchCount )
                {
                    context.Monitor.Error().Send( $"Expected {_info.ExpectedMatchCount} ranges but found {_matchCount}?." );
                    _hasError = true;
                }
                if( _lastBuffer != null )
                {
                    int idx = _lastBuffer.Count - _info.Offset - 1;
                    if( idx >= 0 ) r = _lastBuffer[idx].SetChunk( _info.Each );
                }
                Debug.Assert( r == null || r.IsChunk() == _info.Each );
                return r;
            }
            return null;
        }

        ISqlNodeLocationRange Handle( ISqlNodeLocationRange inner, IVisitContextBase context )
        {
            if( inner != null
                && inner.Count > 0
                && !_hasError
                && ShouldHandleBasedOnMatchCount( context.Monitor, inner.Count ) )
            {
                if( _lastBuffer != null )
                {
                    Debug.Assert( !_info.FromFirst );
                    foreach( var r in inner ) _lastBuffer.Push( r );
                }
                else
                {
                    Debug.Assert( _info.FromFirst );
                    ISqlNodeLocationRange result = inner;
                    if( !_info.All )
                    {
                        // ShouldHandleBasedOnMatchCount has incremented  _matchCount by inner.Count.
                        int offsetInInner = _info.Offset - _matchCount + inner.Count;
                        result = offsetInInner == 0 ? inner.First : inner.ElementAt( offsetInInner );
                    }
                    if( _info.Each ) result = result.SetChunk();
                    return result;
                }
            }
            return null;
        }

        bool ShouldHandleBasedOnMatchCount( IActivityMonitor monitor, int innerCount )
        {
            if( (_matchCount = _matchCount+innerCount) > 1 
                && _info.ExpectedMatchCount > 0 
                && _matchCount > _info.ExpectedMatchCount )
            {
                monitor.Error().Send( $"Too many matches found for '{_inner.ToString()}' (max is {_info.ExpectedMatchCount})." );
                _hasError = true;
            }
            else if( _info.All || !_info.FromFirst || _matchCount >= _info.Offset + 1 )
            {
                return true;
            }
            return false;
        }

        public override string ToString() => $"(cardinality '{_info.ToString()}' on {_inner})";


    }


}
