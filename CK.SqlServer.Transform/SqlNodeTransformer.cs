using CK.Core;
using CK.SqlServer.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Transform
{
    public class SqlNodeTransformer : SqlNodeVisitor
    {
        readonly IActivityMonitor _monitor;
        bool _stop;

        protected SqlNodeTransformer( IActivityMonitor monitor )
        {
            _monitor = monitor;
        }

        public override ISqlNode VisitItem( ISqlNode e )
        {
            return _stop ? e : base.VisitItem( e );
        }

        protected IActivityMonitor Monitor => _monitor;

        protected void StopVisit() => _stop = true;

    }
}
