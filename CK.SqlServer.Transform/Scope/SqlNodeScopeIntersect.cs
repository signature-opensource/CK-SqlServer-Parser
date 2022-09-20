using CK.Core;
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
            Throw.CheckNotNullArgument( left );
            Throw.CheckNotNullArgument( right );
            _left = left.GetSafeBuilder();
            _right = right.GetSafeBuilder();
            _state = new RangeIntersector( true );
        }

        private protected override SqlNodeScopeBuilder Clone() => new SqlNodeScopeIntersect( _left, _right );

        private protected override void DoReset()
        {
            _left.Reset();
            _right.Reset();
            _state.Reset();
        }

        private protected override ISqlNodeLocationRange DoEnter( IVisitContext context )
        {
            var l = _left.Enter( context );
            var r = _right.Enter( context );
            var f = StateIntersect( l, r ); 
            ActivityMonitor.StaticLogger.Debug( $"Intersect Enter: {l}, {r} => {f}" );
            return f;
        }

        private protected override ISqlNodeLocationRange DoLeave( IVisitContext context )
        {
            var l = _left.Leave( context );
            var r = _right.Leave( context );
            var f = StateIntersect( l, r );
            ActivityMonitor.StaticLogger.Debug( $"Intersect Enter: {l}, {r} => {f}" );
            return f;
        }

        private protected override ISqlNodeLocationRange DoConclude( IVisitContextBase context )
        {
            var l = _left.Conclude( context );
            var r = _right.Conclude( context );
            var f = StateIntersect( l, r );
            ActivityMonitor.StaticLogger.Debug( $"Intersect Conclude: {l}, {r} => {f}" );
            return f;
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

        /// <summary>
        /// Overridden to return a description of this builder.
        /// </summary>
        /// <returns>The intersect description.</returns>
        public override string ToString() => $"({_left} intersect {_right})";
    }

}
