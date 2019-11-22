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
    /// Simple abstract wrapper around an array of T optionally enclosed.
    /// </summary>
    public abstract class ASqlNodePrefixedEnclosedSeparatedList<TPrefix,TOpener,T,TSep,TCloser> : SqlNonTokenAutoWidth, ISqlNodeList<T>
        where TPrefix : class, ISqlNode
        where TOpener : class, ISqlNode
        where T : class, ISqlNode
        where TSep : class, ISqlNode
        where TCloser : class, ISqlNode
    {
        readonly ISqlNode[] _items;

        protected ASqlNodePrefixedEnclosedSeparatedList(
            ASqlNodePrefixedEnclosedSeparatedList<TPrefix,TOpener, T, TSep, TCloser> o,
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
                CheckContent( minCount );
            }
        }

        void CheckContent( int minCount )
        {
            ASqlNodePrefixedSeparatedList<TPrefix, T, TSep>.CheckPrefix( _items );
            ASqlNodeEnclosableList<TOpener, T, TCloser>.CheckEnclosed( this, _items, 1 );
            ASqlNodeSeparatedList<T, TSep>.CheckItemAndSeparators( this, minCount, _items, 2, _items.Length - 3 );
        }

        protected ASqlNodePrefixedEnclosedSeparatedList( 
            int minCount,
            TPrefix prefix,
            TOpener opener,
            IEnumerable<ISqlNode> content, 
            TCloser closer )
            : this( null, 
                    minCount,
                    null,
                    ASqlNodeEnclosableList<TOpener,T,TCloser>.BuildEnclosed( prefix, opener, content, closer ), 
                    null )
        {
            CheckContent( minCount );
        }

        protected TPrefix Prefix => (TPrefix)_items[0];

        protected TOpener Opener => (TOpener)_items[1];

        public T this[int index] => (T)_items[(index+1) * 2];

        protected TCloser Closer => (TCloser)_items[_items.Length - 1];

        /// <summary>
        /// Gets the direct children if any. Never null.
        /// </summary>
        public override IReadOnlyList<ISqlNode> ChildrenNodes => _items;

        public override sealed IList<ISqlNode> GetRawContent() => _items.ToList();

        public int Count => (_items.Length) / 2 - 1;

        public IEnumerator<T> GetEnumerator()
        {
            return Count > 0 
                    ? _items.Where( (x,i) => i > 0 && (i&1) == 0 ).Cast<T>().GetEnumerator()
                    : Enumerable.Empty<T>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
