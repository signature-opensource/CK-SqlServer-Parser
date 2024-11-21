using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CK.SqlServer.Parser;


public sealed class SqlOpenXml : SqlNonTokenAutoWidth
{
    readonly SNode<SqlTokenIdentifier, SqlEnclosedCommaList, SqlTokenIdentifier, SqlEnclosedCommaList, ISqlIdentifier> _content;

    public SqlOpenXml( SqlTokenIdentifier openXmlT, SqlEnclosedCommaList parameters, SqlTokenIdentifier withT, SqlEnclosedCommaList schemaDefinition, ISqlIdentifier schemaTable, ImmutableList<SqlTrivia> leading = null, ImmutableList<SqlTrivia> trailing = null )
        : base( leading, trailing )
    {
        _content = new SNode<SqlTokenIdentifier, SqlEnclosedCommaList, SqlTokenIdentifier, SqlEnclosedCommaList, ISqlIdentifier>( openXmlT, parameters, withT, schemaDefinition, schemaTable );
        CheckContent();
    }

    void CheckContent()
    {
        Helper.CheckToken( FunName, nameof( FunName ), SqlTokenType.OpenXml );
    }

    SqlOpenXml( SqlOpenXml o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
        : base( leading, trailing )
    {
        if( items == null ) _content = o._content;
        else
        {
            _content = new SNode<SqlTokenIdentifier, SqlEnclosedCommaList, SqlTokenIdentifier, SqlEnclosedCommaList, ISqlIdentifier>( items );
            CheckContent();
        }
    }

    protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
    {
        return new SqlOpenXml( this, leading, content, trailing );
    }

    public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

    public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

    public SqlTokenIdentifier FunName => _content.V1;

    public SqlEnclosedCommaList Parameters => _content.V2;

    public bool HasSchema => _content.V3 != null;

    public SqlTokenIdentifier WithT => _content.V3;

    public bool HasSchemaDefinition => _content.V4 != null;

    public SqlEnclosedCommaList SchemaDefinition => _content.V4;

    public bool HasSchemaTable => _content.V5 != null;

    public ISqlIdentifier SchemaTable => _content.V5;

    [DebuggerStepThrough]
    internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

}
