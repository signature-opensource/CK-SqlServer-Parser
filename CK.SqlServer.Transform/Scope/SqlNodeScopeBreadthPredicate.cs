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
    /// Builds scopes based on a node predicate.
    /// </summary>
    public sealed class SqlNodeScopeBreadthPredicate : SqlNodeScopeBuilder
    {
        readonly Func<ISqlNode,bool> _predicate;
        readonly int _maxOccur;
        SqlNodeLocationRange _current;
        int _currentRemainder;

        public SqlNodeScopeBreadthPredicate( Func<ISqlNode,bool> predicate, int maxOccur = -1 )
            : base( false )
        {
            if( predicate == null ) throw new ArgumentNullException( nameof( predicate ) );
            if( maxOccur == 0 ) throw new ArgumentException( "Must not be zero.", nameof( maxOccur ) );
            _predicate = predicate;
            _maxOccur = maxOccur > 0 ? maxOccur : int.MaxValue;
            _currentRemainder = _maxOccur;
        }

        protected override void DoReset()
        {
            _current = null;
            _currentRemainder = _maxOccur;
        }

        protected override ISqlNodeLocationRange DoEnter( SqlNodeLocationVisitor.IVisitContext context )
        {
            if( _current == null && _currentRemainder > 0 && _predicate( context.VisitedNode ) )
            {
                --_currentRemainder;
                var beg = context.GetCurrentLocation( true );
                Debug.Assert( beg.Node == context.VisitedNode );
                return _current = new SqlNodeLocationRange( beg, context.LocationManager.GetRawLocation( beg.Position + context.VisitedNode.Width ) );
            }
            return null;
        }

        protected override ISqlNodeLocationRange DoLeave( SqlNodeLocationVisitor.IVisitContext context )
        {
            if( _current != null && _current.Beg.Node == context.VisitedNode )
            {
                _current = null;
            }
            return null;
        }

        protected override ISqlNodeLocationRange DoConclude( SqlNodeLocationVisitor.IVisitContextBase context )
        {
            return null;
        }
    }


}
