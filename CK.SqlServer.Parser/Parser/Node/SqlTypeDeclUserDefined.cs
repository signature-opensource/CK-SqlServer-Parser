using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Diagnostics;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CK.SqlServer.Parser;


/// <summary>
/// A user defined type is denoted by a dotted identifier [dbo].DefinedType or single identifier like geometry.
/// </summary>
public sealed class SqlTypeDeclUserDefined : SqlNonTokenAutoWidth, ISqlUnifiedTypeDecl
{
    readonly SNode<ISqlIdentifier> _content;

    public SqlTypeDeclUserDefined( ISqlIdentifier identifier )
        : base( null, null )
    {
        _content = new SNode<ISqlIdentifier>( identifier );
        CheckContent();
    }

    void CheckContent()
    {
        Helper.CheckNotNull( Identifier, nameof( Identifier ) );
    }

    SqlTypeDeclUserDefined( SqlTypeDeclUserDefined o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
        : base( leading, trailing )
    {
        if( items == null ) _content = o._content;
        else
        {
            _content = new SNode<ISqlIdentifier>( items );
            CheckContent();
        }
    }

    protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
    {
        return new SqlTypeDeclUserDefined( this, leading, content, trailing );
    }

    public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

    public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

    public ISqlIdentifier Identifier => _content.V;

    public SqlDbType DbType => SqlDbType.Udt;

    string ISqlServerUnifiedTypeDecl.ToStringClean() => ChildrenNodes.ToStringCompact();

    [DebuggerStepThrough]
    internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

    int ISqlServerUnifiedTypeDecl.SyntaxSize => -2;

    byte ISqlServerUnifiedTypeDecl.SyntaxPrecision => 0;

    byte ISqlServerUnifiedTypeDecl.SyntaxScale => 0;

    int ISqlServerUnifiedTypeDecl.SyntaxSecondScale => -1;
}
