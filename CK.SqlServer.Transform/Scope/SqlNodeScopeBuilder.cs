using CK.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace CK.SqlServer.Transform
{

    /// <summary>
    /// Abstract range builder.
    /// </summary>
    public abstract class SqlNodeScopeBuilder
    {
        readonly bool _autoMergeContiguous;
        ISqlNodeLocationRangeInternal _last;
        bool _inUse;

        /// <summary>
        /// Initializes a new <see cref="SqlNodeScopeBuilder"/> that, by default, merges emitted 
        /// directly subsequent ranges (ie. [1,8[ and [8,12[ are merged as [1,12[). 
        /// </summary>
        /// <param name="autoMergeContiguous">True to emit merged contiguous ranges.</param>
        public SqlNodeScopeBuilder( bool autoMergeContiguous = false )
        {
            _autoMergeContiguous = autoMergeContiguous;
        }

        /// <summary>
        /// Resets any internal state
        /// </summary>
        public void Reset()
        {
            _last = null;
            DoReset();
        }

        [DebuggerStepThrough]
        internal ISqlNodeLocationRange Enter( IVisitContext context )
        {
            return Handle( (ISqlNodeLocationRangeInternal)DoEnter( context ) );
        }

        [DebuggerStepThrough]
        internal ISqlNodeLocationRange Leave( IVisitContext context )
        {
            return Handle( (ISqlNodeLocationRangeInternal)DoLeave( context ) );
        }

        internal ISqlNodeLocationRange Conclude( IVisitContextBase context )
        {
            var r1 = (ISqlNodeLocationRangeInternal)Handle( (ISqlNodeLocationRangeInternal)DoConclude( context ) );
            var r2 = _last;
            if( r2 != null )
            {
                _last = null;
                return r1 != null ? new LocationRangeCombined( r1, r2 ) : r2;
            }
            return r1;
        }

        internal SqlNodeScopeBuilder GetSafeBuilder()
        {
            if( !_inUse )
            {
                _inUse = true;
                return this;
            }
            return Clone();
        }

        /// <summary>
        /// Must reset any internal state.
        /// </summary>
        private protected abstract void DoReset();

        /// <summary>
        /// Must provide a clone of this builder.
        /// </summary>
        private protected abstract SqlNodeScopeBuilder Clone();

        /// <summary>
        /// Called for each node, before visiting its children. May return a range.
        /// </summary>
        /// <param name="context">The visited node and location manager to use.</param>
        /// <returns>Null or a range to consider.</returns>
        private protected abstract ISqlNodeLocationRange DoEnter( IVisitContext context );

        /// <summary>
        /// Called for each node, before visiting its children. May return a range.
        /// </summary>
        /// <param name="context">The visited node and location manager to use.</param>
        /// <returns>Null or a range to consider.</returns>
        private protected abstract ISqlNodeLocationRange DoLeave( IVisitContext context );

        /// <summary>
        /// Called at the end of the visit.
        /// </summary>
        /// <param name="context">Base context (offers location manager and error management).</param>
        /// <returns>Null or the final range to consider.</returns>
        private protected abstract ISqlNodeLocationRange DoConclude( IVisitContextBase context );

        ISqlNodeLocationRange Handle( ISqlNodeLocationRangeInternal r )
        {
            if( r == null || r == SqlNodeLocationRange.EmptySet ) return null;

            ISqlNodeLocationRangeInternal result = _last;
            if( result != null )
            {
                var l = result.Last;
                Throw.CheckState( "Newly built range intersects previous one.", l.End.Position <= r.First.Beg.Position );

                if( _autoMergeContiguous && l.End.Position == r.First.Beg.Position )
                {
                    _last = _last.InternalSetEnd( r.Last.End );
                    return null;
                }
            }
            _last = r;
            return result;
        }

        private protected class RangeEnumerator : IEnumerator<SqlNodeLocationRange>
        {
            IEnumerator<SqlNodeLocationRange> _current;
            IEnumerable<SqlNodeLocationRange> _next;

            /// <summary>
            /// Initializes a new RangeEnumerator.
            /// </summary>
            public RangeEnumerator()
            {
            }

            RangeEnumerator( IEnumerator<SqlNodeLocationRange> current, IEnumerable<SqlNodeLocationRange> next )
            {
                _current = current;
                _next = next;
            }

            /// <summary>
            /// Appends an enumerable. Either this RangeEnumerator or a new one is returned.
            /// </summary>
            /// <param name="next">The next enumerable. Can be null.</param>
            /// <returns>This or a new one that combines this and the next range.</returns>
            public RangeEnumerator Add( IEnumerable<SqlNodeLocationRange> next )
            {
                if( next == null ) return this;
                if( _current == null )
                {
                    Debug.Assert( _next == null );
                    _current = next.GetEnumerator();
                    if( !_current.MoveNext() )
                    {
                        _current = null;
                    }
                    return this;
                }
                if( _next == null )
                {
                    _next = next;
                    return this;
                }
                return new RangeEnumerator( this, next );
            }

            /// <summary>
            /// Gets whether this RangeEnumerator has no more <see cref="Current"/> range.
            /// </summary>
            public bool IsEmpty => _current == null;

            /// <summary>
            /// Gets whether this RangeEnumerator has a <see cref="Current"/> range.
            /// </summary>
            public bool HasMore => _current != null;

            /// <summary>
            /// Gets the current range.
            /// </summary>
            public SqlNodeLocationRange Current => _current.Current;

            /// <summary>
            /// Moves to the next range if possible.
            /// </summary>
            /// <returns>True if the move succeeded (<see cref="IsEmpty"/> is false).</returns>
            public bool MoveNext()
            {
                if( _current == null || !_current.MoveNext() )
                {
                    if( _next == null )
                    {
                        _current = null;
                        return false;
                    }
                    _current = _next.GetEnumerator();
                    _next = null;
                    if( !_current.MoveNext() )
                    {
                        _current = null;
                        return false;
                    }
                }
                return true;
            }

            /// <summary>
            /// Resets the current ranges.
            /// </summary>
            public void Reset()
            {
                _current = null;
                _next = null;
            }

            object IEnumerator.Current => _current.Current;

            void IDisposable.Dispose()
            {
            }

        }

        private protected readonly struct RangeBuffer
        {
            readonly List<SqlNodeLocationRange> _buffer;

            public RangeBuffer( bool onlyCtor )
            {
                _buffer = new List<SqlNodeLocationRange>();
            }

            public void Reset() => _buffer.Clear();

            public void AddResult( SqlNodeLocationRange r ) => _buffer.Add( r );

            public ISqlNodeLocationRange ExtractResult()
            {
                ISqlNodeLocationRange r = SqlNodeLocationRange.EmptySet;
                if( _buffer.Count > 0 )
                {
                    r = SqlNodeLocationRange.Create( _buffer, _buffer.Count, true );
                    _buffer.Clear();
                }
                return r;
            }
        }

        /// <summary>
        /// Helper class that factorizes code between union and except implementations.
        /// </summary>
        private protected struct BiRangeState
        {
            readonly RangeBuffer _buffer;
            public RangeEnumerator LeftE { get; private set; }
            public RangeEnumerator RightE { get; private set; }

            public BiRangeState( bool onlyCtor )
            {
                LeftE = new RangeEnumerator();
                RightE = new RangeEnumerator();
                _buffer = new RangeBuffer( true );
            }

            public void Reset()
            {
                LeftE.Reset();
                RightE.Reset();
                _buffer.Reset();
            }

            public void AddInputRanges( ISqlNodeLocationRange left, ISqlNodeLocationRange right )
            {
                LeftE = LeftE.Add( left );
                RightE = RightE.Add( right );
            }

            public bool BothHaveMore => LeftE.HasMore && RightE.HasMore;

            public void ForwardLeftUntil( int position )
            {
                if( LeftE.HasMore ) while( LeftE.Current.End.Position <= position && LeftE.MoveNext() ) ;
            }

            public void ForwardRightUntil( int position )
            {
                if( RightE.HasMore ) while( RightE.Current.End.Position <= position && RightE.MoveNext() ) ;
            }

            public void MoveLeftOnceAndRightUntil( int position, bool swapped )
            {
                if( swapped )
                {
                    RightE.MoveNext();
                    while( LeftE.MoveNext() && LeftE.Current.End.Position <= position ) ;
                }
                else
                {
                    LeftE.MoveNext();
                    while( RightE.MoveNext() && RightE.Current.End.Position <= position ) ;
                }
            }

            public void MoveLeft( bool swapped )
            {
                if( swapped )
                {
                    RightE.MoveNext();
                }
                else
                {
                    LeftE.MoveNext();
                }
            }

            public void MoveBoth()
            {
                LeftE.MoveNext();
                RightE.MoveNext();
            }

            public void AddResult( SqlNodeLocationRange r ) => _buffer.AddResult( r );

            public ISqlNodeLocationRange ExtractResult() => _buffer.ExtractResult();

            /// <summary>
            /// Unconditionnaly flushes left ranges (or right if swapped is true) to the results, combining
            /// the remaining ranges with a current one that will be null after this.
            /// </summary>
            /// <param name="current">A current range that will be extended and emitted if not null.</param>
            /// <param name="swapped">True to flush right instead of left.</param>
            public void FlushLeft( ref SqlNodeLocationRange current, bool swapped = false )
            {
                RangeEnumerator e = swapped ? RightE : LeftE;
                if( current != null )
                {
                    while( current.End.Position >= e.Current.Beg.Position )
                    {
                        if( e.Current.End.Position > current.End.Position ) current = current.InternalSetEnd( e.Current.End );
                        if( !e.MoveNext() ) break;
                    }
                    AddResult( current );
                    current = null;
                }
                if( e.HasMore )
                {
                    do
                    {
                        AddResult( e.Current );
                    }
                    while( e.MoveNext() );
                }
            }


        }


    }


}
