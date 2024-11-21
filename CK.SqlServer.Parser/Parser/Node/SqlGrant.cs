using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CK.SqlServer.Parser;

/// <summary>
/// Handles "GRANT perm TO target" statement. 
/// </summary>
public sealed class SqlGrant : SqlNonTokenAutoWidth, ISqlNamedStatement
{
    readonly SNode<SqlTokenIdentifier, SqlNodeList, SqlTokenIdentifier, SqlNodeList, SqlTokenTerminal> _content;

    public SqlGrant( SqlTokenIdentifier grantT, SqlNodeList perm, SqlTokenIdentifier toT, SqlNodeList target, SqlTokenTerminal term )
        : base( null, null )
    {
        _content = new SNode<SqlTokenIdentifier, SqlNodeList, SqlTokenIdentifier, SqlNodeList, SqlTokenTerminal>( grantT, perm, toT, target, term );
        CheckContent();
    }

    void CheckContent()
    {
        Helper.CheckToken( GrantT, nameof( GrantT ), SqlTokenType.Grant );
        Helper.CheckNotNull( Perm, nameof( Perm ) );
        Helper.CheckToken( ToT, nameof( ToT ), SqlTokenType.To );
        Helper.CheckNotNull( Target, nameof( Target ) );
    }

    SqlGrant( SqlGrant o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
        : base( leading, trailing )
    {
        if( items == null ) _content = o._content;
        else
        {
            _content = new SNode<SqlTokenIdentifier, SqlNodeList, SqlTokenIdentifier, SqlNodeList, SqlTokenTerminal>( items );
            CheckContent();
        }
    }

    protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
    {
        return new SqlGrant( this, leading, content, trailing );
    }

    public StatementKnownName StatementKnownName => StatementKnownName.Grant;

    public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

    public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

    public SqlTokenIdentifier GrantT => _content.V1;

    public SqlNodeList Perm => _content.V2;

    public SqlTokenIdentifier ToT => _content.V3;

    public SqlNodeList Target => _content.V4;

    public SqlTokenTerminal StatementTerminator => _content.V5;

    [DebuggerStepThrough]
    internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

}
