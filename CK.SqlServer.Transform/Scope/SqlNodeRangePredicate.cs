using CK.SqlServer.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Transform
{

    /// <summary>
    /// Builds ranges based on a node predicate.
    /// </summary>
    public class SqlNodeRangePredicate : SqlNodeRangeBuilder
    {
        readonly Func<ISqlNode,bool> _predicate;
        readonly int _maxOccur;
        SqlNodeLocationRange _current;
        ISqlNode _currentNode;
        int _currentRemainder;

        public SqlNodeRangePredicate( Func<ISqlNode,bool> predicate, int maxOccur = -1 )
        {
            if( predicate == null ) throw new ArgumentNullException( nameof( predicate ) );
            if( maxOccur == 0 ) throw new ArgumentException( "Must not be zero.", nameof( maxOccur ) );
            _predicate = predicate;
            _currentRemainder = _maxOccur = maxOccur;
        }

        protected override void DoReset()
        {
            _current = null;
            _currentNode = null;
            _currentRemainder = _maxOccur;
        }

        protected override ISqlNodeLocationRange DoEnter( SqlNodeLocationVisitor.IVisitContext context )
        {
            if( _current == null && _currentRemainder > 0 && _predicate( context.VisitedNode ) )
            {
                --_currentRemainder;
                _currentNode = context.VisitedNode;
                var beg = context.GetCurrentLocation();
                _current = new SqlNodeLocationRange( beg, context.LocationManager.GetRawLocation( beg.Position + _currentNode.Width ) );
                return _current;
            }
            return null;
        }

        protected override ISqlNodeLocationRange DoLeave( SqlNodeLocationVisitor.IVisitContext context )
        {
            if( _currentNode != null && _currentNode == context.VisitedNode )
            {
                _current = null;
                _currentNode = null;
            }
            return null;
        }

        protected override ISqlNodeLocationRange DoConclude( ISqlNodeLocationManager locManager )
        {
            return null;
        }
    }


}
