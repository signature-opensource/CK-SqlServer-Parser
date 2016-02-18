using CK.SqlServer.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Transform
{

    /// <summary>
    /// Builds scopes based on a node predicate.
    /// </summary>
    public class SqlNodeScopePredicate : SqlNodeScopeBuilder
    {
        readonly Func<ISqlNode,bool> _predicate;
        readonly int _maxOccur;
        SqlNodeLocationRange _current;
        ISqlNode _currentNode;
        int _currentRemainder;

        public SqlNodeScopePredicate( Func<ISqlNode,bool> predicate, int maxOccur = -1 )
        {
            if( predicate == null ) throw new ArgumentNullException( nameof( predicate ) );
            if( maxOccur == 0 ) throw new ArgumentException( "Must not be zero.", nameof( maxOccur ) );
            _predicate = predicate;
            _currentRemainder = _maxOccur = maxOccur > 0 ? maxOccur : int.MaxValue;
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
                _current = new SqlNodeLocationRange( beg, context.LocationManager.GetRawLocation( beg.Position + _currentNode.Width + 1 ) );
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
