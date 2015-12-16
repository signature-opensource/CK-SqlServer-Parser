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
    /// Simple abstract wrapper around an array of T.
    /// </summary>
    public abstract class SqlNodeList<T> : SqlNode, IReadOnlyList<T> where T : SqlNode
    {
        readonly T[] _items;

        protected SqlNodeList( IEnumerable<T> items, ImmutableList<SqlTrivia> leading = null, ImmutableList<SqlTrivia> trailing = null )
            : base( leading, trailing )
        {
            _items = items.ToArray();
        }

        protected SqlNodeList( ImmutableList<SqlTrivia> leading, T[] items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            Debug.Assert( items != null );
            _items = items;
        }

        public T this[int index] => _items[index];

        /// <summary>
        /// Gets the direct children if any. Never null.
        /// </summary>
        public override IReadOnlyList<SqlNode> ChildrenNodes => _items;

        public int Count => _items.Length;

        public IEnumerator<T> GetEnumerator()
        {
            return ((IReadOnlyList<T>)_items).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
