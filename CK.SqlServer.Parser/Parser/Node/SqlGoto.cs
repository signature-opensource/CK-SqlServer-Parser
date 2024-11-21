using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CK.SqlServer.Parser;

/// <summary>
/// 
/// </summary>
public sealed class SqlGoto : SqlNonTokenAutoWidth, ISqlNamedStatement
{
    readonly SNode<SqlTokenIdentifier, SqlTokenIdentifier, SqlTokenTerminal> _content;

    public SqlGoto( SqlTokenIdentifier gotoToken, SqlTokenIdentifier target, SqlTokenTerminal terminator )
        : base( null, null )
    {
        _content = new SNode<SqlTokenIdentifier, SqlTokenIdentifier, SqlTokenTerminal>( gotoToken, target, terminator );
        CheckContent();
    }

    void CheckContent()
    {
        Helper.CheckToken( GotoT, nameof( GotoT ), SqlTokenType.Goto );
        Helper.CheckNotNull( Target, nameof( Target ) );
    }

    SqlGoto( SqlGoto o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
        : base( leading, trailing )
    {
        if( items == null ) _content = o._content;
        else
        {
            _content = new SNode<SqlTokenIdentifier, SqlTokenIdentifier, SqlTokenTerminal>( items );
            CheckContent();
        }
    }

    protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
    {
        return new SqlGoto( this, leading, content, trailing );
    }

    public StatementKnownName StatementKnownName => StatementKnownName.Goto;

    public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

    public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

    public SqlTokenIdentifier GotoT => _content.V1;

    public SqlTokenIdentifier Target => _content.V2;

    public SqlTokenTerminal StatementTerminator => _content.V3;

    [DebuggerStepThrough]
    internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

}
