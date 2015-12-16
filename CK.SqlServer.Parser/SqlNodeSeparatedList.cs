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
    public abstract class SqlNodeSeparatedList<T,TSep> : SqlNode, IReadOnlyList<T>
        where T : SqlNode
        where TSep : SqlNode
    {
        readonly SqlNode[] _items;

        protected SqlNodeSeparatedList( IEnumerable<SqlNode> items, ImmutableList<SqlTrivia> leading = null, ImmutableList<SqlTrivia> trailing = null )
            : base( leading, trailing )
        {
            _items = items.ToArray();
        }

        protected SqlNodeSeparatedList( ImmutableList<SqlTrivia> leading, SqlNode[] items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            Debug.Assert( items != null );
            _items = items;
        }

        public T this[int index] => (T)_items[index*2];

        /// <summary>
        /// Gets the direct children if any. Never null.
        /// </summary>
        public override IReadOnlyList<SqlNode> ChildrenNodes => _items;

        public int Count => _items.Length / 2;

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
