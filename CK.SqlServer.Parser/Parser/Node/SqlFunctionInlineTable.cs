using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using CK.Core;
using System.Collections.Immutable;

namespace CK.SqlServer.Parser
{
    public sealed class SqlFunctionInlineTable : SqlNode, ISqlNamedStatement
    {
        readonly SNode<
            SqlTokenIdentifier,
            SqlTokenIdentifier,
            ISqlIdentifier,
            SqlParameterList,
            SqlTokenIdentifier,
            SqlTokenIdentifier,
            SqlNodeList,
            SqlTokenIdentifier,
            SqlTokenIdentifier,
            ISqlNode,
            SqlTokenTerminal> _content;

        public SqlFunctionInlineTable( 
            SqlTokenIdentifier alterOrCreate, 
            SqlTokenIdentifier type,
            ISqlIdentifier name, 
            SqlParameterList parameters,
            SqlTokenIdentifier returns,
            SqlTokenIdentifier table,
            SqlNodeList options,
            SqlTokenIdentifier asToken,
            SqlTokenIdentifier returnToken,
            ISqlNode select, 
            SqlTokenTerminal term )
            : base( null, null ) 
        {
            _content = new SNode<SqlTokenIdentifier, SqlTokenIdentifier, ISqlIdentifier, SqlParameterList, SqlTokenIdentifier, SqlTokenIdentifier, SqlNodeList, SqlTokenIdentifier, SqlTokenIdentifier, ISqlNode, SqlTokenTerminal>(
                alterOrCreate, 
                type, 
                name, 
                parameters, 
                returns, 
                table, 
                options, 
                asToken, 
                returnToken, 
                select, 
                term );
            CheckContent();
        }

        void CheckContent()
        {
            SNode.CheckToken( AlterOrCreateT, nameof( AlterOrCreateT ), SqlTokenType.Alter, SqlTokenType.Create );
            SNode.CheckToken( ObjectTypeT, nameof( ObjectTypeT ), SqlTokenType.Function );
            SNode.CheckNotNull( Parameters, nameof( Parameters ) );
            SNode.CheckToken( ReturnsT, nameof( ReturnsT ), SqlTokenType.Returns );
            SNode.CheckNotNull( TableT, nameof( TableT ) );
            SNode.CheckNullableToken( AsT, nameof( AsT ), SqlTokenType.As );
            SNode.CheckNotNull( Select, nameof( Select ) );
        }

        SqlFunctionInlineTable( SqlFunctionInlineTable o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _content = o._content;
            else
            {
                _content = new SNode<SqlTokenIdentifier, SqlTokenIdentifier, ISqlIdentifier, SqlParameterList, SqlTokenIdentifier, SqlTokenIdentifier, SqlNodeList, SqlTokenIdentifier, SqlTokenIdentifier, ISqlNode, SqlTokenTerminal>( items );
                CheckContent();
            }
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlFunctionInlineTable( this, leading, children, trailing );
        }

        public StatementKnownName StatementKnownName => AlterOrCreateT.TokenType == SqlTokenType.Alter 
                                                    ? StatementKnownName.AlterFunction 
                                                    : StatementKnownName.CreateFunction;

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public SqlTokenIdentifier AlterOrCreateT => _content.V1;

        public SqlTokenIdentifier ObjectTypeT => _content.V2;

        /// <summary>
        /// Gets the name of the procedure (may start with the Schema).
        /// </summary>
        public ISqlIdentifier Name => _content.V3;

        public SqlParameterList Parameters => _content.V4;

        public SqlTokenIdentifier ReturnsT => _content.V5;

        public SqlTokenIdentifier TableT => _content.V6;

        public SqlNodeList Options => _content.V7;

        public SqlTokenIdentifier AsT => _content.V8;

        public SqlTokenIdentifier ReturnT => _content.V9;

        public ISqlNode Select => _content.V10;

        public SqlTokenTerminal StatementTerminator => _content.V11;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor ) => visitor.Visit( this );

    }
}
