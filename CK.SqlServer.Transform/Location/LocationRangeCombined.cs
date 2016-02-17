using CK.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Transform
{
    internal class LocationRangeCombined : ISqlNodeLocationRange
    {
        readonly ISqlNodeLocationRange _r1;
        readonly ISqlNodeLocationRange _r2;

        internal LocationRangeCombined( ISqlNodeLocationRange r1, ISqlNodeLocationRange r2 )
        {
            Debug.Assert( r1 != null && r1 != SqlNodeLocationRange.Empty );
            Debug.Assert( r2 != null && r2 != SqlNodeLocationRange.Empty );
            _r1 = r1;
            _r2 = r2;
        }

        public SqlNodeLocationRange First => _r1.First;

        public SqlNodeLocationRange Last => _r2.Last;

        public IEnumerator<SqlNodeLocationRange> GetEnumerator() => _r1.Concat( _r2 ).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    }
}
