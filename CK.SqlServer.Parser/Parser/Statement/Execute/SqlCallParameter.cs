using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CK.SqlServer.Parser;

using CNode = SNode<SqlTokenIdentifier, SqlTokenTerminal, ISqlNode, SqlTokenIdentifier>;

/// <summary>
/// Captures a call parameter.
/// </summary>
public sealed class SqlCallParameter : SqlNonTokenAutoWidth
{
    readonly CNode _content;

    public SqlCallParameter( SqlTokenIdentifier name, SqlTokenTerminal assignT, ISqlNode value, SqlTokenIdentifier outputT )
        : base( null, null )
    {
        _content = new CNode( name, assignT, value, outputT );
        CheckContent();
    }

    void CheckContent()
    {
        Helper.CheckNullableToken( Name, nameof( Name ), SqlTokenType.IdentifierVariable );
        Helper.CheckNullableToken( AsssignT, nameof( AsssignT ), SqlTokenType.Assign );
        Helper.CheckBothNullOrNot( Name, nameof( Name ), AsssignT, nameof( AsssignT ) );
        Helper.CheckNotNull( Value, nameof( Value ) );
        Helper.CheckNullableToken( OutputT, nameof( OutputT ), SqlTokenType.Output );
    }

    SqlCallParameter( SqlCallParameter o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
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
        return new SqlCallParameter( this, leading, content, trailing );
    }

    public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

    public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

    public SqlTokenIdentifier Name => _content.V1;

    public SqlToken AsssignT => _content.V2;

    public ISqlNode Value => _content.V3;

    public SqlTokenIdentifier OutputT => _content.V4;

    [DebuggerStepThrough]
    internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

}
