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
    /// Enclosable list of separated T (relies on <see cref="ISqlStructurallyEnclosed"/>).
    /// </summary>
    public abstract class ASqlNodeEnclosableSeparatedList<TOpener,T,TSep,TCloser> : SqlNonToken, ISqlEnclosable, IReadOnlyList<T>
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
        public override sealed IReadOnlyList<ISqlNode> ChildrenNodes => _items;

        public override sealed IList<ISqlNode> GetRawContent() => _items.ToList();

        public int Count => (_items.Length + 1) / 2 - _enclosed;

        public IEnumerator<T> GetEnumerator()
        {
            return Count > 0 
                    ? _items.Where( (x,i) => (i&1) == _enclosed ).Cast<T>().GetEnumerator()
                    : Enumerable.Empty<T>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        protected ASqlNodeEnclosableSeparatedList<TOpener, T, TSep, TCloser> DoInsertAt( int idx, T item, bool encloseFirstItem = false )
        {
            if( item == null ) throw new ArgumentNullException( nameof( item ) );
            if( idx < 0 || idx > Count ) throw new IndexOutOfRangeException();
            ISqlNode result;
            int count = Count;
            if( count == 0 )
            {
                if( !encloseFirstItem || _enclosed != 0 )
                {
                    result = DoSetRawContent( _enclosed == 0
                                                ? new ISqlNode[] { item }
                                                : new ISqlNode[] { Opener, item, Closer } );
                }
                else
                {
                    result = DoSetRawContent( new ISqlNode[] { CreateDefaultOpener(), item, CreateDefaultCloser() } );
                }
            }
            else if( count == 1 )
            {
                TSep sep = CreateDefaultSeparator();
                if( idx == 0 )
                {
                    result = DoSetRawContent( _enclosed == 0
                                                ? new ISqlNode[] { item, sep, _items[0] }
                                                : new ISqlNode[] { Opener, item, sep, _items[1], Closer } );
                }
                else
                {
                    result = DoSetRawContent( _enclosed == 0
                                                ? new ISqlNode[] { _items[0], sep, item }
                                                : new ISqlNode[] { Opener, _items[1], sep, item, Closer } );
                }
            }
            else if( idx < count )
            {
                int rIdx = idx * 2 + _enclosed;
                TSep sep = idx == count-1 ? (TSep)_items[rIdx - 1] : (TSep)_items[rIdx + 1];
                var content = _items.ToList();
                content.Insert( rIdx, item );
                content.Insert( rIdx + 1, sep );
                result = DoSetRawContent( content );
            }
            else 
            {
                Debug.Assert( idx == count );
                TSep lastSep = (TSep)_items[_items.Length - 2 - _enclosed];
                var content = _items.ToList();
                if( _enclosed == 0 )
                {
                    content.Add( lastSep );
                    content.Add( item );
                }
                else
                {
                    content.Insert( content.Count - 1, lastSep );
                    content.Insert( content.Count - 1, item );
                }
                result = DoSetRawContent( content );
            }
            return (ASqlNodeEnclosableSeparatedList<TOpener, T, TSep, TCloser>)result;
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

        /// <summary>
        /// Returns the <see cref="SqlKeyword.OpenPar"/> by default.
        /// This MUST be overridden if <typeparamref name="TOpener"/> is not a parenthesis.
        /// </summary>
        /// <returns>A default opener.</returns>
        protected virtual TOpener CreateDefaultOpener()
        {
            Debug.Assert( typeof( TOpener ) == typeof( SqlTokenOpenPar ), "Defaults to OpenPar." );
            return SqlKeyword.OpenPar as TOpener;
        }

        /// <summary>
        /// Returns the <see cref="SqlKeyword.ClosePar"/> by default.
        /// This MUST be overridden if <typeparamref name="TCloser"/> is not a parenthesis.
        /// </summary>
        /// <returns>A default opener.</returns>
        protected virtual TCloser CreateDefaultCloser()
        {
            Debug.Assert( typeof( TCloser ) == typeof( SqlTokenClosePar ), "Defaults to ClosePar." );
            return SqlKeyword.ClosePar as TCloser;
        }
    }
}
