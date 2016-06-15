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
                var beg = context.GetCurrentLocation( true );
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
