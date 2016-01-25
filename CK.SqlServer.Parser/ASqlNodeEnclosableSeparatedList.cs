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
    public abstract class ASqlNodeEnclosableSeparatedList<TOpener,T,TSep,TCloser> : SqlNode, ISqlEnclosable, IReadOnlyList<T>
        where TOpener : class, ISqlNode
        where T : class, ISqlNode
        where TSep : class, ISqlNode
        where TCloser : class, ISqlNode
    {
        readonly ISqlNode[] _items;
        // 0 when no Opener/Closer, 1 otherwise.
        readonly int _enclosed;

        protected ASqlNodeEnclosableSeparatedList(
            ASqlNodeEnclosableSeparatedList<TOpener, T, TSep, TCloser> o,
            int minCount,
            ImmutableList<SqlTrivia> leading,
            IEnumerable<ISqlNode> items,
            ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            bool enclosed = this is ISqlStructurallyEnclosed;
            if( items == null )
            {
                if( enclosed && o._enclosed == 0 )
                {
                    throw new ArgumentException( string.Format( "{0}: Must always have Opener/Closer.", 
                        GetType().Name ), 
                        nameof( items ) );
                }
                _items = o._items;
                _enclosed = o._enclosed;
            }
            else
            {
                var a = Helper.EnsureArray( items );
                if( enclosed || (a.Length > 0 && a[0] is TOpener) )
                {
                    _enclosed = 1;
                    ASqlNodeEnclosableList<TOpener,T,TCloser>.CheckEnclosed( this, a );
                    ASqlNodeSeparatedList<T, TSep>.CheckItemAndSeparators( this, minCount, a, 1, a.Length - 2 );
                }
                else
                {
                    _enclosed = 0;
                    ASqlNodeSeparatedList<T, TSep>.CheckItemAndSeparators( this, minCount, a, 0, a.Length );
                }
                _items = a;
            }
        }

        protected ASqlNodeEnclosableSeparatedList( 
            int minCount,
            TOpener opener,
            IEnumerable<ISqlNode> content, 
            TCloser closer, 
            ImmutableList<SqlTrivia> leading = null, 
            ImmutableList<SqlTrivia> trailing = null )
            : this( null, 
                    minCount,
                    leading, 
                    opener != null ? BuildEnclosed( null, opener, content, closer ) : content, 
                    trailing )
        {
        }

        static internal ISqlNode[] BuildEnclosed( ISqlNode prefix, TOpener opener, IEnumerable<ISqlNode> content, TCloser closer )
        {
            var a = new List<ISqlNode>();
            if( prefix != null ) a.Add( prefix );
            a.Add( opener );
            a.AddRange( content );
            a.Add( closer );
            return a.ToArray();
        }

        public bool IsEnclosed => _enclosed != 0;

        public TOpener Opener => _enclosed != 0 ? (TOpener)_items[0] : null;

        public T this[int index] => (T)_items[index * 2 + _enclosed];

        public TCloser Closer => _enclosed != 0 ? (TCloser)_items[_items.Length - 1] : null;

        /// <summary>
        /// Gets the direct children if any. Never null.
        /// </summary>
        public sealed override IReadOnlyList<ISqlNode> ChildrenNodes => _items;

        public sealed override IList<ISqlNode> GetRawContent() => _items.ToList();

        public int Count => (_items.Length + 1) / 2 - _enclosed;

        public IEnumerator<T> GetEnumerator()
        {
            return Count > 0 
                    ? _items.Where( (x,i) => (i&1) == _enclosed ).Cast<T>().GetEnumerator()
                    : Enumerable.Empty<T>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
