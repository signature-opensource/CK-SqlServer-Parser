using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CK.SqlServer.Parser;

using CNode = SNode<SqlTokenOpenPar, ISqlNode, SqlTokenClosePar>;

public sealed class SqlPar : SqlNonTokenAutoWidth
{
    readonly CNode _content;

    public SqlPar( SqlTokenOpenPar opener, ISqlNode content, SqlTokenClosePar closer, ImmutableList<SqlTrivia> leading = null, ImmutableList<SqlTrivia> trailing = null )
        : base( leading, trailing )
    {
        _content = new CNode( opener, content, closer );
        CheckContent();
    }

    void CheckContent()
    {
        Helper.CheckNotNull( Opener, nameof( Opener ) );
        Helper.CheckNotNull( Content, nameof( Content ) );
        Helper.CheckNotNull( Closer, nameof( Closer ) );
    }

    SqlPar( SqlPar o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
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
        return new SqlPar( this, leading, content, trailing );
    }

    public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

    public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

    public override ISqlNode UnPar => Content.UnPar;

    public SqlTokenOpenPar Opener => _content.V1;

    public ISqlNode Content => _content.V2;

    public SqlTokenClosePar Closer => _content.V3;

    [DebuggerStepThrough]
    internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );
}
