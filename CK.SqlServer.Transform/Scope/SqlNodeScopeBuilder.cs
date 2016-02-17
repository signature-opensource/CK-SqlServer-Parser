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
    /// Abstract range builder.
    /// </summary>
    public abstract class SqlNodeScopeBuilder
    {
        ISqlNodeLocationRange _last;

        /// <summary>
        /// Resets any internal state
        /// </summary>
        public void Reset()
        {
            _last = null;
        }

        internal ISqlNodeLocationRange Enter( SqlNodeLocationVisitor.IVisitContext context )
        {
            return Handle( DoEnter( context ) );
        }

        internal ISqlNodeLocationRange Leave( SqlNodeLocationVisitor.IVisitContext context )
        {
            return Handle( DoLeave( context ) );
        }

        internal ISqlNodeLocationRange Conclude( ISqlNodeLocationManager locManager )
        {
            var r1 = Handle( DoConclude( locManager ) );
            var r2 = _last;
            if( r2 != null )
            {
                _last = null;
                return r1 != null ? new LocationRangeCombined( r1, r2 ) : r2;
            }
            return r1;
        }

        /// <summary>
        /// Must reset any internal state.
        /// </summary>
        protected abstract void DoReset();

        /// <summary>
        /// Called for each node, before visiting its children. Mey return a range.
        /// </summary>
        /// <param name="context">The visited node and location manager to use.</param>
        /// <returns>Null or a range to consider.</returns>
        protected abstract ISqlNodeLocationRange DoEnter( SqlNodeLocationVisitor.IVisitContext context );

        /// <summary>
        /// Called for each node, before visiting its children. Mey return a range.
        /// </summary>
        /// <param name="context">The visited node and location manager to use.</param>
        /// <returns>Null or a range to consider.</returns>
        protected abstract ISqlNodeLocationRange DoLeave( SqlNodeLocationVisitor.IVisitContext context );

        /// <summary>
        /// Called at the end of the visit.
        /// </summary>
        /// <param name="locManager">Location manager to use.</param>
        /// <returns>Null or the final range to consider.</returns>
        protected abstract ISqlNodeLocationRange DoConclude( ISqlNodeLocationManager locManager );

        ISqlNodeLocationRange Handle( ISqlNodeLocationRange r )
        {
            if( r == null || r == SqlNodeLocationRange.Empty ) return null;
            ISqlNodeLocationRange result = _last;
            if( result != null )
            {
                var l = result.Last;
                if( l.End.Position > r.First.Beg.Position ) throw new InvalidOperationException( "Newly built range intersects previous one." );
                if( l.End.Position == r.First.Beg.Position )
                {
                    l.InternalExtend( r.Last.End );
                    return null;
                }
            }
            _last = r;
            return result;
        }

        protected class RangeEnumerator : IEnumerator<SqlNodeLocationRange>
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
            /// Appends an enumerable. Either this RangeEnumerator or a new on is returned.
            /// </summary>
            /// <param name="next">The next eanumeable. Can be null.</param>
            /// <returns>This or a one that cobines this and the next range.</returns>
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
            /// Gets whether this RangeEnumerator has a <see cref="Current"/> range.
            /// </summary>
            public bool IsEmpty => _current == null;

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

        static protected void OnTheFlyIntersect( RangeEnumerator left, RangeEnumerator right, Action<SqlNodeLocationRange> result )
        {
            for( ;;)
            {
                if( left.IsEmpty || right.IsEmpty ) return;
                SqlNodeLocationRange l = left.Current.Intersect( right.Current );
                if( l != SqlNodeLocationRange.Empty ) result( l );
                bool forward1 = left.Current.End.Position <= right.Current.End.Position;
                bool forward2 = left.Current.End.Position >= right.Current.End.Position;
                if( forward1 ) left.MoveNext();
                if( forward2 ) right.MoveNext();
            }
        }


    }


}
