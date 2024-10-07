using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CK.SqlServer.Parser;

public sealed class SqlTokenLiteralDecimal : SqlTokenBaseLiteral
{
    public SqlTokenLiteralDecimal( SqlTokenType t, string value, ImmutableList<SqlTrivia> leadingTrivia = null, ImmutableList<SqlTrivia> trailingTrivia = null )
        : base( t, leadingTrivia, trailingTrivia )
    {
        if( t != SqlTokenType.Decimal ) throw new ArgumentException( "Invalid token type.", "t" );
        if( value == null ) throw new ArgumentNullException( "value" );
        ValueAsString = value;
        int precision, scale;

        int iDot = value.IndexOf( '.' );
        if( iDot >= 0 )
        {
            precision = value.Length - 1;
            if( iDot == 1 && value[0] == '0' ) --precision;
            scale = precision - iDot;
        }
        else
        {
            precision = value.Length;
            scale = 0;
        }
        Precision = (byte)precision;
        Scale = (byte)scale;
        Decimal d;
        IsValidDecimalValue = Decimal.TryParse( value, NumberStyles.Number, CultureInfo.InvariantCulture, out d );
        DecimalValue = d;
    }

    SqlTokenLiteralDecimal( SqlTokenLiteralDecimal x, ImmutableList<SqlTrivia> leadingTrivia, ImmutableList<SqlTrivia> trailingTrivia )
     : base( x.TokenType, leadingTrivia, trailingTrivia )
    {
        ValueAsString = x.ValueAsString;
        DecimalValue = x.DecimalValue;
        IsValidDecimalValue = x.IsValidDecimalValue;
        Precision = x.Precision;
        Scale = x.Scale;
    }

    /// <summary>
    /// Decimal is kept as a string, it is not converted to a numeric .Net type.
    /// Since .Net <see cref="Decimal"/> type has only 28 digits whereas Sql server numerics has 38.
    /// </summary>
    public string ValueAsString { get; private set; }

    /// <summary>
    /// Decimal value parsed if <see cref="IsValidDecimalValue"/> is true. 0 otherwise.
    /// </summary>
    public Decimal DecimalValue { get; private set; }

    /// <summary>
    /// Whether <see cref="DecimalValue"/> has been successfully parsed into a <see cref="Decimal"/> .Net type.
    /// </summary>
    public bool IsValidDecimalValue { get; private set; }

    /// <summary>
    /// Gets the number of digits.
    /// </summary>
    public byte Precision { get; private set; }

    /// <summary>
    /// Gets the number of fractional digits.
    /// </summary>
    public byte Scale { get; private set; }

    /// <summary>
    /// Gets the <see cref="ValueAsString"/>.
    /// </summary>
    public override string LiteralValue => ValueAsString;

    protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
    {
        return new SqlTokenLiteralDecimal( this, leading, trailing );
    }

    [DebuggerStepThrough]
    internal protected override ISqlNode Accept( SqlNodeVisitor visitor )
    {
        return visitor.Visit( this );
    }
}
