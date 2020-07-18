using CK.SqlServer.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Transform
{

    /// <summary>
    /// Builds scopes based on a node predicate. This is a depth-first matcher: nodes match regardless
    /// of whether any of its parents have matched.
    /// </summary>
    public sealed class SqlNodeScopeDepthPredicate : SqlNodeScopeBuilder
    {
        readonly DepthFirstNodeMatcherHelper _nodeMatcher;

        public SqlNodeScopeDepthPredicate( Func<ISqlNode, bool> predicate, bool isPartMatch = false )
        {
            _nodeMatcher = new DepthFirstNodeMatcherHelper( isPartMatch, predicate );
        }

        protected override void DoReset()
        {
            _nodeMatcher.Reset();
        }

        protected override ISqlNodeLocationRange DoEnter( IVisitContext context )
        {
            _nodeMatcher.OnBeforeVisitItem( context );
            return null;
        }

        protected override ISqlNodeLocationRange DoLeave( IVisitContext context )
        {
            if( _nodeMatcher.Match( context, context.VisitedNode ) )
            {
                var beg = context.GetCurrentLocation();
                return new SqlNodeLocationRange( beg, context.LocationManager.GetRawLocation( beg.Position + context.VisitedNode.Width ) );
            }
            return null;
        }

        protected override ISqlNodeLocationRange DoConclude( IVisitContextBase context )
        {
            return null;
        }

        public override string ToString() => _nodeMatcher.IsPartMatch ? "(depth-first part match)" : "(depth-first node match)";

    }


}
