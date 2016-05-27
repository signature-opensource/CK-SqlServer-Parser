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
    public abstract class ASqlNodeEnclosableList<TOpener,T,TCloser> : SqlNonToken, ISqlEnclosable, IReadOnlyList<T>
        where TOpener : class, ISqlNode
        where T : class, ISqlNode 
        where TCloser : class, ISqlNode
    {
        readonly ISqlNode[] _items;
        // 0 when no Opener/Closer, 1 otherwise.
        int _enclosed;

        /// <summary>
        /// Initializes a new enclosable list.
        /// </summary>
        /// <param name="o"></param>
        /// <param name="minCount"></param>
        /// <param name="leading"></param>
        /// <param name="items"></param>
        /// <param name="trailing"></param>
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

        protected ASqlNodeEnclosableList(
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

        static internal void CheckEnclosed( ISqlNode o, ISqlNode[] items, int startIdx = 0 )
        {
            if( items.Length < startIdx + 2 || !(items[startIdx] is TOpener) || !(items[items.Length - 1] is TCloser) )
            {
                throw new ArgumentException( string.Format( "'{0}': Items must {3} a '{1}' and end with a '{2}'.",
                                                                o.GetType().Name,
                                                                typeof( TOpener ).Name,
                                                                typeof( TCloser ).Name,
                                                                startIdx == 0 ? "start with" : "contain"), nameof( items ) );
            }
        }

        /// <summary>
        /// Gets whether this list is actually enclosed in parenthesis.
        /// </summary>
        public bool IsEnclosed => _enclosed != 0;

        /// <summary>
        /// Gets the opener element. Null if <see cref="IsEnclosed"/> is false.
        /// </summary>
        public TOpener Opener => (TOpener)_items[0];

        /// <summary>
        /// Gets the node at a position.
        /// </summary>
        /// <param name="index">Index of the node in list.</param>
        /// <returns>The node.</returns>
        public T this[int index] => (T)_items[index+1];

        /// <summary>
        /// Gets the closer element. Null if <see cref="IsEnclosed"/> is false.
        /// </summary>
        public TCloser Closer => (TCloser)_items[_items.Length-1];
        
        /// <summary>
        /// Gets the direct children if any. Never null.
        /// </summary>
        public override IReadOnlyList<ISqlNode> ChildrenNodes => _items;

        public override sealed IList<ISqlNode> GetRawContent() => _items.ToList();

        /// <summary>
        /// Gets the count of items in this list.
        /// </summary>
        public int Count => _items.Length - 2;

        /// <summary>
        /// Gets the enumerator on the node (not containing the <see cref="Opener"/> nor the <see cref="Closer"/>).
        /// </summary>
        /// <returns>The enumerator.</returns>
        public IEnumerator<T> GetEnumerator()
        {
            return _items.Skip(1).Take( _items.Length-2 ).Cast<T>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    }
}
