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
    public abstract class SqlNodeEnclosedSeparatedList<TOpener,T,TSep,TCloser> : SqlNode, IReadOnlyList<T>
        where TOpener : SqlNode
        where T : SqlNode
        where TSep : SqlNode
        where TCloser : SqlNode
    {
        readonly ISqlNode[] _items;

        protected SqlNodeEnclosedSeparatedList( IEnumerable<ISqlNode> enclosedItems, ImmutableList<SqlTrivia> leading = null, ImmutableList<SqlTrivia> trailing = null )
            : base( leading, trailing )
        {
            _items = enclosedItems.ToArray();
        }

        protected SqlNodeEnclosedSeparatedList( ImmutableList<SqlTrivia> leading, ISqlNode[] items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            Debug.Assert( items != null );
            _items = items;
        }

        public TOpener Opener => (TOpener)_items[0];

        public T this[int index] => (T)_items[index*2+1];

        public TCloser Closer => (TCloser)_items[_items.Length - 1];

        /// <summary>
        /// Gets the direct children if any. Never null.
        /// </summary>
        public override IReadOnlyList<ISqlNode> ChildrenNodes => _items;

        public int Count => _items.Length / 2 - 1;

        public IEnumerator<T> GetEnumerator()
        {
            return _items.Where( (x,i) => (i&1) == 1 ).Cast<T>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
