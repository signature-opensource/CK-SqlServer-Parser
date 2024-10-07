using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CK.SqlServer.Parser;

using CNode = SNode<
        SqlTokenIdentifier,
        ISqlHasStringValue,
        SqlTokenIdentifier,
        ISqlHasStringValue,
        SqlTokenTerminal>;

/// <summary>
/// Injects unparsed text into a named fragment.
/// </summary>
public sealed class SqlTInjectInto : SqlNonTokenAutoWidth, ISqlTStatement
{
    readonly CNode _content;

    public SqlTInjectInto( SqlTokenIdentifier injectT,
                           ISqlHasStringValue content,
                           SqlTokenIdentifier intoT,
                           ISqlHasStringValue target,
                           SqlTokenTerminal terminator )
        : base( null, null )
    {
        _content = new CNode( injectT, content, intoT, target, terminator );
        CheckContent();
    }

    void CheckContent()
    {
        Helper.CheckToken( InjectT, nameof( InjectT ), SqlTokenType.Inject );
        Helper.CheckNotNull( Content, nameof( Content ) );
        Helper.CheckToken( IntoT, nameof( IntoT ), SqlTokenType.Into );
        Helper.CheckNotNull( Target, nameof( Target ) );
    }

    SqlTInjectInto( SqlTInjectInto o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
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
        return new SqlTInjectInto( this, leading, content, trailing );
    }

    public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

    public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

    public SqlTokenIdentifier InjectT => _content.V1;

    public ISqlHasStringValue Content => _content.V2;

    public SqlTokenIdentifier IntoT => _content.V3;

    public ISqlHasStringValue Target => _content.V4;

    public SqlTokenTerminal StatementTerminator => _content.V5;

    [DebuggerStepThrough]
    internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

}
