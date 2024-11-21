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
/// Generic list of T separated by TSep and prefixed by a TPrefix.
/// </summary>
public abstract class ASqlNodePrefixedSeparatedList<TPrefix, T, TSep> : SqlNonTokenAutoWidth, ISqlNodeList<T>
    where TPrefix : class, ISqlNode
    where T : class, ISqlNode
    where TSep : class, ISqlNode
{
    readonly ISqlNode[] _items;

    private protected ASqlNodePrefixedSeparatedList( int minCount,
                                                     TPrefix prefix,
                                                     IEnumerable<ISqlNode> items )
        : base( null, null )
    {
        List<ISqlNode> i = new List<ISqlNode>();
        i.Add( prefix );
        i.AddRange( items );
        _items = i.ToArray();
        CheckContent( _items, minCount );
    }

    private protected ASqlNodePrefixedSeparatedList( ASqlNodePrefixedSeparatedList<TPrefix, T, TSep> o,
                                                     int minCount,
                                                     ImmutableList<SqlTrivia>? leading,
                                                     IEnumerable<ISqlNode> items,
                                                     ImmutableList<SqlTrivia>? trailing )
        : base( leading, trailing )
    {
        if( items == null ) _items = o._items;
        else
        {
            _items = items as ISqlNode[] ?? items.ToArray();
            CheckContent( _items, minCount );
        }
    }

    void CheckContent( ISqlNode[] content, int minCount )
    {
        CheckPrefix( content );
        ASqlNodeSeparatedList<T, TSep>.CheckItemAndSeparators( this, minCount, content, 1, content.Length - 1 );
    }

    internal static void CheckPrefix( ISqlNode[] content )
    {
        if( content.Length == 0 )
        {
            throw new ArgumentException( string.Format( "Expected prefix of type '{0}'.", typeof( TPrefix ).Name ) );
        }
        if( !(content[0] is TPrefix) )
        {
            throw new ArgumentException( string.Format( "Expected prefix of type '{0}', not '{1}'.",
                typeof( TPrefix ).Name, content[0] != null ? content[0].GetType().Name : "null" ) );
        }
    }

    /// <summary>
    /// Gets the children at the given index, skipping prefix and separators.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>The children.</returns>
    public T this[int index] => (T)_items[1 + index * 2];

    /// <summary>
    /// Gets all the direct children if any. Never null.
    /// </summary>
    public override IReadOnlyList<ISqlNode> ChildrenNodes => _items;

    /// <inheritdoc />
    public override sealed IList<ISqlNode> GetRawContent() => _items.ToList();

    /// <summary>
    /// Gets the prefix of this list.
    /// </summary>
    protected TPrefix Prefix => (TPrefix)_items[0];

    /// <summary>
    /// Gets the number of actual children, excluding prefix and separators.
    /// </summary>
    public int Count => _items.Length / 2;

    /// <summary>
    /// Gets the actual children skipping prefix and separators.
    /// </summary>
    /// <returns>An enumerator of actual children.</returns>
    public IEnumerator<T> GetEnumerator()
    {
        return _items.Where( ( x, i ) => (i & 1) == 1 ).Cast<T>().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
