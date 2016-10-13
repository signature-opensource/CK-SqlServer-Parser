using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Transform
{
    public static class SqlNodeLocationRangeChunkExtension
    {
        class Chunk : ISqlNodeLocationRange
        {
            public readonly ISqlNodeLocationRange Inner;

            public Chunk( ISqlNodeLocationRange r )
            {
                Inner = r;
            }

            public int Count => Inner.Count;

            public SqlNodeLocationRange First => Inner.First;

            public SqlNodeLocationRange Last => Inner.Last;

            public IEnumerator<SqlNodeLocationRange> GetEnumerator() => Inner.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => Inner.GetEnumerator();
        }

        /// <summary>
        /// Gets whether this range is a chunk: it must be handled as an independant range.  
        /// </summary>
        /// <param name="this">Thie range.</param>
        /// <returns>True if this is a chunk.</returns>
        public static bool IsChunk( this ISqlNodeLocationRange @this ) => @this is Chunk;

        /// <summary>
        /// Returns a chunk or a non chunk range.
        /// </summary>
        /// <param name="this">This range.</param>
        /// <param name="set">True to obtain a chunk, false to return a mere range.</param>
        public static ISqlNodeLocationRange SetChunk( this ISqlNodeLocationRange @this, bool set = true )
        {
            return @this == null || set == @this.IsChunk()
                            ? @this
                            : (set ? new Chunk(@this) : ((Chunk)@this).Inner);
        }
    }
}
