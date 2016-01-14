using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using CK.Core;
using System.Collections.Immutable;

namespace CK.SqlServer.Parser
{
    public sealed class SqlTrigger : SqlNode, ISqlNamedStatement
    {
        readonly SNode<
            SqlTokenIdentifier,
            SqlTokenIdentifier,
            ISqlIdentifier,
            SqlTokenIdentifier,
            ISqlNode,
            SqlWithOptions,
            SqlNodeList,
            SqlTokenIdentifier,
            SqlStatementList,
            SqlTokenTerminal> _content;

        public SqlTrigger( 
            SqlTokenIdentifier alterOrCreate, 
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
            _content = new SNode<SqlTokenIdentifier, SqlTokenIdentifier, ISqlIdentifier, SqlTokenIdentifier, ISqlNode, SqlWithOptions, SqlNodeList, SqlTokenIdentifier, SqlStatementList, SqlTokenTerminal>(
                alterOrCreate,
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
                _content = new SNode<SqlTokenIdentifier, SqlTokenIdentifier, ISqlIdentifier, SqlTokenIdentifier, ISqlNode, SqlWithOptions, SqlNodeList, SqlTokenIdentifier, SqlStatementList, SqlTokenTerminal>( items );
                CheckContent();
            }
        }

        void CheckContent()
        {
            SNode.CheckToken( AlterOrCreateT, nameof( AlterOrCreateT ), SqlTokenType.Alter, SqlTokenType.Create );
            SNode.CheckToken( ObjectTypeT, nameof( ObjectTypeT ), SqlTokenType.Trigger );
            SNode.CheckNotNull( Name, nameof( Name ) );
            SNode.CheckNotNull( TargetName, nameof( TargetName ) );
            SNode.CheckNotNull( Configuration, nameof( Configuration ) );
            SNode.CheckToken( AsT, nameof( AsT ), SqlTokenType.As );
            SNode.CheckNotNull( Body, nameof( Body ) );
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlTrigger( this, leading, children, trailing );
        }

        public StatementKnownName StatementKnownName => AlterOrCreateT.TokenType == SqlTokenType.Alter
                                    ? StatementKnownName.AlterTrigger
                                    : StatementKnownName.CreateTrigger;


        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public SqlTokenIdentifier AlterOrCreateT => _content.V1;

        public bool IsAlter => AlterOrCreateT.TokenType == SqlTokenType.Alter;

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
        internal protected override ISqlNode Accept( SqlItemVisitor visitor ) => visitor.Visit( this );

    }
}
