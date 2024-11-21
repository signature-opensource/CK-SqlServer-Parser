using CK.Core;
using System;
using System.Diagnostics;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CK.SqlServer.Transform
{

    /// <summary>
    /// Builds intersected ranges.
    /// </summary>
    public sealed class SqlNodeScopeExcept : SqlNodeScopeBuilder
    {
        readonly SqlNodeScopeBuilder _left;
        readonly SqlNodeScopeBuilder _right;
        readonly RangeExceptor _exceptor;

        class RangeExceptor
        {
            readonly BiRangeState _state;
            SqlNodeLocationRange _current;

            public RangeExceptor()
            {
                _state = new BiRangeState( true );
                _current = null;
            }

            public void Reset()
            {
                _state.Reset();
                _current = null;
            }

            public ISqlNodeLocationRange DoExcept( ISqlNodeLocationRange left, ISqlNodeLocationRange right, bool conclude )
            {
                _state.AddInputRanges( left, right );
                if( _current == null )
                {
                    if( !_state.LeftE.HasMore ) return null;
                    _current = _state.LeftE.Current;
                }
                Debug.Assert( _current != null );
                while( _current != null && _state.RightE.HasMore )
                {
                    SqlNodeLocationRange.Unified( _current, _state.RightE.Current, ProcessCurrent );
                }
                Debug.Assert( _current == null || _state.RightE.IsEmpty );
                if( conclude )
                {
                    _state.FlushLeft( ref _current );
                }
                return _state.ExtractResult();
            }

            ISqlNodeLocationRange ProcessCurrent( SqlNodeLocationRange.Kind k, SqlNodeLocationRange r1, SqlNodeLocationRange r2 )
            {
                Debug.Assert( _current != null );
                Debug.Assert( (_current == r1 && (k & SqlNodeLocationRange.Kind.Swapped) == 0) || (_current == r2 && (k & SqlNodeLocationRange.Kind.Swapped) != 0) );
                switch( k )
                {
                    case SqlNodeLocationRange.Kind.Equal:
                    case SqlNodeLocationRange.Kind.Contained | SqlNodeLocationRange.Kind.Swapped:
                    case SqlNodeLocationRange.Kind.SameEnd | SqlNodeLocationRange.Kind.Swapped:
                    {
                        _state.RightE.MoveNext();
                        _current = _state.LeftE.MoveNext() ? _state.LeftE.Current : null;
                        return null;
                    }
                    case SqlNodeLocationRange.Kind.Congruent:
                    case SqlNodeLocationRange.Kind.Independent:
                    {
                        _state.AddResult( _current );
                        _state.ForwardLeftUntil( r2.Beg.Position );
                        _current = _state.LeftE.HasMore ? _state.LeftE.Current : null;
                        return null;
                    }
                    case SqlNodeLocationRange.Kind.Congruent | SqlNodeLocationRange.Kind.Swapped:
                    case SqlNodeLocationRange.Kind.Independent | SqlNodeLocationRange.Kind.Swapped:
                    {
                        _state.ForwardRightUntil( r2.Beg.Position );
                        return null;
                    }
                    case SqlNodeLocationRange.Kind.Contained:
                    {
                        _state.AddResult( new SqlNodeLocationRange( r1.Beg, r2.Beg ) );
                        _current = _current.InternalSetBeg( r2.End );
                        _state.RightE.MoveNext();
                        return null;
                    }

                    case SqlNodeLocationRange.Kind.SameStart:
                    {
                        _state.ForwardLeftUntil( r2.End.Position );
                        _current = _state.LeftE.HasMore ? _state.LeftE.Current : null;
                        return null;
                    }
                    case SqlNodeLocationRange.Kind.SameStart | SqlNodeLocationRange.Kind.Swapped:
                    {
                        _current = _current.InternalSetBeg( r1.End );
                        _state.ForwardRightUntil( r2.End.Position );
                        return null;
                    }
                    case SqlNodeLocationRange.Kind.Overlapped:
                    case SqlNodeLocationRange.Kind.SameEnd:
                    {
                        _current = _current.InternalSetEnd( r2.Beg );
                        _state.AddResult( _current );
                        _state.ForwardLeftUntil( r2.End.Position );
                        _current = _state.LeftE.HasMore ? _state.LeftE.Current : null;
                        return null;
                    }
                    case SqlNodeLocationRange.Kind.Overlapped | SqlNodeLocationRange.Kind.Swapped:
                    {
                        _current = _current.InternalSetBeg( r1.End );
                        _state.RightE.MoveNext();
                        return null;
                    }
                }
                throw new NotImplementedException();
            }
        }

        public SqlNodeScopeExcept( SqlNodeScopeBuilder left, SqlNodeScopeBuilder right )
        {
            Throw.CheckNotNullArgument( left );
            Throw.CheckNotNullArgument( right );
            _left = left.GetSafeBuilder();
            _right = right.GetSafeBuilder();
            _exceptor = new RangeExceptor();
        }

        private protected override void DoReset()
        {
            _left.Reset();
            _right.Reset();
            _exceptor.Reset();
        }

        private protected override SqlNodeScopeBuilder Clone() => new SqlNodeScopeExcept( _left, _right );

        private protected override ISqlNodeLocationRange DoEnter( IVisitContext context )
        {
            return Handle( _left.Enter( context ), _right.Enter( context ) );
        }

        private protected override ISqlNodeLocationRange DoLeave( IVisitContext context )
        {
            return Handle( _left.Leave( context ), _right.Leave( context ) );
        }

        private protected override ISqlNodeLocationRange DoConclude( IVisitContextBase context )
        {
            return Handle( _left.Conclude( context ), _right.Conclude( context ), true );
        }

        ISqlNodeLocationRange Handle( ISqlNodeLocationRange left, ISqlNodeLocationRange right, bool conclude = false )
        {
            if( left != null || right != null || conclude )
            {
                return _exceptor.DoExcept( left, right, conclude );
            }
            return null;
        }

        internal static ISqlNodeLocationRange DoExcept( ISqlNodeLocationRange left, ISqlNodeLocationRange right )
        {
            return new RangeExceptor().DoExcept( left, right, true ) ?? SqlNodeLocationRange.EmptySet;
        }

        /// <summary>
        /// Overridden to return a description of this builder.
        /// </summary>
        /// <returns>The except description.</returns>
        public override string ToString() => $"({_left} except {_right})";

    }


}
