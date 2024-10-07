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
        SqlTokenIdentifier,
        ISqlTLocationFinder,
        SqlTokenTerminal>;

/// <summary>
/// Injects unparsed text around, before or after a <see cref="ISqlTLocationFinder"/>.
/// </summary>
public sealed class SqlTInject : SqlNonTokenAutoWidth, ISqlTStatement
{
    readonly CNode _content;

    public SqlTInject( SqlTokenIdentifier injecT,
                       ISqlHasStringValue content,
                       SqlTokenIdentifier andT,
                       ISqlHasStringValue content2,
                       SqlTokenIdentifier afterBeforeOrAroundT,
                       ISqlTLocationFinder location,
                       SqlTokenTerminal terminator )
        : base( null, null )
    {
        _content = new CNode( injecT, content, andT, content2, afterBeforeOrAroundT, location, terminator );
        CheckContent();
    }

    void CheckContent()
    {
        Helper.CheckToken( InjectT, nameof( InjectT ), SqlTokenType.Inject );
        Helper.CheckNotNull( Content, nameof( Content ) );
        Helper.CheckNullableToken( AndT, nameof( AndT ), SqlTokenType.And );
        Helper.CheckBothNullOrNot( AndT, nameof( AndT ), Content2, nameof( Content2 ) );
        Helper.CheckToken( AfterBeforeOrAroundT, nameof( AfterBeforeOrAroundT ), SqlTokenType.After, SqlTokenType.Before, SqlTokenType.Around );
        Helper.CheckNotNull( Location, nameof( Location ) );
    }

    SqlTInject( SqlTInject o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
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
        return new SqlTInject( this, leading, content, trailing );
    }

    public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

    public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

    public SqlTokenIdentifier InjectT => _content.V1;

    public ISqlHasStringValue Content => _content.V2;

    public SqlTokenIdentifier AndT => _content.V3;

    public ISqlHasStringValue Content2 => _content.V4;

    public string TextBefore => _content.V5.TokenType != SqlTokenType.After
                                    ? Content.Value
                                    : null;

    public string TextAfter => _content.V5.TokenType == SqlTokenType.After
                                    ? Content.Value
                                    : Content2?.Value;

    public SqlTokenIdentifier AfterBeforeOrAroundT => _content.V5;

    public ISqlTLocationFinder Location => _content.V6;

    public SqlTokenTerminal StatementTerminator => _content.V7;

    [DebuggerStepThrough]
    internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

}
