using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CK.SqlServer.Parser;


/// <summary>
/// 
/// </summary>
public sealed class SqlLike : SqlNonTokenAutoWidth
{
    readonly SNode<ISqlNode, SqlTokenIdentifier, SqlTokenIdentifier, ISqlNode, SqlTokenIdentifier, SqlTokenLiteralString> _content;

    public SqlLike( ISqlNode left, SqlTokenIdentifier notToken, SqlTokenIdentifier likeToken, ISqlNode pattern, SqlTokenIdentifier escapeToken = null, SqlTokenLiteralString escapeChar = null )
        : base( null, null )
    {
        _content = new SNode<ISqlNode, SqlTokenIdentifier, SqlTokenIdentifier, ISqlNode, SqlTokenIdentifier, SqlTokenLiteralString>( left, notToken, likeToken, pattern, escapeToken, escapeChar );
        CheckContent();
    }

    void CheckContent()
    {
        Helper.CheckNotNull( Left, nameof( Left ) );
        Helper.CheckNullableToken( NotT, nameof( NotT ), SqlTokenType.Not );
        Helper.CheckToken( LikeT, nameof( LikeT ), SqlTokenType.Like );
        Helper.CheckNotNull( Pattern, nameof( Pattern ) );
        Helper.CheckNullableToken( EscapeT, nameof( EscapeT ), SqlTokenType.Escape );
        Helper.CheckBothNullOrNot( EscapeT, nameof( EscapeT ), EscapeChar, nameof( EscapeChar ) );
    }

    SqlLike( SqlLike o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
        : base( leading, trailing )
    {
        if( items == null ) _content = o._content;
        else
        {
            _content = new SNode<ISqlNode, SqlTokenIdentifier, SqlTokenIdentifier, ISqlNode, SqlTokenIdentifier, SqlTokenLiteralString>( items );
            CheckContent();
        }
    }

    protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
    {
        return new SqlLike( this, leading, content, trailing );
    }

    public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

    public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

    public ISqlNode Left => _content.V1;

    public bool IsNotLike => _content.V2 != null;

    public SqlTokenIdentifier NotT => _content.V2;

    public SqlTokenIdentifier LikeT => _content.V3;

    public ISqlNode Pattern => _content.V4;

    public bool HasEscape => _content.V5 != null;

    public SqlTokenIdentifier EscapeT => _content.V5;

    public SqlTokenLiteralString EscapeChar => _content.V6;

    [DebuggerStepThrough]
    internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

}
