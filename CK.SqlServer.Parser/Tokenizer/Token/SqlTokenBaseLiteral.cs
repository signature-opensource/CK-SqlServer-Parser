using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Core;
using System.Collections.Immutable;

namespace CK.SqlServer.Parser;

/// <summary>
/// Base class for literal numbers (<see cref="SqlTokenLiteralBinary"/>, <see cref="SqlTokenLiteralDecimal"/>, <see cref="SqlTokenLiteralFloat"/>, 
/// <see cref="SqlTokenLiteralInteger"/>, <see cref="SqlTokenLiteralMoney"/>)
/// and strings <see cref="SqlTokenLiteralString"/> (either N'unicode' or 'ansi').
/// </summary>
public abstract class SqlTokenBaseLiteral : SqlToken
{
    /// <summary>
    /// Initializes a new <see cref="SqlTokenBaseLiteral"/>.
    /// </summary>
    /// <param name="t">The token type (must be <see cref="SqlTokenType.IsString"/> or <see cref="SqlTokenType.IsNumber"/>).</param>
    /// <param name="leadingTrivia">The leading trivias.</param>
    /// <param name="trailingTrivia">The trailing trivias.</param>
    public SqlTokenBaseLiteral( SqlTokenType t, ImmutableList<SqlTrivia>? leadingTrivia = null, ImmutableList<SqlTrivia>? trailingTrivia = null )
        : base( t, leadingTrivia, trailingTrivia )
    {
        Throw.CheckArgument( "Invalid literal token.", (t & (SqlTokenType.IsString | SqlTokenType.IsNumber)) != 0 );
    }

    /// <summary>
    /// Gets the literal form of this token.
    /// This may be a slightly modified string as the raw, original, text.
    /// </summary>
    public abstract string LiteralValue { get; }

    /// <inheritdoc />
    public override bool TokenEquals( SqlToken t ) => t is SqlTokenBaseLiteral && LiteralValue == t.ToString();

    /// <summary>
    /// By default, the string is the <see cref="LiteralValue"/>.
    /// </summary>
    /// <returns></returns>
    public sealed override string ToString() => LiteralValue;

    /// <summary>
    /// Simply appends the <see cref="LiteralValue"/>.
    /// </summary>
    /// <param name="w">The builder to use.</param>
    public override void WriteWithoutTrivias( ISqlTextWriter w ) => w.Write( TokenType, LiteralValue );

}
