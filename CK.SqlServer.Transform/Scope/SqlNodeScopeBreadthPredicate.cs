using CK.SqlServer.Parser;
using System;
using System.Diagnostics;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CK.SqlServer.Transform
{

    /// <summary>
    /// Builds scopes based on a node predicate. This is a breadth-first matcher: as soon as a node match,
    /// none of its children will match.
    /// </summary>
    public sealed class SqlNodeScopeBreadthPredicate : SqlNodeScopeBuilder
    {
        readonly Func<ISqlNode,bool> _predicate;
        SqlNodeLocationRange _current;

        public SqlNodeScopeBreadthPredicate( Func<ISqlNode,bool> predicate )
        {
            if( predicate == null ) throw new ArgumentNullException( nameof( predicate ) );
            _predicate = predicate;
        }

        protected override void DoReset()
        {
            _current = null;
        }

        protected override ISqlNodeLocationRange DoEnter( IVisitContext context )
        {
            if( _current == null 
                && context.RangeFilterStatus.IsIncludedInFilteredRange() 
                && _predicate( context.VisitedNode ) )
            {
                var beg = context.GetCurrentLocation();
                Debug.Assert( beg.Node == context.VisitedNode );
                return _current = new SqlNodeLocationRange( beg, context.LocationManager.GetRawLocation( beg.Position + context.VisitedNode.Width ) );
            }
            return null;
        }

        protected override ISqlNodeLocationRange DoLeave( IVisitContext context )
        {
            if( _current != null && _current.Beg.Node == context.VisitedNode )
            {
                _current = null;
            }
            return null;
        }

        protected override ISqlNodeLocationRange DoConclude( IVisitContextBase context )
        {
            return null;
        }

        public override string ToString() => "(breadth-first node match)";

    }


}
