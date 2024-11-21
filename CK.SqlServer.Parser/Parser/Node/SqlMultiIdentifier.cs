using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CK.SqlServer.Parser;


public sealed class SqlMultiIdentifier : ASqlNodeSeparatedList<ISqlIdentifier, ISqlTokenIdentifierSeparator>, ISqlIdentifier
{
    /// <summary>
    /// Initializes a new <see cref="SqlMultiIdentifier"/>.
    /// </summary>
    /// <param name="tokens">Identifiers and separator tokens.</param>
    public SqlMultiIdentifier( IEnumerable<ISqlNode> tokens )
        : this( null, null, tokens, null )
    {
    }

    SqlMultiIdentifier( SqlMultiIdentifier o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
        : base( o, 1, leading, items, trailing )
    {
    }

    protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
    {
        return new SqlMultiIdentifier( this, leading, content, trailing );
    }
    public bool IsVariable => this[0].IsVariable;

    public bool IsOpenDataSource => this[0].IsOpenDataSource;

    IReadOnlyList<ISqlIdentifier> ISqlIdentifier.Identifiers => this;

    public ISqlIdentifier SetPartName( int idxPart, string name )
    {
        if( idxPart <= 0 || idxPart > 4 ) throw new ArgumentException( "Must be between 1 and 4.", nameof( idxPart ) );
        if( IsOpenDataSource ) throw new InvalidOperationException();
        int idx = Count - idxPart;
        if( name == null )
        {
            if( idx < 0 ) throw new ArgumentException( nameof( idxPart ) );
            return (ISqlIdentifier)DoReplaceItems( ( i, n ) => i == idx ? null : n );
        }
        if( idx < -1 ) throw new ArgumentException( nameof( idxPart ) );
        return idx == -1
                ? (ISqlIdentifier)DoInsertAt( 0, SqlTokenIdentifier.Create( name ) )
                : (ISqlIdentifier)DoReplaceItems( ( i, n ) => i == idx ? n.SetPartName( 1, name ) : n );
    }

    protected override ISqlTokenIdentifierSeparator CreateDefaultSeparator() => SqlKeyword.Dot;

    [DebuggerStepThrough]
    internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

}
