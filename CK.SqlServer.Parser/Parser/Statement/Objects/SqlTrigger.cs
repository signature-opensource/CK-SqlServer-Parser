using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CK.SqlServer.Parser;

using CNode = SNode<SqlCreateOrAlter,
                    SqlTokenIdentifier,
                    ISqlIdentifier,
                    SqlTokenIdentifier,
                    ISqlNode,
                    SqlWithOptions,
                    SqlNodeList,
                    SqlTokenIdentifier,
                    SqlStatementList,
                    SqlTokenTerminal>;

public sealed class SqlTrigger : SqlNonTokenAutoWidth, ISqlNamedStatement
{
    readonly CNode _content;

    public SqlTrigger( SqlCreateOrAlter createOrAlter,
                       SqlTokenIdentifier type,
                       ISqlIdentifier name,
                       SqlTokenIdentifier onT,
                       ISqlNode target,
                       SqlWithOptions options,
                       SqlNodeList configuration,
                       SqlTokenIdentifier asT,
                       SqlStatementList bodyStatements,
                       SqlTokenTerminal term )
        : base( null, null )
    {
        _content = new CNode( createOrAlter,
                              type,
                              name,
                              onT,
                              target,
                              options,
                              configuration,
                              asT,
                              bodyStatements,
                              term );
        CheckContent();
    }

    SqlTrigger( SqlTrigger o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
        : base( leading, trailing )
    {
        if( items == null ) _content = o._content;
        else
        {
            _content = new CNode( items );
            CheckContent();
        }
    }

    void CheckContent()
    {
        Helper.CheckNotNull( CreateOrAlter, nameof( CreateOrAlter ) );
        Helper.CheckToken( ObjectTypeT, nameof( ObjectTypeT ), SqlTokenType.Trigger );
        Helper.CheckNotNull( Name, nameof( Name ) );
        Helper.CheckNotNull( TargetName, nameof( TargetName ) );
        Helper.CheckNotNull( Configuration, nameof( Configuration ) );
        Helper.CheckToken( AsT, nameof( AsT ), SqlTokenType.As );
        Helper.CheckNotNull( Body, nameof( Body ) );
    }

    protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
    {
        return new SqlTrigger( this, leading, content, trailing );
    }

    public StatementKnownName StatementKnownName
                                    => CreateOrAlter.StatementPrefix == CreateOrAlterStatementPrefix.Alter
                                        ? StatementKnownName.AlterTrigger
                                        : (CreateOrAlter.StatementPrefix == CreateOrAlterStatementPrefix.Create
                                            ? StatementKnownName.CreateTrigger
                                            : StatementKnownName.CreateOrAlterTrigger);


    public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

    public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

    public SqlCreateOrAlter CreateOrAlter => _content.V1;

    public SqlTokenIdentifier ObjectTypeT => _content.V2;

    /// <summary>
    /// Gets the name of the procedure (may start with the Schema).
    /// </summary>
    public string ObjectName => Name.ToString();

    /// <summary>
    /// Gets the name of the trigger (may start with the Schema).
    /// </summary>
    public ISqlIdentifier Name => _content.V3;

    public SqlTokenIdentifier OnT => _content.V4;

    /// <summary>
    /// Can be ALL SERVER (a <see cref="SqlNodeList"/>) or a <see cref="ISqlIdentifier"/> (DATABASE as well 
    /// as a table or view name).
    /// </summary>
    public ISqlNode TargetName => _content.V5;

    public bool HasOptions => _content.V6 != null;

    public SqlWithOptions Options => _content.V6;

    /// <summary>
    /// Captures the whole trigger configuration upt to the required AS token.
    ///   { FOR | AFTER | INSTEAD OF } { [INSERT] [,] [UPDATE] [,] [DELETE] } [WITH APPEND] [NOT FOR REPLICATION]
    ///  or { FOR | AFTER } { event_type | event_group } [ ,...n ]
    /// </summary>
    public SqlNodeList Configuration => _content.V7;

    public SqlTokenIdentifier AsT => _content.V8;

    public SqlStatementList Body => _content.V9;

    public SqlTokenTerminal StatementTerminator => _content.V10;

    [DebuggerStepThrough]
    internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

}
