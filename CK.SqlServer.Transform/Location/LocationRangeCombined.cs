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
    internal class LocationRangeCombined : ISqlNodeLocationRange, ISqlNodeLocationRangeInternal
    {
        readonly ISqlNodeLocationRangeInternal _r1;
        readonly ISqlNodeLocationRangeInternal _r2;

        internal LocationRangeCombined( ISqlNodeLocationRangeInternal r1, ISqlNodeLocationRangeInternal r2 )
        {
            Debug.Assert( r1 != null && r1 != SqlNodeLocationRange.EmptySet );
            Debug.Assert( r2 != null && r2 != SqlNodeLocationRange.EmptySet );
            _r1 = r1;
            _r2 = r2;
        }

        public int Count => _r1.Count + _r2.Count;

        public SqlNodeLocationRange First => _r1.First;

        public SqlNodeLocationRange Last => _r2.Last;

        public IEnumerator<SqlNodeLocationRange> GetEnumerator() => _r1.Concat( _r2 ).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public ISqlNodeLocationRangeInternal InternalSetEnd( SqlNodeLocation end )
        {
            return new LocationRangeCombined( _r1, _r2.InternalSetEnd( end ) );
        }

        public override string ToString()
        {
            return string.Join( "-", this.Select( r => r.ToString() ) );
        }


    }
}
