using CK.SqlServer.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Transform
{

    /// <summary>
    /// Abstract range builder.
    /// </summary>
    public abstract class SqlNodeRangeBuilder
    {
        ISqlNodeLocationRange _last;

        /// <summary>
        /// Resets any internal state
        /// </summary>
        public void Reset()
        {
            _last = null;
        }

        internal ISqlNodeLocationRange Enter( SqlNodeLocationVisitor.IVisitContext context )
        {
            return Handle( DoEnter( context ) );
        }

        internal ISqlNodeLocationRange Leave( SqlNodeLocationVisitor.IVisitContext context )
        {
            return Handle( DoLeave( context ) );
        }

        internal void Conclude( ISqlNodeLocationManager locManager, Action<ISqlNodeLocationRange> flush )
        {
            var r1 = Handle( DoConclude( locManager ) );
            if( r1 != null ) flush( r1 );
            if( _last != null ) flush( _last );
            _last = null;
        }

        /// <summary>
        /// Must reset any internal state.
        /// </summary>
        protected abstract void DoReset();

        /// <summary>
        /// Called for each node, before visiting its children. Mey return a range.
        /// </summary>
        /// <param name="context">The visited node and location manager to use.</param>
        /// <returns>Null or a range to consider.</returns>
        protected abstract ISqlNodeLocationRange DoEnter( SqlNodeLocationVisitor.IVisitContext context );

        /// <summary>
        /// Called for each node, before visiting its children. Mey return a range.
        /// </summary>
        /// <param name="context">The visited node and location manager to use.</param>
        /// <returns>Null or a range to consider.</returns>
        protected abstract ISqlNodeLocationRange DoLeave( SqlNodeLocationVisitor.IVisitContext context );

        /// <summary>
        /// Called at the end of the visit.
        /// </summary>
        /// <param name="locManager">Location manager to use.</param>
        /// <returns>Null or the final range to consider.</returns>
        protected abstract ISqlNodeLocationRange DoConclude( ISqlNodeLocationManager locManager );

        ISqlNodeLocationRange Handle( ISqlNodeLocationRange r )
        {
            if( r == null || r == SqlNodeLocationRange.Empty ) return null;
            ISqlNodeLocationRange result = _last;
            if( result != null )
            {
                var l = result.Last;
                if( l.End.Position > r.First.Beg.Position ) throw new InvalidOperationException( "Newly built range intersects previous one." );
                if( l.End.Position == r.First.Beg.Position )
                {
                    l.InternalExtend( r.Last.End );
                    return null;
                }
            }
            _last = r;
            return result;
        }
    }


}
