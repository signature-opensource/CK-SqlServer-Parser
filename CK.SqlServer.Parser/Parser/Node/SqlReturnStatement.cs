using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CK.SqlServer.Parser;

/// <summary>
/// 
/// </summary>
public sealed class SqlReturnStatement : SqlNonTokenAutoWidth, ISqlNamedStatement
{
    readonly SNode<SqlTokenIdentifier, ISqlNode, SqlTokenTerminal> _content;

    public SqlReturnStatement( SqlTokenIdentifier returnToken, ISqlNode value, SqlTokenTerminal terminator )
        : base( null, null )
    {
        _content = new SNode<SqlTokenIdentifier, ISqlNode, SqlTokenTerminal>( returnToken, value, terminator );
        CheckContent();
    }

    SqlReturnStatement( SqlReturnStatement o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
        : base( leading, trailing )
    {
        if( items == null ) _content = o._content;
        else
        {
            _content = new SNode<SqlTokenIdentifier, ISqlNode, SqlTokenTerminal>( items );
            CheckContent();
        }
    }

    protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
    {
        return new SqlReturnStatement( this, leading, content, trailing );
    }

    void CheckContent()
    {
        Helper.CheckToken( ReturnT, nameof( ReturnT ), SqlTokenType.Return );
    }

    public StatementKnownName StatementKnownName => StatementKnownName.Return;

    public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

    public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

    public SqlTokenIdentifier ReturnT => _content.V1;

    /// <summary>
    /// Gets the returned value. Can be null.
    /// </summary>
    public ISqlNode Value => _content.V2;

    public SqlTokenTerminal StatementTerminator => _content.V3;

    [DebuggerStepThrough]
    internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

}
