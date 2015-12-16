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
    /// Abstract base class for an enclosed list of nodes.
    /// </summary>
    public abstract class SqlNodeEnclosedList<TOpener,T,TCloser> : SqlNode, IReadOnlyList<T>
        where TOpener : SqlNode
        where T : SqlNode
        where TCloser : SqlNode
    {
        readonly SqlNode[] _items;

        protected SqlNodeEnclosedList( IEnumerable<SqlNode> enclosedItems, ImmutableList<SqlTrivia> leading = null, ImmutableList<SqlTrivia> trailing = null )
            : base( leading, trailing )
        {
            _items = enclosedItems.ToArray();
        }

        protected SqlNodeEnclosedList( ImmutableList<SqlTrivia> leading, T[] items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            Debug.Assert( items != null );
            _items = items;
        }

        public TOpener Opener => (TOpener)_items[0];

        public T this[int index] => (T)_items[index+1];

        public TCloser Closer => (TCloser)_items[_items.Length-1];
        
        /// <summary>
        /// Gets the direct children if any. Never null.
        /// </summary>
        public override IReadOnlyList<SqlNode> ChildrenNodes => _items;

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
