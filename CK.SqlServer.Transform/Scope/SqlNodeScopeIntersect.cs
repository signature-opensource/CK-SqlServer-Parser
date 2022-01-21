using System;
using System.Diagnostics;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CK.SqlServer.Transform
{

    /// <summary>
    /// Builds intersected ranges.
    /// </summary>
    public sealed class SqlNodeScopeIntersect : SqlNodeScopeBuilder
    {
        readonly SqlNodeScopeBuilder _left;
        readonly SqlNodeScopeBuilder _right;
        readonly RangeIntersector _state;

        struct RangeIntersector
        {
            readonly RangeBuffer _buffer;
            RangeEnumerator _leftE;
            RangeEnumerator _rightE;

            public RangeIntersector( bool onlyCtor )
            {
                _buffer = new RangeBuffer( true );
                _leftE = new RangeEnumerator();
                _rightE = new RangeEnumerator();
            }

            public void Reset()
            {
                _leftE.Reset();
                _rightE.Reset();
                _buffer.Reset();
            }

            public ISqlNodeLocationRange DoIntersect( ISqlNodeLocationRange left, ISqlNodeLocationRange right )
            {
                Debug.Assert( left != null || right != null );
                _leftE = _leftE.Add( left );
                _rightE = _rightE.Add( right );
                while( _leftE.HasMore && _rightE.HasMore )
                {
                    SqlNodeLocationRange l = _leftE.Current.Intersect( _rightE.Current );
                    if( l != SqlNodeLocationRange.EmptySet ) _buffer.AddResult( l );
                    bool forward1 = _leftE.Current.End.Position <= _rightE.Current.End.Position;
                    bool forward2 = _leftE.Current.End.Position >= _rightE.Current.End.Position;
                    if( forward1 ) _leftE.MoveNext();
                    if( forward2 ) _rightE.MoveNext();
                }
                return _buffer.ExtractResult();
            }

        }

        public SqlNodeScopeIntersect( SqlNodeScopeBuilder left, SqlNodeScopeBuilder right )
        {
            if( left == null ) throw new ArgumentNullException( nameof( left ) );
            if( right == null ) throw new ArgumentNullException( nameof( right ) );
            _left = left;
            _right = right;
            _state = new RangeIntersector( true );
        }

        protected override void DoReset()
        {
            _left.Reset();
            _right.Reset();
            _state.Reset();
        }

        protected override ISqlNodeLocationRange DoEnter( IVisitContext context )
        {
            return StateIntersect( _left.Enter( context ), _right.Enter( context ) );
        }

        protected override ISqlNodeLocationRange DoLeave( IVisitContext context )
        {
            return StateIntersect( _left.Leave( context ), _right.Leave( context ) );
        }

        protected override ISqlNodeLocationRange DoConclude( IVisitContextBase context )
        {
            return StateIntersect( _left.Conclude( context ), _right.Conclude( context ) );
        }

        ISqlNodeLocationRange StateIntersect( ISqlNodeLocationRange left, ISqlNodeLocationRange right )
        {
            return left != null || right != null
                    ? _state.DoIntersect( left, right )
                    : null;
        }

        internal static ISqlNodeLocationRange DoIntersect( ISqlNodeLocationRange left, ISqlNodeLocationRange right )
        {
            return left == null || left == SqlNodeLocationRange.EmptySet || right == null || right == SqlNodeLocationRange.EmptySet
                    ? SqlNodeLocationRange.EmptySet
                    : new RangeIntersector( true ).DoIntersect( left, right ) ?? SqlNodeLocationRange.EmptySet;
        }

        public override string ToString() => $"({_left} intersect {_right})";
    }

}
