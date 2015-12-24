using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using CK.Core;
using System.Collections.Immutable;

namespace CK.SqlServer.Parser
{
    public sealed class SqlView : SqlNode, ISqlStatement
    {
        readonly SNode<SqlTokenIdentifier, SqlTokenIdentifier, ISqlIdentifier, SqlColumnNameList, SqlNodeList, SqlTokenIdentifier, SqlNodeList, SqlTokenTerminal> _content;

        public SqlView( SqlTokenIdentifier alterOrCreate, SqlTokenIdentifier type, ISqlIdentifier name, SqlColumnNameList columns, SqlNodeList options, SqlTokenIdentifier asToken, SqlNodeList select, SqlTokenTerminal term )
            : base( null, null )
        {
            _content = new SNode<SqlTokenIdentifier, SqlTokenIdentifier, ISqlIdentifier, SqlColumnNameList, SqlNodeList, SqlTokenIdentifier, SqlNodeList, SqlTokenTerminal>(
                alterOrCreate,
                type,
                name,
                columns,
                options,
                asToken,
                select,
                term );
            CheckContent();
        }

        void CheckContent()
        {
            SNode.CheckToken( AlterOrCreateT, nameof( AlterOrCreateT ), SqlTokenType.Alter, SqlTokenType.Create );
            SNode.CheckToken( ObjectTypeT, nameof( ObjectTypeT ), SqlTokenType.View );
            SNode.CheckNotNull( Name, nameof( Name ) );
            SNode.CheckNullableToken( AsT, nameof( AsT ), SqlTokenType.As );
            SNode.CheckNotNull( Select, nameof( Select ) );
        }

        SqlView( ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            _content = new SNode<SqlTokenIdentifier, SqlTokenIdentifier, ISqlIdentifier, SqlColumnNameList, SqlNodeList, SqlTokenIdentifier, SqlNodeList, SqlTokenTerminal>( items );
            CheckContent();
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IReadOnlyList<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlView( leading, children, trailing );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public SqlTokenIdentifier AlterOrCreateT => _content.V1;

        public SqlTokenIdentifier ObjectTypeT => _content.V2;

        public ISqlIdentifier Name => _content.V3;

        public bool HasColumns => _content.V4 != null;

        public bool HasOptions => _content.V5 != null;

        public SqlColumnNameList Columns => _content.V4;

        public SqlNodeList Options => _content.V5;

        public SqlTokenIdentifier AsT => _content.V6;

        public SqlNodeList Select => _content.V7;

        public SqlTokenTerminal StatementTerminator => _content.V8;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor ) => visitor.Visit( this );

    }
}
