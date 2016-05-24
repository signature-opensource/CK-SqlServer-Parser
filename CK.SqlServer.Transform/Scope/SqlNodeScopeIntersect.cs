using CK.SqlServer.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            public ISqlNodeLocationRange DoIntesect( ISqlNodeLocationRange left, ISqlNodeLocationRange right )
            {
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

        protected override ISqlNodeLocationRange DoEnter( SqlNodeLocationVisitor.IVisitContext context )
        {
            return _state.DoIntesect( _left.Enter( context ), _right.Enter( context ) );
        }

        protected override ISqlNodeLocationRange DoLeave( SqlNodeLocationVisitor.IVisitContext context )
        {
            return _state.DoIntesect( _left.Leave( context ), _right.Leave( context ) );
        }

        protected override ISqlNodeLocationRange DoConclude( SqlNodeLocationVisitor.IVisitContextBase context )
        {
            return _state.DoIntesect( _left.Conclude( context ), _right.Conclude( context ) );
        }

        internal static ISqlNodeLocationRange DoIntersect( ISqlNodeLocationRange left, ISqlNodeLocationRange right )
        {
            return new RangeIntersector( true ).DoIntesect( left, right ) ?? SqlNodeLocationRange.EmptySet;
        }

    }

}
