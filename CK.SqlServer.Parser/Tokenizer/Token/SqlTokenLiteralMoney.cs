using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CK.SqlServer.Parser;

public sealed class SqlTokenLiteralMoney : SqlTokenBaseLiteral
{
    public SqlTokenLiteralMoney( SqlTokenType t, string value, ImmutableList<SqlTrivia> leadingTrivia = null, ImmutableList<SqlTrivia> trailingTrivia = null )
        : base( t, leadingTrivia, trailingTrivia )
    {
        if( t != SqlTokenType.Money ) throw new ArgumentException( "Invalid token type.", "t" );
        Value = value;
    }

    /// <summary>
    /// Money is kept as a string, it is not converted to a numeric .Net type.
    /// Since Money is actually a Int64 for sql server: we could handle the conversion here...
    /// </summary>
    public string Value { get; }

    public override string LiteralValue { get { return Value; } }

    protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
    {
        return new SqlTokenLiteralMoney( TokenType, Value, leading, trailing );
    }

    [DebuggerStepThrough]
    internal protected override ISqlNode Accept( SqlNodeVisitor visitor )
    {
        return visitor.Visit( this );
    }
}
