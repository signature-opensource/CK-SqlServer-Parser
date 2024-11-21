using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CK.SqlServer.Parser;

/// <summary>
/// Possibly empty list of comma separated <see cref="SelectColumn"/>
/// </summary>
public class SelectColumnList : ASqlNodeSeparatedList<SelectColumn, SqlTokenComma>
{
    public SelectColumnList( IEnumerable<ISqlNode> items )
        : base( null, 0, null, items, null )
    {
    }

    SelectColumnList( SelectColumnList o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
        : base( o, 0, leading, items, trailing )
    {
    }

    protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
    {
        return new SelectColumnList( this, leading, content, trailing );
    }

    public SelectColumnList InsertAt( int idx, SelectColumn col ) => (SelectColumnList)DoInsertAt( idx, col );

    [DebuggerStepThrough]
    internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

}
