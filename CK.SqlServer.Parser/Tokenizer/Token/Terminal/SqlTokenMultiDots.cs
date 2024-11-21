using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CK.SqlServer.Parser;

/// <summary>
/// Specific <see cref="SqlTokenTerminal"/> for <see cref="SqlTokenType.DoubleDots"/> and <see cref="SqlTokenType.TripleDots"/>.
/// </summary>
public sealed class SqlTokenMultiDots : SqlTokenTerminal, ISqlTokenIdentifierSeparator
{
    public SqlTokenMultiDots( SqlTokenType tokenType, ImmutableList<SqlTrivia> leadingTrivia = null, ImmutableList<SqlTrivia> trailingTrivia = null )
        : base( tokenType, leadingTrivia, trailingTrivia )
    {
        if( tokenType != SqlTokenType.DoubleDots && tokenType != SqlTokenType.TripleDots ) throw new ArgumentException();
    }

    public override void WriteWithoutTrivias( ISqlTextWriter w )
    {
        Debug.Assert( SqlKeyword.ToString( SqlTokenType.DoubleDots ) == ".." );
        Debug.Assert( SqlKeyword.ToString( SqlTokenType.TripleDots ) == "..." );
        w.Write( TokenType, ToString(), whiteSpaceBefore: false, whiteSpaceAfter: false );
    }

    public override string ToString() => TokenType == SqlTokenType.DoubleColons ? ".." : "...";

    protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
    {
        return new SqlTokenMultiDots( TokenType, leading, trailing );
    }
}
