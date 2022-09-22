using CK.SqlServer.Parser;
using System;
using System.Reflection;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

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

        private protected override SqlNodeScopeBuilder Clone() => new SqlNodeScopeDepthPredicate( _nodeMatcher.Matcher, _nodeMatcher.IsPartMatch );

        private protected override void DoReset()
        {
            _nodeMatcher.Reset();
        }

        private protected override ISqlNodeLocationRange DoEnter( IVisitContext context )
        {
            _nodeMatcher.OnBeforeVisitItem( context );
            return null;
        }

        private protected override ISqlNodeLocationRange DoLeave( IVisitContext context )
        {
            if( _nodeMatcher.Match( context, context.VisitedNode ) )
            {
                var beg = context.GetCurrentLocation();
                return new SqlNodeLocationRange( beg, context.LocationManager.GetRawLocation( beg.Position + context.VisitedNode.Width ) );
            }
            return null;
        }

        private protected override ISqlNodeLocationRange DoConclude( IVisitContextBase context )
        {
            return null;
        }

        /// <summary>
        /// Overridden to return the description of this predicate.
        /// </summary>
        /// <returns>A description.</returns>
        public override string ToString() => _nodeMatcher.IsPartMatch ? "(depth-first part match)" : "(depth-first node match)";

    }


}
