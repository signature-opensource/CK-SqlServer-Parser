using CK.Core;
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
    /// Checks the number of ranges.
    /// </summary>
    public sealed class SqlNodeScopeCounter : SqlNodeScopeBuilder
    {
        readonly SqlNodeScopeBuilder _inner;
        readonly int _minCount;
        readonly int _maxCount;
        int _currentCount;

        public SqlNodeScopeCounter( SqlNodeScopeBuilder inner, int minCount = 0, int maxCount = -1 )
        {
            if( inner == null ) throw new ArgumentNullException( nameof( inner ) );
            _inner = inner;
            _minCount = minCount;
            _maxCount = maxCount < 0 ? int.MaxValue : maxCount;
       }

        protected override void DoReset()
        {
            _inner.Reset();
            _currentCount = 0;
        }

        protected override ISqlNodeLocationRange DoEnter( SqlNodeLocationVisitor.IVisitContext context )
        {
            return Handle( _inner.Enter( context ), context );
        }

        protected override ISqlNodeLocationRange DoLeave( SqlNodeLocationVisitor.IVisitContext context )
        {
            return Handle( _inner.Leave( context ), context );
        }

        protected override ISqlNodeLocationRange DoConclude( SqlNodeLocationVisitor.IVisitContextBase context )
        {
            var r = Handle( _inner.Conclude( context ), context );
            if( _currentCount < _minCount )
            {
                context.Monitor.Error().Send( "Missing range (Min = {0}) @{1}", _minCount );
            }
            return r;
        }

        ISqlNodeLocationRange Handle( ISqlNodeLocationRange inner, SqlNodeLocationVisitor.IVisitContextBase context )
        {
            if( inner != null )
            {
                _currentCount += inner.Count;
                if( _currentCount > _maxCount )
                {
                    int delta = _currentCount - _maxCount;
                    var extra = inner.Skip( delta ).First(); 
                    context.Monitor.Error().Send( "Unexpected range (max = {0}) @{1}", _maxCount, extra.Beg );
                }
            }
            return inner;
        }

    }


}
