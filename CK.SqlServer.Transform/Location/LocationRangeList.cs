using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Transform
{
    internal class LocationRangeList : ISqlNodeLocationRange, ISqlNodeLocationRangeInternal
    {
        readonly IReadOnlyList<SqlNodeLocationRange> _v;

        internal LocationRangeList( params SqlNodeLocationRange[] values )
            : this( (IReadOnlyList<SqlNodeLocationRange>)values )
        {
        }

        internal LocationRangeList( IReadOnlyList<SqlNodeLocationRange> list )
        {
            Debug.Assert( list != null && list.Count > 1 && list.All( r => r != null ) );
            _v = list;
        }
        public int Count => _v.Count;

        public SqlNodeLocationRange First => _v[0];

        public SqlNodeLocationRange Last => _v[_v.Count - 1];

        public IEnumerator<SqlNodeLocationRange> GetEnumerator() => _v.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _v.GetEnumerator();

        public ISqlNodeLocationRangeInternal InternalSetEnd( SqlNodeLocation end )
        {
            var v = _v.ToArray();
            v[v.Length - 1] = v[v.Length - 1].InternalSetEnd( end );
            return new LocationRangeList( v );
        }

        public override string ToString()
        {
            return string.Join( "-", this.Select( r => r.ToString() ) );
        }

    }
}
