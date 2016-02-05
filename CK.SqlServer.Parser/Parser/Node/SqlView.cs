using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using CK.Core;
using System.Collections.Immutable;

namespace CK.SqlServer.Parser
{
    public sealed class SqlView : SqlNode, ISqlNamedStatement
    {
        readonly SNode<SqlTokenIdentifier, SqlTokenIdentifier, ISqlIdentifier, SqlEnclosedIdentifierCommaList, SqlNodeList, SqlTokenIdentifier, ISqlNode, SqlTokenTerminal> _content;

        public SqlView( SqlTokenIdentifier alterOrCreate, SqlTokenIdentifier type, ISqlIdentifier name, SqlEnclosedIdentifierCommaList columns, SqlNodeList options, SqlTokenIdentifier asToken, ISqlNode select, SqlTokenTerminal term )
            : base( null, null )
        {
            _content = new SNode<SqlTokenIdentifier, SqlTokenIdentifier, ISqlIdentifier, SqlEnclosedIdentifierCommaList, SqlNodeList, SqlTokenIdentifier, ISqlNode, SqlTokenTerminal>(
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
            Helper.CheckToken( AlterOrCreateT, nameof( AlterOrCreateT ), SqlTokenType.Alter, SqlTokenType.Create );
            Helper.CheckToken( ObjectTypeT, nameof( ObjectTypeT ), SqlTokenType.View );
            Helper.CheckNotNull( Name, nameof( Name ) );
            Helper.CheckNullableToken( AsT, nameof( AsT ), SqlTokenType.As );
            Helper.CheckNotNull( Select, nameof( Select ) );
        }

        SqlView( ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            _content = new SNode<SqlTokenIdentifier, SqlTokenIdentifier, ISqlIdentifier, SqlEnclosedIdentifierCommaList, SqlNodeList, SqlTokenIdentifier, ISqlNode, SqlTokenTerminal>( items );
            CheckContent();
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlView( leading, content, trailing );
        }

        public StatementKnownName StatementKnownName => AlterOrCreateT.TokenType == SqlTokenType.Alter
                                            ? StatementKnownName.AlterView
                                            : StatementKnownName.CreateView;

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

        public SqlTokenIdentifier AlterOrCreateT => _content.V1;

        public SqlTokenIdentifier ObjectTypeT => _content.V2;

        public ISqlIdentifier Name => _content.V3;

        public bool HasColumnNames => _content.V4 != null;

        public bool HasOptions => _content.V5 != null;

        public SqlEnclosedIdentifierCommaList ColumnNames => _content.V4;

        public SqlNodeList Options => _content.V5;

        public SqlTokenIdentifier AsT => _content.V6;

        public ISqlNode Select => _content.V7;

        public SqlTokenTerminal StatementTerminator => _content.V8;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

    }
}
