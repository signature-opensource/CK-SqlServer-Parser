using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CK.SqlServer.Parser;


using CNode = SNode<ISqlNode, SqlTokenIdentifier, SqlTokenIdentifier, SqlTokenIdentifier>;

/// <summary>
/// Operator ... is [not] null.
/// </summary>
public class SqlIsNull : SqlNonTokenAutoWidth
{
    readonly CNode _content;

    public SqlIsNull( ISqlNode left, SqlTokenIdentifier isT, SqlTokenIdentifier notT, SqlTokenIdentifier nullT )
        : base( null, null )
    {
        _content = new CNode( left, isT, notT, nullT );
        CheckContent();
    }

    void CheckContent()
    {
        Helper.CheckNotNull( Left, nameof( Left ) );
        Helper.CheckToken( IsT, nameof( IsT ), SqlTokenType.Is );
        Helper.CheckNullableToken( NotT, nameof( NotT ), SqlTokenType.Not );
        Helper.CheckToken( NullT, nameof( NullT ), SqlTokenType.Null );
    }

    SqlIsNull( SqlIsNull o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
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
        return new SqlIsNull( this, leading, content, trailing );
    }

    public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

    public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

    public ISqlNode Left => _content.V1;

    public SqlTokenIdentifier IsT => _content.V2;

    public bool IsNotNull => _content.V3 != null;

    public SqlTokenIdentifier NotT => _content.V3;

    public SqlTokenIdentifier NullT => _content.V4;

    [DebuggerStepThrough]
    internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

}
