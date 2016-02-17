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
    public class SqlNodeScopeIntersect : SqlNodeScopeBuilder
    {
        readonly SqlNodeScopeBuilder _left;
        readonly SqlNodeScopeBuilder _right;
        readonly List<SqlNodeLocationRange> _buffer;
        readonly RangeEnumerator _leftE;
        readonly RangeEnumerator _rightE;

        public SqlNodeScopeIntersect( SqlNodeScopeBuilder left, SqlNodeScopeBuilder right )
        {
            if( left == null ) throw new ArgumentNullException( nameof( left ) );
            if( right == null ) throw new ArgumentNullException( nameof( right ) );
            _left = left;
            _right = right;
            _buffer = new List<SqlNodeLocationRange>();
            _leftE = new RangeEnumerator();
            _rightE = new RangeEnumerator();
        }

        protected override void DoReset()
        {
            _left.Reset();
            _right.Reset();
            _leftE.Reset();
            _rightE.Reset();
            _buffer.Clear();
        }

        protected override ISqlNodeLocationRange DoEnter( SqlNodeLocationVisitor.IVisitContext context )
        {
            return Handle( _left.Enter( context ), _right.Enter( context ) );
        }

        protected override ISqlNodeLocationRange DoLeave( SqlNodeLocationVisitor.IVisitContext context )
        {
            return Handle( _left.Leave( context ), _right.Leave( context ) );
        }

        protected override ISqlNodeLocationRange DoConclude( ISqlNodeLocationManager locManager )
        {
            return Handle( _left.Conclude( locManager ), _right.Conclude( locManager ) );
        }

        ISqlNodeLocationRange Handle( ISqlNodeLocationRange left, ISqlNodeLocationRange right )
        {
            _leftE.Add( left );
            _rightE.Add( right );
            OnTheFlyIntersect( _leftE, _rightE, _buffer.Add );
            if( _buffer.Count > 0 )
            {
                var r = _buffer.ToArray();
                _buffer.Clear();
                return new LocationRangeList( r );
            }
            return null;
        }

    }


}
