using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CK.SqlServer.Parser;

/// <summary>
/// Defines a non empty list of <see cref="SqlCaseWhenSelector"/> ("when Expression then Value") items 
/// of a <see cref="SqlCase"/> expression.
/// </summary>
public sealed class SqlCaseWhenList : ASqlNodeList<SqlCaseWhenSelector>
{
    public SqlCaseWhenList( IEnumerable<SqlCaseWhenSelector> items )
        : base( 1, items )
    {
    }

    SqlCaseWhenList( SqlCaseWhenList o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
        : base( o, 1, leading, items, trailing )
    {
    }

    protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
    {
        return new SqlCaseWhenList( this, leading, content, trailing );
    }

    [DebuggerStepThrough]
    internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

}
