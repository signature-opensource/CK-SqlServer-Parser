using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CK.SqlServer.Parser;

public sealed class SqlTokenLiteralBinary : SqlTokenBaseLiteral
{
    public SqlTokenLiteralBinary( SqlTokenType t, string value, ImmutableList<SqlTrivia> leadingTrivia = null, ImmutableList<SqlTrivia> trailingTrivia = null )
        : base( t, leadingTrivia, trailingTrivia )
    {
        if( t != SqlTokenType.Binary ) throw new ArgumentException( "Invalid token type.", "t" );
        if( value == null ) throw new ArgumentNullException( "value" );
        Value = value;
    }

    public string Value { get; }

    public override string LiteralValue => Value;

    protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
    {
        return new SqlTokenLiteralBinary( TokenType, Value, leading, trailing );
    }

    [DebuggerStepThrough]
    internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

}
