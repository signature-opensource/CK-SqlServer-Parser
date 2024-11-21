using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CK.SqlServer.Parser;

using CNode = SNode<SqlTokenIdentifier, SqlTokenIdentifier, SqlNodeList>;

/// <summary>
/// Captures the optional "For xml, browse, json or system_time" select part.
/// </summary>
public sealed class SelectFor : SqlNonTokenAutoWidth
{
    readonly CNode _content;

    public SelectFor( SqlTokenIdentifier forT, SqlTokenIdentifier targetType, SqlNodeList content )
        : base( null, null )
    {
        _content = new CNode( forT, targetType, content );
        CheckContent();
    }

    void CheckContent()
    {
        Helper.CheckToken( ForT, nameof( ForT ), SqlTokenType.For );
        Helper.CheckToken( TargetType, nameof( TargetType ),
            SqlTokenType.XmlDbType,
            SqlTokenType.Browse,
            SqlTokenType.Json,
            SqlTokenType.SystemTime );
        Helper.CheckNotNull( TargetType, nameof( TargetType ) );
        Helper.CheckNotNull( Format, nameof( Format ) );
    }

    SelectFor( SelectFor o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
        : base( leading, trailing )
    {
        if( items == null ) _content = o._content;
        else
        {
            _content = new CNode( items );
            CheckContent();
        }
    }

    protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
    {
        return new SelectFor( this, leading, content, trailing );
    }

    public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

    public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

    public SqlTokenIdentifier ForT => _content.V1;

    public SqlTokenIdentifier TargetType => _content.V2;

    public SqlNodeList Format => _content.V3;

    [DebuggerStepThrough]
    internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

}
