using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CK.SqlServer.Parser;

/// <summary>
/// List of comma separated <see cref="SqlCTEName"/> that can not be empty.
/// </summary>
public sealed class SqlCTENameList : ASqlNodeSeparatedList<SqlCTEName, SqlTokenComma>
{
    public SqlCTENameList( IEnumerable<ISqlNode> items )
        : base( null, 1, null, items, null )
    {
    }

    SqlCTENameList( SqlCTENameList o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
        : base( o, 1, leading, items, trailing )
    {
    }

    protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
    {
        return new SqlCTENameList( this, leading, content, trailing );
    }

    [DebuggerStepThrough]
    internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

}
