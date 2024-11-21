using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CK.SqlServer.Parser;

/// <summary>
/// Specific <see cref="SqlTokenTerminal"/> for <see cref="SqlTokenType.QuestionMark"/>, <see cref="SqlTokenType.DoubleQuestionMark"/> 
/// and <see cref="SqlTokenType.TripleQuestionMark"/>.
/// </summary>
public sealed class SqlTokenQuestionMarks : SqlTokenTerminal
{
    public SqlTokenQuestionMarks( SqlTokenType tokenType, ImmutableList<SqlTrivia> leadingTrivia = null, ImmutableList<SqlTrivia> trailingTrivia = null )
        : base( tokenType, leadingTrivia, trailingTrivia )
    {
        if( tokenType != SqlTokenType.QuestionMark
            && tokenType != SqlTokenType.DoubleQuestionMark
            && tokenType != SqlTokenType.TripleQuestionMark ) throw new ArgumentException();
    }

    public override void WriteWithoutTrivias( ISqlTextWriter w )
    {
        Debug.Assert( SqlKeyword.ToString( SqlTokenType.QuestionMark ) == "?" );
        Debug.Assert( SqlKeyword.ToString( SqlTokenType.DoubleQuestionMark ) == "??" );
        Debug.Assert( SqlKeyword.ToString( SqlTokenType.TripleQuestionMark ) == "???" );
        w.Write( TokenType, ToString(), whiteSpaceBefore: false, whiteSpaceAfter: false );
    }

    public override string ToString() => TokenType == SqlTokenType.QuestionMark
                                            ? "?"
                                            : (TokenType == SqlTokenType.DoubleQuestionMark
                                                ? "??"
                                                : "???");

    protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
    {
        return new SqlTokenMultiDots( TokenType, leading, trailing );
    }
}
