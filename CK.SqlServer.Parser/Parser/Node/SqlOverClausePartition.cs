using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CK.SqlServer.Parser;

using CNode = SNode<SqlTokenIdentifier, SqlTokenIdentifier, SqlCommaList>;

/// <summary>
/// Captures the partition_by_clause of the OVER clause.
/// See https://docs.microsoft.com/en-us/sql/t-sql/queries/select-over-clause-transact-sql#syntax
/// </summary>
public sealed class SqlOverClausePartition : SqlNonTokenAutoWidth
{
    readonly CNode _content;

    public SqlOverClausePartition( SqlTokenIdentifier partitionT, SqlTokenIdentifier byT, SqlCommaList expressions )
        : base( null, null )
    {
        _content = new CNode( partitionT, byT, expressions );
        CheckContent();
    }

    void CheckContent()
    {
        Helper.CheckToken( PartitionT, nameof( PartitionT ), SqlTokenType.Partition );
        Helper.CheckToken( ByT, nameof( ByT ), SqlTokenType.By );
        Helper.CheckNotNull( Expressions, nameof( Expressions ) );
    }

    SqlOverClausePartition( SqlOverClausePartition o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
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
        return new SqlOverClausePartition( this, leading, content, trailing );
    }

    public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

    public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

    public SqlTokenIdentifier PartitionT => _content.V1;

    public SqlTokenIdentifier ByT => _content.V2;

    /// <summary>
    /// Gets the over clause content. May be null. 
    /// </summary>
    public SqlCommaList Expressions => _content.V3;

    [DebuggerStepThrough]
    internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

}
