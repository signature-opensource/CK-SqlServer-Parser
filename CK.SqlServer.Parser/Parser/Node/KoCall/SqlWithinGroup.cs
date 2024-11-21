using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CK.SqlServer.Parser;

using CNode = SNode<SqlTokenIdentifier, SqlTokenIdentifier, SqlTokenOpenPar, ISqlNode, SqlTokenClosePar>;

/// <summary>
/// Captures a 'within group' and select column definition. 
/// </summary>
public sealed class SqlWithinGroup : SqlNonTokenAutoWidth
{
    readonly CNode _content;

    public SqlWithinGroup( SqlTokenIdentifier withinT, SqlTokenIdentifier groupT, SqlTokenOpenPar opener, ISqlNode content, SqlTokenClosePar closer )
        : base( null, null )
    {
        _content = new CNode( withinT, groupT, opener, content, closer );
        CheckContent();
    }

    void CheckContent()
    {
        Helper.CheckToken( WithinT, nameof( WithinT ), SqlTokenType.Within );
        Helper.CheckToken( GroupT, nameof( GroupT ), SqlTokenType.Group );
        Helper.CheckNotNull( Opener, nameof( Opener ) );
        Helper.CheckNotNull( Content, nameof( Content ) );
        Helper.CheckNotNull( Closer, nameof( Closer ) );
    }

    SqlWithinGroup( SqlWithinGroup o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
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
        return new SqlWithinGroup( this, leading, content, trailing );
    }

    public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

    public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

    public SqlTokenIdentifier WithinT => _content.V1;

    public SqlTokenIdentifier GroupT => _content.V2;

    public SqlTokenOpenPar Opener => _content.V3;

    /// <summary>
    /// Gets the within group content. Can not be null. 
    /// </summary>
    public ISqlNode Content => _content.V4;

    public SqlTokenClosePar Closer => _content.V5;

    [DebuggerStepThrough]
    internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

}
