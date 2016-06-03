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
        readonly Func<ISqlNode,int> _matcher;
        SqlNodeLocationRange _last;

        public SqlNodeScopeDepthPredicate( Func<ISqlNode, int> matcher )
            : base( false )
        {
            if( matcher == null ) throw new ArgumentNullException( nameof( matcher ) );
            _matcher = matcher;
        }

        public SqlNodeScopeDepthPredicate( Func<ISqlNode, bool> predicate )
            : this( n => predicate(n) ? n.Width : 0 )
        {
        }

        protected override void DoReset()
        {
            _last = null;
        }

        protected override ISqlNodeLocationRange DoEnter( IVisitContext context )
        {
            return null;
        }

        protected override ISqlNodeLocationRange DoLeave( IVisitContext context )
        {
            int width;
            if( (_last == null || _last.End.Position <= context.Position) 
                && (width = _matcher( context.VisitedNode )) > 0 )
            {
                var beg = context.GetCurrentLocation( true );
                return _last = new SqlNodeLocationRange( beg, context.LocationManager.GetRawLocation( beg.Position + width ) );
            }
            return null;
        }

        protected override ISqlNodeLocationRange DoConclude( IVisitContextBase context )
        {
            return null;
        }
    }


}
