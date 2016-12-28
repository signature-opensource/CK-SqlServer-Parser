using CK.Core;
using CK.SqlServer.Parser;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace CK.SqlServer.Transform
{
    /// <summary>
    /// Checks the number of ranges.
    /// </summary>
    public sealed class SqlNodeScopeCardinalityFilter : SqlNodeScopeBuilder
    {
        readonly EachSplitter _inner;
        readonly LocationCardinalityInfo _info;
        readonly FIFOBuffer<SqlNodeLocationRange> _lastBuffer;
        int _matchCount;
        int _eachNumber;
        bool _hasError;

        class SqlNodeLocationRangeSlice : ISqlNodeLocationRange
        {
            readonly ISqlNodeLocationRange _r;
            readonly int _offset;
            readonly int _count;

            SqlNodeLocationRangeSlice( ISqlNodeLocationRange r, int offset, int count )
            {
                _r = r;
                _offset = offset;
                _count = count;
            }

            public static ISqlNodeLocationRange Create( ISqlNodeLocationRange r, int offset, int count )
            {
                if( r == null || count == 0 ) return SqlNodeLocationRange.EmptySet;
                if( offset + count > r.Count || offset < 0 ) throw new ArgumentOutOfRangeException();
                if( count == r.Count ) return r;
                if( count == 1 )
                {
                    if( offset == 0 ) return r.First;
                    else if( offset == r.Count - 1 ) return r.Last;
                    return r.ElementAt( offset );
                }
                return new SqlNodeLocationRangeSlice( r, offset, count );
            }

            public int Count => _count;

            public SqlNodeLocationRange First => _r.ElementAt( _offset );

            public SqlNodeLocationRange Last => _r.ElementAt( _offset + _count );

            public IEnumerator<SqlNodeLocationRange> GetEnumerator() => _r.Skip( _offset ).Take( _count ).GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        class EachSplitter
        {
            public readonly SqlNodeScopeBuilder Inner;
            int _currentEachNumber;

            public EachSplitter( SqlNodeScopeBuilder inner )
            {
                Inner = inner;
            }

            public void Reset()
            {
                Inner.Reset();
                _currentEachNumber = 0;
            }

            public IEnumerable<ISqlNodeLocationRange> Enter( IVisitContext context )
            {
                return Handle( Inner.Enter( context ), context );
            }

            public IEnumerable<ISqlNodeLocationRange> Leave( IVisitContext context )
            {
                return Handle( Inner.Leave( context ), context );
            }

            public IEnumerable<ISqlNodeLocationRange> Conclude( IVisitContextBase context )
            {
                var r = Handle( Inner.Conclude( context ), context );
                _currentEachNumber = 0;
                return r;
            }

            IEnumerable<ISqlNodeLocationRange> Handle( ISqlNodeLocationRange inner, IVisitContextBase context )
            {
                if( inner == null || inner.Count <= 1 ) return inner;
                List<ISqlNodeLocationRange> result = null;
                int idxLast = 0;
                int idx = 0;
                foreach( var r in inner )
                {
                    if( r.EachNumber != _currentEachNumber )
                    {
                        int countToEmit = idx - idxLast;
                        if( countToEmit > 0 )
                        {
                            if( result == null ) result = new List<ISqlNodeLocationRange>();
                            result.Add( SqlNodeLocationRangeSlice.Create( inner, idxLast, countToEmit ) );
                            idxLast = idx;
                        }
                        _currentEachNumber = r.EachNumber;
                    }
                    idx++;
                }
                if( result != null )
                {
                    int remainder = idx - idxLast;
                    if( remainder > 0 )
                    {
                        result.Add( SqlNodeLocationRangeSlice.Create( inner, idxLast, remainder ) );
                    }
                    return result;
                }
                return inner;
            }
        }

        public SqlNodeScopeCardinalityFilter( SqlNodeScopeBuilder inner, LocationCardinalityInfo info )
        {
            if( inner == null ) throw new ArgumentNullException( nameof( inner ) );
            _inner = new EachSplitter( inner );
            _info = info;
            _eachNumber = -1;
            if( !_info.FromFirst ) _lastBuffer = new FIFOBuffer<SqlNodeLocationRange>( _info.Offset + 1 );
       }

        protected override void DoReset()
        {
            _inner.Reset();
            _eachNumber = -1;
            LocalStateReset();
        }

        void LocalStateReset()
        {
            if( _lastBuffer != null ) _lastBuffer.Clear();
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
            if( !_hasError ) return LocalConclude( r, context );
            return null;
        }

        ISqlNodeLocationRange LocalConclude( ISqlNodeLocationRange r, IVisitContextBase context )
        {
            Debug.Assert( !_hasError );
            if( _matchCount < _info.ExpectedMatchCount )
            {
                context.Monitor.Error().Send( $"Expected {_info.ExpectedMatchCount} ranges but found {_matchCount}." );
                _hasError = true;
            }
            if( _lastBuffer != null )
            {
                Debug.Assert( !_info.Each );
                int idx = _lastBuffer.Count - _info.Offset - 1;
                if( idx >= 0 ) r = _lastBuffer[idx];
            }
            return r;
        }

        ISqlNodeLocationRange Handle( IEnumerable<ISqlNodeLocationRange> inner, IVisitContextBase context )
        {
            if( inner == null ) return null; 
            var multi = inner as List<ISqlNodeLocationRange>;
            if( multi == null )
            {
                return HandleSameEachNumber( (ISqlNodeLocationRange)inner, context );
            }

            ISqlNodeLocationRange result = null;
            foreach( var r in multi )
            {
                var one = HandleSameEachNumber( r, context );
                if( one != null )
                {
                    result = result != null ? new LocationRangeCombined( result, one ) : one;
                }
            }
            return result;
        }

        private ISqlNodeLocationRange HandleSameEachNumber( ISqlNodeLocationRange mono, IVisitContextBase context )
        {
            bool starting = _eachNumber == -1;
            if( starting || mono.First.EachNumber == _eachNumber )
            {
                _eachNumber = mono.First.EachNumber;
                return HandleWithCurrentState( mono, context );
            }
            ISqlNodeLocationRange fromPrevious = null;
            if( !starting )
            {
                fromPrevious = LocalConclude( null, context );
                LocalStateReset();
            }
            _eachNumber = mono.First.EachNumber;
            var current = HandleWithCurrentState( mono, context );
            return fromPrevious != null
                    ? (current != null ? new LocationRangeCombined( fromPrevious, current ) : fromPrevious)
                    : current;
        }

        ISqlNodeLocationRange HandleWithCurrentState( ISqlNodeLocationRange inner, IVisitContextBase context )
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
                    if( _info.All )
                    {
                        return _info.Each 
                                ? ((ISqlNodeLocationRangeInternal)inner).InternalSetEachNumber()
                                : inner;
                    }
                    // ShouldHandleBasedOnMatchCount has incremented  _matchCount by inner.Count.
                    int offsetInInner = _info.Offset - _matchCount + inner.Count;
                    return offsetInInner == 0 ? inner.First : inner.ElementAt( offsetInInner );
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
                monitor.Error().Send( $"Too many matches found for '{_inner.Inner.ToString()}' (max is {_info.ExpectedMatchCount})." );
                _hasError = true;
            }
            else if( _info.All || !_info.FromFirst || _matchCount >= _info.Offset + 1 )
            {
                return true;
            }
            return false;
        }

        public override string ToString() => $"(cardinality '{_info.ToString()}' on {_inner.Inner})";


    }


}
