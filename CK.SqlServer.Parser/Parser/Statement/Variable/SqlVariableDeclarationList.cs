using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CK.SqlServer.Parser;

/// <summary>
/// List of variable declarations.
/// </summary>
public sealed class SqlVariableDeclarationList : ASqlNodeSeparatedList<SqlVariableDeclaration, SqlTokenComma>
{
    /// <summary>
    /// Initializes a new list of variable declarations.
    /// </summary>
    /// <param name="items">Comma separated list of <see cref="SqlVariableDeclaration"/> (must not be empty).</param>
    public SqlVariableDeclarationList( IEnumerable<ISqlNode> items )
        : base( null, 1, null, items, null )
    {
    }

    SqlVariableDeclarationList( SqlVariableDeclarationList o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
        : base( o, 1, leading, items, trailing )
    {
    }

    protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
    {
        return new SqlVariableDeclarationList( this, leading, content, trailing );
    }

    [DebuggerStepThrough]
    internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

}
