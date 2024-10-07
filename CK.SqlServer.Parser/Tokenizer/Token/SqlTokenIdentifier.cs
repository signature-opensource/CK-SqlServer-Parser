using CK.Core;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CK.SqlServer.Parser;

/// <summary>
/// Token for identifiers.
/// </summary>
public sealed class SqlTokenIdentifier : SqlToken, ISqlIdentifier, IReadOnlyList<ISqlIdentifier>, ISqlHasStringValue
{
    readonly string _name;

    public SqlTokenIdentifier( SqlTokenType t, string name, ImmutableList<SqlTrivia> leadingTrivia = null, ImmutableList<SqlTrivia> trailingTrivia = null )
        : base( t, leadingTrivia, trailingTrivia )
    {
        if( (t & SqlTokenType.IsIdentifier) == 0 ) throw new ArgumentException( "Invalid token type.", "t" );
        if( name == null ) throw new ArgumentNullException( "name" );
        if( IsVariable && name[0] != '@' ) throw new ArgumentException( "Invalid variable name.", "name" );
        _name = name;
    }

    /// <summary>
    /// True if this <see cref="SqlTokenIdentifier"/> is a @Variable or a @@SystemFunction.
    /// </summary>
    public bool IsVariable => TokenType.IsVariable();

    bool ISqlIdentifier.IsOpenDataSource => false;

    IReadOnlyList<ISqlIdentifier> ISqlIdentifier.Identifiers => this;

    int IReadOnlyCollection<ISqlIdentifier>.Count => 1;

    ISqlIdentifier IReadOnlyList<ISqlIdentifier>.this[int index]
    {
        get
        {
            if( index != 0 ) throw new IndexOutOfRangeException();
            return this;
        }
    }

    IEnumerator<ISqlIdentifier> IEnumerable<ISqlIdentifier>.GetEnumerator()
    {
        return new CKEnumeratorMono<SqlTokenIdentifier>( this );
    }

    /// <summary>
    /// Two identifiers are equal if they have the same <see cref="SqlToken.TokenType"/> and <see cref="Name"/>.
    /// </summary>
    /// <param name="t">The other token to test.</param>
    /// <returns>True if the this identifier is equal to the other one.</returns>
    public override bool TokenEquals( SqlToken t )
    {
        SqlTokenIdentifier id = t as SqlTokenIdentifier;
        return id != null && TokenType == id.TokenType && _name == id._name;
    }

    public SqlTokenIdentifier RemoveQuoteIfPossible( bool keepIfReservedKeyword )
    {
        // Already quotes free.
        if( !TokenType.IsQuotedIdentifier() ) return this;

        // Quotes exist.

        // Are quotes required? If yes, don't do it.
        if( SqlToken.IsQuoteRequired( _name ) ) return this;

        // If it is a known (reserved) keyword and it must be preserved, do not do anything.
        SqlTokenType typeWithoutQuote;
        bool isReservedKeyWord = SqlKeyword.IsReservedKeyword( _name, out typeWithoutQuote );
        if( keepIfReservedKeyword && isReservedKeyWord ) return this;
        if( typeWithoutQuote == SqlTokenType.None ) typeWithoutQuote = SqlTokenType.IdentifierStandard;

        return new SqlTokenIdentifier( typeWithoutQuote, _name, LeadingTrivias, TrailingTrivias );
    }

    /// <summary>
    /// Gets the identifier string (without quotes or brackets if this is quoted).
    /// </summary>
    public string Name => _name;

    /// <summary>
    /// Gets the string value (i.e. the <see cref="Name"/>).
    /// This unifies the [] and "" quoted identifiers with '' litteral strings.
    /// </summary>
    string ISqlHasStringValue.Value => _name;

    /// <summary>
    /// Sets this identifier name.
    /// </summary>
    /// <param name="name">Name to set (must not be null nor empty).</param>
    /// <param name="quoteReservedKeyword">False to not quote a reserved keyword. By default, a reserved keyword will be [quoted].</param>
    /// <returns>This or a new identifier with the same trivias as this one.</returns>
    public SqlTokenIdentifier SetName( string name, bool quoteReservedKeyword = true )
    {
        return name == _name ? this : Create( name, quoteReservedKeyword, LeadingTrivias, TrailingTrivias );
    }

    /// <summary>
    /// Creates an identifier form a name. It will be [quoted] as needed.
    /// </summary>
    /// <param name="name">Name (must not be null nor empty).</param>
    /// <param name="quoteReservedKeyword">False to not quote a reserved keyword. By default, a reserved keyword will be [quoted].</param>
    /// <param name="leading">Optional leading trivias.</param>
    /// <param name="trailing">Optional trailing trivias.</param>
    /// <returns>A new identifier.</returns>
    public static SqlTokenIdentifier Create( string name, bool quoteReservedKeyword = true, ImmutableList<SqlTrivia> leading = null, ImmutableList<SqlTrivia> trailing = null )
    {
        if( name == null ) throw new ArgumentNullException( nameof( name ) );
        SqlTokenType knownType = SqlTokenType.None;
        bool isReservedKeyWord = name.Length > 0 && SqlKeyword.IsReservedKeyword( name, out knownType );

        if( (quoteReservedKeyword && isReservedKeyWord) || IsQuoteRequired( name ) )
        {
            return new SqlTokenIdentifier( SqlTokenType.IdentifierQuotedBracket, name, leading, trailing );
        }
        if( knownType == SqlTokenType.None ) knownType = SqlTokenType.IdentifierStandard;
        return new SqlTokenIdentifier( knownType, name, leading, trailing );
    }

    public ISqlIdentifier SetPartName( int idxPart, string name )
    {
        if( idxPart <= 0 || idxPart > 4 ) throw new ArgumentException( "Must be between 1 and 4.", nameof( idxPart ) );
        if( name == null ) throw new InvalidOperationException();
        int idx = 1 - idxPart;
        if( idx < -1 ) throw new ArgumentException( nameof( idxPart ) );
        return idx == -1
                ? (ISqlIdentifier)new SqlMultiIdentifier( new ISqlNode[] { Create( name ), SqlKeyword.Dot, this } )
                : SetName( name );
    }

    protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
    {
        return new SqlTokenIdentifier( TokenType, _name, leading, trailing );
    }

    public override string ToString()
    {
        switch( TokenType )
        {
            case SqlTokenType.IdentifierQuoted:
                return "\"" + _name.Replace( "\"", "\"\"" ) + "\"";
            case SqlTokenType.IdentifierQuotedBracket:
                return "[" + _name.Replace( "]", "]]" ) + "]";
            default: return _name;
        }
    }

    public override void WriteWithoutTrivias( ISqlTextWriter w ) => w.Write( this );

    [DebuggerStepThrough]
    internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

}
