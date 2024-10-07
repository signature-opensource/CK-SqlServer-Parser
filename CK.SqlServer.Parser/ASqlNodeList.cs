using CK.Core;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace CK.SqlServer.Parser;

/// <summary>
/// Simple abstract wrapper around an array of T.
/// </summary>
public abstract class ASqlNodeList<T> : SqlNonTokenAutoWidth, ISqlNodeList<T> where T : class, ISqlNode
{
    readonly IReadOnlyList<T> _items;

    private protected ASqlNodeList( int minCount, IEnumerable<T> items )
        : this( null, minCount, null, (IEnumerable<ISqlNode>)items, null )
    {
    }

    private protected ASqlNodeList( ASqlNodeList<T> o,
                                    int minCount,
                                    ImmutableList<SqlTrivia> leading,
                                    IEnumerable<ISqlNode> items,
                                    ImmutableList<SqlTrivia> trailing )
        : base( leading, trailing )
    {
        if( items == null ) _items = o._items;
        else
        {
            int i = CheckItemsTypeAndCount( this, minCount, items );
            T[] a = items as T[];
            if( i == 0 ) a = Array.Empty<T>();
            else if( a == null )
            {
                a = new T[i];
                i = 0;
                foreach( var e in items ) a[i++] = (T)e;
            }
            _items = a;
        }
    }

    static internal int CheckItemsTypeAndCount( ISqlNode o, int minCount, IEnumerable<ISqlNode> items )
    {
        int i = 0;
        foreach( var e in items )
        {
            if( e is not T ) RaiseItemTypeError( o, i, e );
            ++i;
        }
        if( i < minCount ) RaiseMinItemCountError( o, i, minCount );
        return i;
    }

    static internal void RaiseMinItemCountError( ISqlNode o, int count, int minCount )
    {
        throw new ArgumentException( string.Format( "'{0}': Must contain at least {1} item(s) (found only {2}).",
                                                        o.GetType().Name,
                                                        minCount,
                                                        count ), "items" );
    }

    static internal void RaiseItemTypeError( ISqlNode o, int i, ISqlNode e )
    {
        throw new ArgumentException( string.Format( "'{0}': Expected item '{1}' at {2} but got '{3}'.",
                o.GetType().Name,
                typeof( T ).Name,
                i,
                e != null ? e.GetType().Name : "null" ), "items" );
    }

    /// <summary>
    /// Gets the children at a given index.
    /// </summary>
    /// <param name="index">The children index.</param>
    /// <returns>The children.</returns>
    public T this[int index] => _items[index];

    /// <summary>
    /// Gets the direct children if any. Never null.
    /// </summary>
    public override sealed IReadOnlyList<ISqlNode> ChildrenNodes => _items;

    /// <inheritdoc />
    public override sealed IList<ISqlNode> GetRawContent() => _items.Cast<ISqlNode>().ToList();

    /// <summary>
    /// Gets the number of children.
    /// </summary>
    public int Count => _items.Count;

    /// <summary>
    /// Gets whether this list is empty.
    /// </summary>
    public bool IsEmpty => _items.Count == 0;

    /// <inheritdoc />
    public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
