using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CK.SqlServer.Parser;

using CNode = SNode<
        SqlTokenIdentifier,
        SqlEnclosedIdentifierCommaList,
        SqlTokenIdentifier,
        SqlTokenOpenPar,
        ISqlNode,
        SqlTokenClosePar>;

/// <summary>
/// Defines named selects in a <see cref="SqlCTEStatement"/>.
/// </summary>
public sealed class SqlCTEName : SqlNonTokenAutoWidth
{
    readonly CNode _content;

    public SqlCTEName(
            SqlTokenIdentifier name,
            SqlEnclosedIdentifierCommaList optionalColumnNames,
            SqlTokenIdentifier asT,
            SqlTokenOpenPar opener,
            ISqlNode selectNode,
            SqlTokenClosePar closer )
        : base( null, null )
    {
        _content = new CNode( name, optionalColumnNames, asT, opener, selectNode, closer );
        CheckContent();
    }

    void CheckContent()
    {
        Helper.CheckNotNull( Name, nameof( Name ) );
        Helper.CheckToken( AsT, nameof( AsT ), SqlTokenType.As );
        Helper.CheckNotNull( Opener, nameof( Opener ) );
        Helper.CheckUnPar<ISelectSpecification>( SelectNode, nameof( SelectNode ) );
        Helper.CheckNotNull( Closer, nameof( Closer ) );
    }

    SqlCTEName( SqlCTEName o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
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
        return new SqlCTEName( this, leading, content, trailing );
    }

    public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

    public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

    public SqlTokenIdentifier Name => _content.V1;

    public bool HasColumnNames => _content.V2 != null;

    public SqlEnclosedIdentifierCommaList ColumnNames => _content.V2;

    public SqlTokenIdentifier AsT => _content.V3;

    public SqlTokenOpenPar Opener => _content.V4;

    public ISqlNode SelectNode => _content.V5;

    public ISelectSpecification Select => (ISelectSpecification)_content.V5.UnPar;

    public SqlTokenClosePar Closer => _content.V6;

    [DebuggerStepThrough]
    internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

}
