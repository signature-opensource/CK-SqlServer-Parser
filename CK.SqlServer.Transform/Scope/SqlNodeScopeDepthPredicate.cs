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
    public sealed class SqlNodeScopeDepthPredicate : SqlNodeScopeBuilder
    {
        readonly Func<ISqlNode,bool> _predicate;
        readonly int _maxOccur;
        SqlNodeLocationRange _last;
        int _currentRemainder;

        public SqlNodeScopeDepthPredicate( Func<ISqlNode,bool> predicate, int maxOccur = -1 )
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
            _last = null;
            _currentRemainder = _maxOccur;
        }

        protected override ISqlNodeLocationRange DoEnter( SqlNodeLocationVisitor.IVisitContext context )
        {
            return null;
        }

        protected override ISqlNodeLocationRange DoLeave( SqlNodeLocationVisitor.IVisitContext context )
        {
            if( CheckCanMatch( context.Position ) && _predicate( context.VisitedNode ) )
            {
                --_currentRemainder;
                var beg = context.GetCurrentLocation( true );
                return _last = new SqlNodeLocationRange( beg, context.LocationManager.GetRawLocation( beg.Position + context.VisitedNode.Width ) );
            }
            return null;
        }

        bool CheckCanMatch( int position )
        {
            return _currentRemainder > 0 && (_last == null || _last.End.Position <= position);
        }

        protected override ISqlNodeLocationRange DoConclude( SqlNodeLocationVisitor.IVisitContextBase context )
        {
            return null;
        }
    }


}
