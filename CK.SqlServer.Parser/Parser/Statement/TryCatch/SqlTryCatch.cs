using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CK.SqlServer.Parser;

using CNode = SNode<SqlTryBlock, SqlCatchBlock, SqlTokenTerminal>;

/// <summary>
/// A try/catch block is defined by <see cref="SqlTryBlock"/> and <see cref="SqlCatchBlock"/>.
/// </summary>
public sealed class SqlTryCatch : SqlNonTokenAutoWidth, ISqlNamedStatement
{
    readonly CNode _content;

    public SqlTryCatch( SqlTryBlock tryBlock, SqlCatchBlock catchBlock, SqlTokenTerminal statementTerminator = null )
        : base( null, null )
    {
        _content = new CNode( tryBlock, catchBlock, statementTerminator );
        CheckContent();
    }

    SqlTryCatch( SqlTryCatch o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
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
        Helper.CheckNotNull( TryBlock, nameof( TryBlock ) );
        Helper.CheckNotNull( CatchBloct, nameof( CatchBloct ) );
    }

    protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
    {
        return new SqlTryCatch( this, leading, content, trailing );
    }

    public StatementKnownName StatementKnownName => StatementKnownName.TryCatch;

    public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

    public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

    public SqlTryBlock TryBlock => _content.V1;

    public SqlCatchBlock CatchBloct => _content.V2;

    public SqlTokenTerminal StatementTerminator => _content.V3;

    [DebuggerStepThrough]
    internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

}
