using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Transform
{
    internal class LocationRangeList : ISqlNodeLocationRange
    {
        readonly IReadOnlyList<SqlNodeLocationRange> _v;

        internal LocationRangeList( params SqlNodeLocationRange[] values )
            : this( (IReadOnlyList<SqlNodeLocationRange>)values )
        {
        }

        internal LocationRangeList( IReadOnlyList<SqlNodeLocationRange> list )
        {
            Debug.Assert( list != null && list.Count > 1 && list.All( r => r != null ) );
            Debug.Assert( list.Select( (r,idx) => idx == 0 || list[idx-1].End.Position < r.Beg.Position ).Any() );
            _v = list;
        }

        public SqlNodeLocationRange First => _v[0];

        public SqlNodeLocationRange Last => _v[_v.Count - 1];

        public IEnumerator<SqlNodeLocationRange> GetEnumerator() => _v.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _v.GetEnumerator();

        public override string ToString()
        {
            return string.Join( ", ", this.Select( r => r.ToString() ) );
        }

    }
}
