using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using CK.Core;
using System.Collections.Immutable;

namespace CK.SqlServer.Parser
{
    public sealed class SqlStoredProcedure : SqlNode, ISqlStatement, ISqlServerStoredProcedure
    {
        readonly SNode<
            SqlTokenIdentifier,
            SqlTokenIdentifier,
            ISqlIdentifier,
            SqlParameterList,
            SqlNodeList,
            SqlTokenIdentifier,
            SqlTokenIdentifier,
            SqlStatementList,
            SqlTokenIdentifier,
            SqlTokenTerminal> _content;

        public SqlStoredProcedure( 
            SqlTokenIdentifier alterOrCreate, 
            SqlTokenIdentifier type,
            ISqlIdentifier name, 
            SqlParameterList parameters, 
            SqlNodeList options, 
            SqlTokenIdentifier asToken, 
            SqlTokenIdentifier begin, 
            SqlStatementList bodyStatements, 
            SqlTokenIdentifier end, 
            SqlTokenTerminal term )
            : base( null, null )
        {
            _content = new SNode<SqlTokenIdentifier, SqlTokenIdentifier, ISqlIdentifier, SqlParameterList, SqlNodeList, SqlTokenIdentifier, SqlTokenIdentifier, SqlStatementList, SqlTokenIdentifier, SqlTokenTerminal>(
                alterOrCreate,
                type,
                name,
                parameters,
                options,
                asToken,
                begin,
                bodyStatements,
                end,
                term );
            CheckContent();
        }

        SqlStoredProcedure( SqlStoredProcedure o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _content = o._content;
            else
            {
                _content = new SNode<SqlTokenIdentifier, SqlTokenIdentifier, ISqlIdentifier, SqlParameterList, SqlNodeList, SqlTokenIdentifier, SqlTokenIdentifier, SqlStatementList, SqlTokenIdentifier, SqlTokenTerminal>( items );
                CheckContent();
            }
        }

        void CheckContent()
        {
            SNode.CheckToken( AlterOrCreateT, nameof( AlterOrCreateT ), SqlTokenType.Alter, SqlTokenType.Create );
            SNode.CheckToken( ObjectTypeT, nameof( ObjectTypeT ), SqlTokenType.Procedure );
            SNode.CheckNotNull( Name, nameof( Name ) );
            SNode.CheckNotNull( Parameters, nameof( Parameters ) );
            SNode.CheckToken( AsT, nameof( AsT ), SqlTokenType.As );
            SNode.CheckNullableToken( BeginT, nameof( BeginT ), SqlTokenType.Begin );
            SNode.CheckNotNull( BodyStatements, nameof( BodyStatements ) );
            SNode.CheckNullableToken( EndT, nameof( EndT ), SqlTokenType.End );
            SNode.CheckBothNullOrNot( BeginT, nameof( BeginT ), EndT, nameof( EndT ) );
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IReadOnlyList<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlStoredProcedure( this, leading, children, trailing );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public SqlTokenIdentifier AlterOrCreateT => _content.V1;

        public SqlTokenIdentifier ObjectTypeT => _content.V2;

        /// <summary>
        /// Gets the name of the procedure (may start with the Schema).
        /// </summary>
        public string ObjectName => Name.ToString();

        /// <summary>
        /// Gets the name of the procedure (may start with the Schema).
        /// </summary>
        public ISqlIdentifier Name => _content.V3;

        public SqlParameterList Parameters => _content.V4;

        ISqlServerParameterList ISqlServerCallableObject.Parameters => _content.V4;

        SqlServerObjectType ISqlServerObject.ObjectType => SqlServerObjectType.Procedure;

        string ISqlServerObject.ToStringSignature( bool withOptions )
        {
            return withOptions ? Header.ToStringCompact() : _content.Skip( 1 ).Take( 3 ).ToStringCompact();
        }

        public bool HasOptions => _content.V5 != null;

        public SqlNodeList Options => _content.V5;

        public IEnumerable<ISqlNode> Header => _content.Skip( 1 ).Take( HasOptions ? 4 : 3 );

        public SqlTokenIdentifier AsT => _content.V6;

        public bool HasBeginEnd => _content.V7 != null;

        public SqlTokenIdentifier BeginT => _content.V7;

        public SqlStatementList BodyStatements => _content.V8;

        public SqlTokenIdentifier EndT => _content.V9;

        public SqlTokenTerminal StatementTerminator => _content.V10;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor ) => visitor.Visit( this );

    }
}
