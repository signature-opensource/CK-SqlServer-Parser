using CK.Core;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CK.SqlServer.Parser;

public sealed class SqlUpdateStatement : SqlNonTokenAutoWidth, ISqlNamedStatement
{
    readonly SNode<
        MIUDHeader,
        IUDTarget,
        SqlTokenIdentifier,
        SqlCommaList,
        SqlOutputClause,
        SelectFrom,
        SqlTokenIdentifier,
        ISqlNode,
        SqlOptionParOptions,
        SqlTokenTerminal> _content;

    public SqlUpdateStatement(
        MIUDHeader header,
        IUDTarget target,
        SqlTokenIdentifier setT,
        SqlCommaList assigns,
        SqlOutputClause outputClause,
        SelectFrom from,
        SqlTokenIdentifier whereT,
        ISqlNode whereExpression,
        SqlOptionParOptions options,
        SqlTokenTerminal terminator )
        : base( null, null )
    {
        _content = new SNode<MIUDHeader, IUDTarget, SqlTokenIdentifier, SqlCommaList, SqlOutputClause, SelectFrom, SqlTokenIdentifier, ISqlNode, SqlOptionParOptions, SqlTokenTerminal>(
            header,
            target,
            setT,
            assigns,
            outputClause,
            from,
            whereT,
            whereExpression,
            options,
            terminator );
        CheckContent();
    }

    void CheckContent()
    {
        Helper.CheckNotNull( Header, nameof( Header ) );
        Helper.CheckNotNull( Target, nameof( Target ) );
        Helper.CheckToken( SetT, nameof( SetT ), SqlTokenType.Set );
        Helper.CheckNotNull( Assigns, nameof( Assigns ) );
        Helper.CheckNullableToken( WhereT, nameof( WhereT ), SqlTokenType.Where );
        Helper.CheckBothNullOrNot( WhereT, nameof( WhereT ), WhereExpression, nameof( WhereExpression ) );
    }

    SqlUpdateStatement( SqlUpdateStatement o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
        : base( leading, trailing )
    {
        if( items == null ) _content = o._content;
        else
        {
            _content = new SNode<MIUDHeader, IUDTarget, SqlTokenIdentifier, SqlCommaList, SqlOutputClause, SelectFrom, SqlTokenIdentifier, ISqlNode, SqlOptionParOptions, SqlTokenTerminal>( items );
            CheckContent();
        }
    }

    protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
    {
        return new SqlUpdateStatement( this, leading, content, trailing );
    }

    public StatementKnownName StatementKnownName => StatementKnownName.Update;

    public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

    public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

    public MIUDHeader Header => _content.V1;

    public IUDTarget Target => _content.V2;

    public SqlTokenIdentifier SetT => _content.V3;

    public SqlCommaList Assigns => _content.V4;

    public SqlUpdateStatement AddColumns( IEnumerable<SelectColumn> columns )
    {
        SqlCommaList assignments = _content.V4;
        foreach( var c in columns )
        {
            Throw.CheckNotNullArgument( "Columns must have a name.", c.ColumnName );
            var eqC = c.ToEqualSyntax();
            assignments = assignments.InsertAt( assignments.Count, eqC );
        }
        return this.ReplaceContentNode( 3, assignments );
    }

    public bool HasOutputClause => _content.V5 != null;

    public SqlOutputClause OutputClause => _content.V5;

    public bool HasFrom => _content.V6 != null;

    public SelectFrom From => _content.V6;

    public bool HasWhere => _content.V7 != null;

    public SqlTokenIdentifier WhereT => _content.V7;

    public ISqlNode WhereExpression => _content.V8;

    public bool HasOptions => _content.V9 != null;

    public SqlOptionParOptions Options => _content.V9;

    public SqlTokenTerminal StatementTerminator => _content.V10;

    [DebuggerStepThrough]
    internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

}
