using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CK.SqlServer.Parser;

/// <summary>
/// Non-enclosable, possibly empty, comma separated list of any kind of node.
/// </summary>
public sealed class SqlCommaList : ASqlNodeSeparatedList<ISqlNode, SqlTokenComma>
{
    public SqlCommaList( IEnumerable<ISqlNode> items )
        : base( null, 0, null, items, null )
    {
    }

    SqlCommaList( SqlCommaList o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
        : base( o, 0, leading, items, trailing )
    {
    }

    protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
    {
        return new SqlCommaList( this, leading, content, trailing );
    }

    /// <summary>
    /// Inserts a new item in this list.
    /// </summary>
    /// <param name="idx">Insertion index.</param>
    /// <param name="item">Item to insert.</param>
    /// <returns>The new list.</returns>
    public SqlCommaList InsertAt( int idx, ISqlNode item ) => (SqlCommaList)DoInsertAt( idx, item );

    [DebuggerStepThrough]
    internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

}
