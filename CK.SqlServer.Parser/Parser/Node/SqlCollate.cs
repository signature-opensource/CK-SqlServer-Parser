using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CK.SqlServer.Parser;

using CNode = SNode<ISqlNode, SqlTokenIdentifier, SqlTokenIdentifier>;

public sealed class SqlCollate : SqlNonTokenAutoWidth
{
    readonly CNode _content;

    public SqlCollate( ISqlNode left, SqlTokenIdentifier collateT, SqlTokenIdentifier nameT )
        : base( null, null )
    {
        _content = new CNode( left, collateT, nameT );
        CheckContent();
    }

    void CheckContent()
    {
        Helper.CheckNotNull( Left, nameof( Left ) );
        Helper.CheckToken( CollateT, nameof( CollateT ), SqlTokenType.Collate );
        Helper.CheckNotNull( CollationName, nameof( CollationName ) );
    }

    SqlCollate( SqlCollate o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
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
        return new SqlCollate( this, leading, content, trailing );
    }

    public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

    public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

    public ISqlNode Left => _content.V1;

    public SqlTokenIdentifier CollateT => _content.V2;

    public SqlTokenIdentifier CollationName => _content.V3;

    [DebuggerStepThrough]
    internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

}
