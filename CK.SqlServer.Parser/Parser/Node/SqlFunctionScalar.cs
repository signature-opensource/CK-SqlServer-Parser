using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using CK.Core;
using System.Collections.Immutable;

namespace CK.SqlServer.Parser
{
    public sealed class SqlFunctionScalar : SqlNode, ISqlNamedStatement
    {
        readonly SNode<SqlTokenIdentifier,
            SqlTokenIdentifier,
            ISqlIdentifier,
            SqlParameterList,
            SqlTokenIdentifier,
            ISqlUnifiedTypeDecl,
            SqlNodeList,
            SqlTokenIdentifier,
            SqlTokenIdentifier,
            SqlStatementList,
            SqlTokenIdentifier,
            SqlTokenTerminal> _content;

        public SqlFunctionScalar( 
            SqlTokenIdentifier alterOrCreate, 
            SqlTokenIdentifier type,
            ISqlIdentifier name, 
            SqlParameterList parameters,
            SqlTokenIdentifier returns,
            ISqlUnifiedTypeDecl returnScalarType,
            SqlNodeList options,
            SqlTokenIdentifier asToken,
            SqlTokenIdentifier begin,
            SqlStatementList bodyStatements, 
            SqlTokenIdentifier end, 
            SqlTokenTerminal term )
            : base( null, null )
        {
            _content = new SNode<SqlTokenIdentifier, SqlTokenIdentifier, ISqlIdentifier, SqlParameterList, SqlTokenIdentifier, ISqlUnifiedTypeDecl, SqlNodeList, SqlTokenIdentifier, SqlTokenIdentifier, SqlStatementList, SqlTokenIdentifier, SqlTokenTerminal>(
                 alterOrCreate,
                 type,
                 name,
                 parameters,
                 returns,
                 returnScalarType,
                 options,
                 asToken,
                 begin,
                 bodyStatements,
                 end,
                 term );
            CheckContent();
        }

        void CheckContent()
        {
            SNode.CheckToken( AlterOrCreateT, nameof( AlterOrCreateT ), SqlTokenType.Alter, SqlTokenType.Create );
            SNode.CheckToken( ObjectTypeT, nameof( ObjectTypeT ), SqlTokenType.Function );
            SNode.CheckNotNull( Parameters, nameof( Parameters ) );
            SNode.CheckToken( ReturnsT, nameof( ReturnsT ), SqlTokenType.Returns );
            SNode.CheckNotNull( ReturnedType, nameof( ReturnedType ) );
            SNode.CheckToken( AsT, nameof( AsT ), SqlTokenType.As );
            SNode.CheckNullableToken( BeginT, nameof( BeginT ), SqlTokenType.Begin );
            SNode.CheckNotNull( BodyStatements, nameof( BodyStatements ) );
            SNode.CheckNullableToken( EndT, nameof( EndT ), SqlTokenType.End );
        }

        SqlFunctionScalar( SqlFunctionScalar o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _content = o._content;
            else
            {
                _content = new SNode<SqlTokenIdentifier, SqlTokenIdentifier, ISqlIdentifier, SqlParameterList, SqlTokenIdentifier, ISqlUnifiedTypeDecl, SqlNodeList, SqlTokenIdentifier, SqlTokenIdentifier, SqlStatementList, SqlTokenIdentifier, SqlTokenTerminal>( items );
                CheckContent();
            }
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IReadOnlyList<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlFunctionScalar( this, leading, children, trailing );
        }

        public StatementName StatementName => AlterOrCreateT.TokenType == SqlTokenType.Alter
                                            ? StatementName.AlterFunction
                                            : StatementName.CreateFunction;

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public SqlTokenIdentifier AlterOrCreateT => _content.V1;

        public SqlTokenIdentifier ObjectTypeT => _content.V2;

        /// <summary>
        /// Gets the name of the procedure (may start with the Schema).
        /// </summary>
        public ISqlIdentifier Name => _content.V3;

        public SqlParameterList Parameters => _content.V4;

        public SqlTokenIdentifier ReturnsT => _content.V5;

        public ISqlUnifiedTypeDecl ReturnedType => _content.V6;

        public SqlNodeList Options => _content.V7;

        public SqlTokenIdentifier AsT => _content.V8;

        public SqlTokenIdentifier BeginT => _content.V9;

        public SqlStatementList BodyStatements => _content.V10;

        public SqlTokenIdentifier EndT => _content.V11;

        public SqlTokenTerminal StatementTerminator => _content.V12;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor ) => visitor.Visit( this );

    }
}


