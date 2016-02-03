using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Transform
{
    static class CK_CORE_NEXTVERSION
    {
        static public int IndexOf<T>( this IReadOnlyList<T> @this, Func<T, bool> predicate )
        {
            if( predicate == null ) throw new ArgumentNullException( nameof( predicate ) );
            int i = 0;
            foreach( var x in @this )
            {
                if( predicate( x ) ) return i;
                ++i;
            }
            return -1;
        }
    }
}
