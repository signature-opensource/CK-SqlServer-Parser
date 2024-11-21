using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CK.SqlServer.Parser;


public sealed class SqlOutputClause : SqlNonTokenAutoWidth
{
    readonly SNode<SqlTokenIdentifier, SelectColumnList, SqlTokenIdentifier, ISqlIdentifier, SqlEnclosedIdentifierCommaList, SqlTokenIdentifier, SelectColumnList> _content;

    public SqlOutputClause(
        SqlTokenIdentifier outputT,
        SelectColumnList columns,
        SqlTokenIdentifier intoT,
        ISqlIdentifier targetTable,
        SqlEnclosedIdentifierCommaList columnNames,
        SqlTokenIdentifier outputT2,
        SelectColumnList columns2 )
        : base( null, null )
    {
        _content = new SNode<SqlTokenIdentifier, SelectColumnList, SqlTokenIdentifier, ISqlIdentifier, SqlEnclosedIdentifierCommaList, SqlTokenIdentifier, SelectColumnList>( outputT, columns, intoT, targetTable, columnNames, outputT2, columns2 );
        CheckContent();
    }

    void CheckContent()
    {
        Helper.CheckToken( OutputT, nameof( OutputT ), SqlTokenType.Output );
        Helper.CheckNotNull( Columns, nameof( Columns ) );
        Helper.CheckNullableToken( IntoT, nameof( IntoT ), SqlTokenType.Into );
        Helper.CheckBothNullOrNot( IntoT, nameof( IntoT ), TargetTable, nameof( TargetTable ) );
        Helper.CheckNullableToken( OutputT2, nameof( OutputT2 ), SqlTokenType.Output );
        Helper.CheckBothNullOrNot( OutputT2, nameof( OutputT2 ), Columns2, nameof( Columns2 ) );
    }

    SqlOutputClause( SqlOutputClause o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
        : base( leading, trailing )
    {
        if( items == null ) _content = o._content;
        else
        {
            _content = new SNode<SqlTokenIdentifier, SelectColumnList, SqlTokenIdentifier, ISqlIdentifier, SqlEnclosedIdentifierCommaList, SqlTokenIdentifier, SelectColumnList>( items );
            CheckContent();
        }
    }

    protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
    {
        return new SqlOutputClause( this, leading, content, trailing );
    }

    public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

    public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

    public SqlTokenIdentifier OutputT => _content.V1;

    public SelectColumnList Columns => _content.V2;

    public SqlTokenIdentifier IntoT => _content.V3;

    public bool HasTargetTable => _content.V4 != null;

    public ISqlIdentifier TargetTable => _content.V4;

    public SqlEnclosedIdentifierCommaList TargetTableColumnNames => _content.V5;

    /// <summary>
    /// Gets the optional second output clause after the output ... into clause.
    /// </summary>
    public SqlTokenIdentifier OutputT2 => _content.V6;

    public SelectColumnList Columns2 => _content.V7;

    [DebuggerStepThrough]
    internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

}
