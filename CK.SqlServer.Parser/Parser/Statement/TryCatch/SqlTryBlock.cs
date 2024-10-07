using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CK.SqlServer.Parser;

using CNode = SNode<SqlTokenIdentifier, SqlTokenIdentifier, SqlStatementList, SqlTokenIdentifier, SqlTokenIdentifier>;

/// <summary>
/// A try/catch block is defined by begin try...end try begin catch...end catch.
/// </summary>
public sealed class SqlTryBlock : SqlNonTokenAutoWidth, ISqlStatementPart
{
    readonly CNode _content;

    public SqlTryBlock( SqlTokenIdentifier beginT, SqlTokenIdentifier tryT,
                              SqlStatementList body,
                              SqlTokenIdentifier endT, SqlTokenIdentifier endTryT )
        : base( null, null )
    {
        _content = new CNode( beginT, tryT, body, endT, endTryT );
        CheckContent();
    }

    SqlTryBlock( SqlTryBlock o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
        : base( leading, trailing )
    {
        if( items == null ) _content = o._content;
        else
        {
            _content = new CNode( items );
            CheckContent();
        }
    }

    void CheckContent()
    {
        Helper.CheckToken( BeginT, nameof( BeginT ), SqlTokenType.Begin );
        Helper.CheckToken( TryT, nameof( TryT ), SqlTokenType.Try );

        Helper.CheckNotNull( Body, nameof( Body ) );

        Helper.CheckToken( EndT, nameof( EndT ), SqlTokenType.End );
        Helper.CheckToken( EndTryT, nameof( EndTryT ), SqlTokenType.Try );
    }

    protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
    {
        return new SqlTryBlock( this, leading, content, trailing );
    }

    public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

    public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

    public SqlTokenIdentifier BeginT => _content.V1;

    public SqlTokenIdentifier TryT => _content.V2;

    public SqlStatementList Body => _content.V3;

    public SqlTokenIdentifier EndT => _content.V4;

    public SqlTokenIdentifier EndTryT => _content.V5;

    [DebuggerStepThrough]
    internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

}
