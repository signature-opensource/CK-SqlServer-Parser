using CK.SqlServer.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Transform
{

    /// <summary>
    /// Builds intersected ranges.
    /// </summary>
    public class SqlNodeRangeIntersect : SqlNodeRangeBuilder
    {
        readonly SqlNodeRangeBuilder _left;
        readonly SqlNodeRangeBuilder _right;

        public SqlNodeRangeIntersect( SqlNodeRangeBuilder left, SqlNodeRangeBuilder right )
        {
            if( left == null ) throw new ArgumentNullException( nameof( left ) );
            if( right == null ) throw new ArgumentNullException( nameof( right ) );
            _left = left;
            _right = right;
        }

        protected override void DoReset()
        {
            _left.Reset();
            _right.Reset();
        }

        protected override ISqlNodeLocationRange DoEnter( SqlNodeLocationVisitor.IVisitContext context )
        {
            ISqlNodeLocationRange rL = _left.Enter( context );
            ISqlNodeLocationRange rR = _right.Enter( context );
            throw new NotImplementedException();
        }

        protected override ISqlNodeLocationRange DoLeave( SqlNodeLocationVisitor.IVisitContext context )
        {
            throw new NotImplementedException();
        }

        protected override ISqlNodeLocationRange DoConclude( ISqlNodeLocationManager locManager )
        {
            throw new NotImplementedException();
        }
    }


}
