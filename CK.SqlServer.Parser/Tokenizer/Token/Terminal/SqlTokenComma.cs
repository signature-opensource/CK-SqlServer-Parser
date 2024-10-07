using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CK.SqlServer.Parser;

/// <summary>
/// Specific <see cref="SqlTokenTerminal"/> for <see cref="SqlTokenType.Comma"/>.
/// </summary>
public sealed class SqlTokenComma : SqlTokenTerminal
{
    public SqlTokenComma( ImmutableList<SqlTrivia> leadingTrivia = null, ImmutableList<SqlTrivia> trailingTrivia = null )
        : base( SqlTokenType.Comma, leadingTrivia, trailingTrivia )
    {
    }

    public override void WriteWithoutTrivias( ISqlTextWriter w )
    {
        Debug.Assert( SqlKeyword.ToString( SqlTokenType.Comma ) == "," );
        w.Write( SqlTokenType.Comma, ",", whiteSpaceBefore: false, whiteSpaceAfter: true );
    }

    public override string ToString() => ",";

    protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
    {
        return new SqlTokenComma( leading, trailing );
    }
}
