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
    /// Builds intersected ranges.
    /// </summary>
    public class SqlNodeScopeUnion : SqlNodeScopeBuilder
    {
        readonly SqlNodeScopeBuilder _left;
        readonly SqlNodeScopeBuilder _right;
        readonly RangeUnioner _unioner;

        class RangeUnioner
        {
            readonly BiRangeState _state;
            SqlNodeLocationRange _current;

            public RangeUnioner()
            {
                _state = new BiRangeState( true );
                _current = null;
            }

            public void Reset()
            {
                _state.Reset();
                _current = null;
            }

            public ISqlNodeLocationRange DoUnion( ISqlNodeLocationRange left, ISqlNodeLocationRange right, bool conclude )
            {
                _state.AddInputRanges( left, right );
                if( _current != null )
                {
                    _state.ForwardLeftUntil( _current.End.Position );
                    _state.ForwardRightUntil( _current.End.Position );
                }
                while( _state.BothHaveMore )
                {
                    SqlNodeLocationRange.Unified( _state.LeftE.Current, _state.RightE.Current, ProcessCurrent );
                }
                Debug.Assert( _state.LeftE.IsEmpty || _state.RightE.IsEmpty );
                if( conclude )
                {
                    if( _state.LeftE.HasMore ) _state.FlushLeft( ref _current );
                    else if( _state.RightE.HasMore ) _state.FlushLeft( ref _current, swapped: true );
                    else
                    {
                        if( _current != null ) _state.AddResult( _current );
                        _current = null;
                    }
                    Debug.Assert( _current == null );
                }
                return _state.ExtractResult();
            }

            ISqlNodeLocationRange ProcessCurrent( SqlNodeLocationRange.Kind k, SqlNodeLocationRange r1, SqlNodeLocationRange r2 )
            {
                switch( k & ~SqlNodeLocationRange.Kind.Swapped )
                {
                    case SqlNodeLocationRange.Kind.Equal:
                        {
                            _state.MoveBoth();
                            return HandleUnioned( r1.MostPrecise( r2 ) );
                        }
                    case SqlNodeLocationRange.Kind.Independent:
                        {
                            _state.MoveLeft( (k & SqlNodeLocationRange.Kind.Swapped) != 0 );
                            return HandleUnioned( r1 );
                        }
                    case SqlNodeLocationRange.Kind.Contained:
                        {
                            _state.MoveLeftOnceAndRightUntil( r1.End.Position, (k & SqlNodeLocationRange.Kind.Swapped) != 0 );
                            return HandleUnioned( r1 );
                        }
                    case SqlNodeLocationRange.Kind.SameStart:
                        {
                            // Inverts left and right here.
                            _state.MoveLeftOnceAndRightUntil( r2.End.Position, (k & SqlNodeLocationRange.Kind.Swapped) == 0 );
                            return HandleUnioned( new SqlNodeLocationRange( r1.Beg.MostPrecise( r2.Beg ), r2.End ) );
                        }
                    case SqlNodeLocationRange.Kind.SameEnd:
                        {
                            _state.MoveBoth();
                            return HandleUnioned( new SqlNodeLocationRange( r1.Beg, r1.End.MostPrecise( r2.End ) ) );
                        }
                    case SqlNodeLocationRange.Kind.Overlapped:
                    case SqlNodeLocationRange.Kind.Congruent:
                        {
                            // Inverts left and right here.
                            _state.MoveLeftOnceAndRightUntil( r2.End.Position, (k & SqlNodeLocationRange.Kind.Swapped) == 0 );
                            return HandleUnioned( new SqlNodeLocationRange( r1.Beg, r2.End ) );
                        }
                }
                throw new NotImplementedException();
            }

            ISqlNodeLocationRange HandleUnioned( SqlNodeLocationRange r )
            {
                Debug.Assert( r != null );
                if( _current == null ) _current = r;
                else
                {
                    if( _current.End.Position >= r.Beg.Position )
                    {
                        _current = _current.InternalSetEnd( r.End );
                    }
                    else
                    {
                        _state.AddResult( _current );
                        _current = r;
                    }
                }
                Debug.Assert( _current != null );
                return _current;
            }

        }

        public SqlNodeScopeUnion( SqlNodeScopeBuilder left, SqlNodeScopeBuilder right )
        {
            if( left == null ) throw new ArgumentNullException( nameof( left ) );
            if( right == null ) throw new ArgumentNullException( nameof( right ) );
            _left = left;
            _right = right;
            _unioner = new RangeUnioner();
        }

        protected override void DoReset()
        {
            _left.Reset();
            _right.Reset();
            _unioner.Reset();
        }

        protected override ISqlNodeLocationRange DoEnter( SqlNodeLocationVisitor.IVisitContext context )
        {
            return Handle( _left.Enter( context ), _right.Enter( context ) );
        }

        protected override ISqlNodeLocationRange DoLeave( SqlNodeLocationVisitor.IVisitContext context )
        {
            return Handle( _left.Leave( context ), _right.Leave( context ) );
        }

        protected override ISqlNodeLocationRange DoConclude( SqlNodeLocationVisitor.IVisitContextBase context )
        {
            return Handle( _left.Conclude( context ), _right.Conclude( context ), true );
        }

        ISqlNodeLocationRange Handle( ISqlNodeLocationRange left, ISqlNodeLocationRange right, bool conclude = false )
        {
            if( left != null || right != null || conclude )
            {
                return _unioner.DoUnion( left, right, conclude );
            }
            return null;
        }

        internal static ISqlNodeLocationRange DoUnion( ISqlNodeLocationRange left, ISqlNodeLocationRange right )
        {
            return new RangeUnioner().DoUnion( left, right, true ) ?? SqlNodeLocationRange.EmptySet;
        }

    }


}
