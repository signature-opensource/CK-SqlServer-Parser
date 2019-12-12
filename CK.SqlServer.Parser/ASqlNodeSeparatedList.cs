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
    public abstract class ASqlNodeSeparatedList<T,TSep> : SqlNonTokenAutoWidth, ISqlNodeList<T>
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
                _items = Helper.EnsureArray( items );
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
        public override sealed IReadOnlyList<ISqlNode> ChildrenNodes => _items;

        public override sealed IList<ISqlNode> GetRawContent() => _items.ToList();

        public int Count => (_items.Length + 1) / 2;

        /// <summary>
        /// Transforms or removes an item.
        /// </summary>
        /// <param name="transform">Transformer. When null is returned, the item is removed.</param>
        /// <returns>A new list.</returns>
        protected ASqlNodeSeparatedList<T, TSep> DoReplaceItems( Func<int,T,T> transform )
        {
            if( transform == null ) throw new ArgumentNullException( nameof( transform ) );
            int count = Count;
            List<ISqlNode> changed = null;
            int iC = 0;
            for( int i = 0; i < count; ++i )
            {
                var item = this[i];
                var item2 = transform( i, item );
                if( item != item2 )
                {
                    if( changed == null ) changed = _items.ToList();
                    int idChanged = iC * 2;
                    if( item2 == null )
                    {
                        changed.RemoveAt( idChanged );
                        if( i < count-1 ) changed.RemoveAt( idChanged );
                        else if( changed.Count > 1 ) changed.RemoveAt( changed.Count-1 );
                        --iC;
                    }
                    else changed[idChanged] = item2;
                }
                ++iC;
            }
            return changed != null ? this.SetRawContent( changed ) : this;
        }

        /// <summary>
        /// Inserts a new item at a specified position in the list.
        /// By default, <see cref="CreateDefaultSeparator"/> is used.
        /// </summary>
        /// <param name="idx">Insertion position.</param>
        /// <param name="item">Item to insert.</param>
        /// <param name="reuseExistingSeparatorIfPossible">False to always use <paramref name="separatorFactory"/>.</param>
        /// <param name="separatorFactory">Let it to null to use <see cref="CreateDefaultSeparator"/>.</param>
        /// <returns>A new list.</returns>
        protected ASqlNodeSeparatedList<T, TSep> DoInsertAt( int idx, T item, bool reuseExistingSeparatorIfPossible = true, Func<TSep> separatorFactory = null )
        {
            if( item == null ) throw new ArgumentNullException( nameof( item ) );
            if( idx < 0 || idx > Count ) throw new IndexOutOfRangeException();
            if( separatorFactory == null ) separatorFactory = CreateDefaultSeparator;
            ISqlNode result;
            int count = Count;
            if( count == 0 )
            {
                result = DoSetRawContent( new ISqlNode[] { item } );
            }
            else if( count == 1 )
            {
                TSep sep = CreateDefaultSeparator();
                if( idx == 0 )
                {
                    result = DoSetRawContent( new ISqlNode[] { item, sep, _items[0] } );
                }
                else
                {
                    result = DoSetRawContent( new ISqlNode[] { _items[0], sep, item } );
                }
            }
            else if( idx < count )
            {
                int rIdx = idx * 2;
                TSep sep;
                if( reuseExistingSeparatorIfPossible )
                    sep = idx == count - 1 ? (TSep)_items[rIdx - 1] : (TSep)_items[rIdx + 1];
                else sep = separatorFactory();
                var content = _items.ToList();
                content.Insert( rIdx, item );
                content.Insert( rIdx + 1, sep );
                result = DoSetRawContent( content );
            }
            else
            {
                Debug.Assert( idx == count );
                TSep sep;
                if( reuseExistingSeparatorIfPossible )
                    sep = (TSep)_items[_items.Length - 2];
                else sep = separatorFactory();
                var content = _items.ToList();
                content.Add( sep );
                content.Add( item );
                result = DoSetRawContent( content );
            }
            return (ASqlNodeSeparatedList<T, TSep>)result;
        }

        /// <summary>
        /// Returns the <see cref="SqlKeyword.CommaOneSpace"/> (", ") by default.
        /// This MUST be overridden if <typeparamref name="TSep"/> is not a comma.
        /// </summary>
        /// <returns>A default list separator.</returns>
        protected virtual TSep CreateDefaultSeparator()
        {
            Debug.Assert( typeof( TSep ) == typeof( SqlTokenComma ), "Defaults to comma." );
            return SqlKeyword.CommaOneSpace as TSep;
        }

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
