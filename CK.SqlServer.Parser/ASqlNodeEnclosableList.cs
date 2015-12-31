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
    /// Abstract base class for an optionally enclosed list of nodes.
    /// </summary>
    public abstract class ASqlNodeEnclosableList<TOpener,T,TCloser> : SqlNode, ISqlEnclosable, IReadOnlyList<T>
        where TOpener : class, ISqlNode
        where T : class, ISqlNode 
        where TCloser : class, ISqlNode
    {
        readonly ISqlNode[] _items;
        // 0 when no Opener/Closer, 1 otherwise.
        int _enclosed;

        protected ASqlNodeEnclosableList(
            ASqlNodeEnclosableList<TOpener, T, TCloser> o,
            int minCount,
            ImmutableList<SqlTrivia> leading,
            IEnumerable<ISqlNode> items,
            ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            bool enclosed = this is ISqlStructurallyEnclosed;
           if( items == null )
            {
                _items = o._items;
                _enclosed = o._enclosed;
            }
            else
            {
                var a = items as ISqlNode[] ?? items.ToArray();
                if( enclosed || (a.Length > 0 && a[0] is TOpener))
                {
                    CheckEnclosed( this, a );
                    int count = a.Length - 2;
                    for( int i = 1; i < count; ++i )
                    {
                        if( !(a[i] is T) ) ASqlNodeList<T>.RaiseItemTypeError( this, i, a[i] );
                    }
                    if( count < minCount ) ASqlNodeList<T>.RaiseMinItemCountError( this, count, minCount );
                }
                else
                {
                    ASqlNodeList<T>.CheckItemsTypeAndCount( this, minCount, a );
                }
                _items = a;
            }
        }

        static internal void CheckEnclosed( ISqlNode o, ISqlNode[] items )
        {
            if( items.Length < 2 || !(items[0] is TOpener) || !(items[items.Length - 1] is TCloser) )
            {
                throw new ArgumentException( string.Format( "'{0}': Items must start with a '{1}' and end with a '{2}'.",
                                                                o.GetType().Name,
                                                                typeof( TOpener ).Name,
                                                                typeof( TCloser ).Name ), nameof( items ) );
            }
        }

        public bool IsEnclosed => _enclosed != 0;

        public TOpener Opener => (TOpener)_items[0];

        public T this[int index] => (T)_items[index+1];

        public TCloser Closer => (TCloser)_items[_items.Length-1];
        
        /// <summary>
        /// Gets the direct children if any. Never null.
        /// </summary>
        public override IReadOnlyList<ISqlNode> ChildrenNodes => _items;

        public int Count => _items.Length - 2;

        public IEnumerator<T> GetEnumerator()
        {
            return _items.Skip(1).Take( _items.Length-2 ).Cast<T>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
