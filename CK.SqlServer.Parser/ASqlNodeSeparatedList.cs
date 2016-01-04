using CK.Core;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace CK.SqlServer.Parser
{
    /// <summary>
    /// Generic list of T separated by TSep.
    /// </summary>
    public abstract class ASqlNodeSeparatedList<T,TSep> : SqlNode, IReadOnlyList<T>
        where T : class, ISqlNode 
        where TSep : class, ISqlNode
    {
        readonly ISqlNode[] _items;

        protected ASqlNodeSeparatedList(
            ASqlNodeSeparatedList<T, TSep> o,
            int minCount, 
            ImmutableList<SqlTrivia> leading, 
            IEnumerable<ISqlNode> items, 
            ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _items = o._items;
            else
            {
                _items = items as ISqlNode[] ?? items.ToArray();
                CheckItemAndSeparators( o, minCount, _items, 0, _items.Length );
            }
        }

        internal static ISqlNode[] CheckItemAndSeparators( ISqlNode o, int minItemCount, ISqlNode[] items, int start, int count )
        {
            int stop = start + count;
            for( int idx = start; idx < stop; ++idx )
            {
                var e = items[idx];
                if( ((idx - start) & 1) == 0 )
                {
                    if( !(e is T) ) ASqlNodeList<T>.RaiseItemTypeError( o, idx, e );
                }
                else
                {
                    if( !(e is TSep) )
                    {
                        throw new ArgumentException( string.Format( "'{0}': Expected separator '{1}' at {2} but got '{3}'.",
                            o.GetType().Name,
                            typeof( TSep ).Name, 
                            idx, 
                            e != null ? e.GetType().Name : "null" ), nameof( items ) );
                    }
                    if( idx == stop - 1 )
                    {
                        throw new ArgumentException( string.Format( "'{0}': Extra trailing separator '{1}' found at {2}.",
                            o.GetType().Name,
                            typeof( TSep ).Name,
                            idx ), nameof( items ) );
                    }
                }
            }
            if( count + 1 < minItemCount * 2 ) ASqlNodeList<T>.RaiseMinItemCountError( o, (count + 1) / 2, minItemCount );
            return items;
        }

        public T this[int index] => (T)_items[index*2];

        /// <summary>
        /// Gets the direct children if any. Never null.
        /// </summary>
        public override IReadOnlyList<ISqlNode> ChildrenNodes => _items;

        public int Count => (_items.Length + 1) / 2;

        public IEnumerator<T> GetEnumerator()
        {
            return _items.Where( (x,i) => (i&1) == 0 ).Cast<T>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
