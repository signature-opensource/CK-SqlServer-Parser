using System;
using System.Collections.Generic;
using System.Linq;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CK.SqlServer.Transform
{

    /// <summary>
    /// Filters a subset of ranges based on an index and an a count.
    /// </summary>
    public sealed class SqlNodeScopeIndex : SqlNodeScopeBuilder
    {
        readonly SqlNodeScopeBuilder _inner;
        readonly int _start;
        readonly int _count;
        readonly int _stop;
        int _currentIdx;

        public SqlNodeScopeIndex( SqlNodeScopeBuilder inner, int start = 0, int count = -1 )
        {
            if( inner == null ) throw new ArgumentNullException( nameof( inner ) );
            _inner = inner;
            _start = start;
            _count = count;
            _stop = count < 0 ? int.MaxValue : _start + count;
       }

        protected override void DoReset()
        {
            _inner.Reset();
            _currentIdx = 0;
        }

        protected override ISqlNodeLocationRange DoEnter( IVisitContext context )
        {
            return Handle( _inner.Enter( context ), context );
        }

        protected override ISqlNodeLocationRange DoLeave( IVisitContext context )
        {
            return Handle( _inner.Leave( context ), context );
        }

        protected override ISqlNodeLocationRange DoConclude( IVisitContextBase context )
        {
            return Handle( _inner.Conclude( context ), context );
        }

        ISqlNodeLocationRange Handle( ISqlNodeLocationRange inner, IVisitContextBase context )
        {
            if( inner == null ) return null;
            int nbInner = inner.Count;
            int futureIdx = _currentIdx + nbInner;
            if( futureIdx < _start || _currentIdx > _stop ) return null;

            IEnumerable<SqlNodeLocationRange> e = inner;
            int deltaFront = _start - _currentIdx;
            if( deltaFront > 0 )
            {
                e = e.Skip( deltaFront );
                nbInner -= deltaFront;
                _currentIdx = _start;
            }
            if( nbInner > _count ) nbInner = _count;
            return SqlNodeLocationRange.Create( e.Take( nbInner ), nbInner, true );
        }

    }


}
