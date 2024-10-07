using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CK.SqlServer.Parser;

public sealed class SqlTokenLiteralString : SqlTokenBaseLiteral, ISqlHasStringValue
{
    public SqlTokenLiteralString( SqlTokenType t, string value, ImmutableList<SqlTrivia> leadingTrivia = null, ImmutableList<SqlTrivia> trailingTrivia = null )
        : base( t, leadingTrivia, trailingTrivia )
    {
        if( (t & SqlTokenType.IsString) == 0 ) throw new ArgumentException( "Invalid token type.", "t" );
        if( value == null ) throw new ArgumentNullException( "value" );
        Value = value;
    }

    public bool IsUnicode => TokenType == SqlTokenType.UnicodeString;

    public string Value { get; }

    public override string LiteralValue => string.Format( IsUnicode ? "N'{0}'" : "'{0}'", Value.Replace( "'", "''" ) );

    protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
    {
        return new SqlTokenLiteralString( TokenType, Value, leading, trailing );
    }

    [DebuggerStepThrough]
    internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

}
