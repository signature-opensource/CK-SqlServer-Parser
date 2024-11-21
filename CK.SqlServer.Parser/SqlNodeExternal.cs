using System.Collections.Immutable;
using System.Diagnostics;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CK.SqlServer.Parser;

/// <summary>
/// Abstract class that can be used to extend the model with any type of nodes.
/// </summary>
public abstract class SqlNodeExternal : SqlNonTokenAutoWidth
{
    protected SqlNodeExternal( ImmutableList<SqlTrivia> leading = null, ImmutableList<SqlTrivia> trailing = null )
        : base( leading, trailing )
    {
    }

    [DebuggerStepThrough]
    internal protected override sealed ISqlNode Accept( SqlNodeVisitor visitor )
    {
        return visitor.Visit( this );
    }
}
