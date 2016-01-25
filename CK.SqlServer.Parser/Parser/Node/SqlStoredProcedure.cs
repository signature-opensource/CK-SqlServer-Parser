using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using CK.Core;
using System.Collections.Immutable;

namespace CK.SqlServer.Parser
{
    public sealed class SqlStoredProcedure : SqlNode, ISqlNamedStatement, ISqlServerStoredProcedure
    {
        readonly SNode<
            SqlTokenIdentifier,
            SqlTokenIdentifier,
            ISqlIdentifier,
            SqlParameterList,
            SqlWithOptions,
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
            SqlWithOptions options, 
            SqlTokenIdentifier asToken, 
            SqlTokenIdentifier begin, 
            SqlStatementList bodyStatements, 
            SqlTokenIdentifier end, 
            SqlTokenTerminal term )
            : base( null, null )
        {
            _content = new SNode<SqlTokenIdentifier, SqlTokenIdentifier, ISqlIdentifier, SqlParameterList, SqlWithOptions, SqlTokenIdentifier, SqlTokenIdentifier, SqlStatementList, SqlTokenIdentifier, SqlTokenTerminal>(
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
                _content = new SNode<SqlTokenIdentifier, SqlTokenIdentifier, ISqlIdentifier, SqlParameterList, SqlWithOptions, SqlTokenIdentifier, SqlTokenIdentifier, SqlStatementList, SqlTokenIdentifier, SqlTokenTerminal>( items );
                CheckContent();
            }
        }

        void CheckContent()
        {
            Helper.CheckToken( AlterOrCreateT, nameof( AlterOrCreateT ), SqlTokenType.Alter, SqlTokenType.Create );
            Helper.CheckToken( ObjectTypeT, nameof( ObjectTypeT ), SqlTokenType.Procedure );
            Helper.CheckNotNull( Name, nameof( Name ) );
            Helper.CheckNotNull( Parameters, nameof( Parameters ) );
            Helper.CheckToken( AsT, nameof( AsT ), SqlTokenType.As );
            Helper.CheckNullableToken( BeginT, nameof( BeginT ), SqlTokenType.Begin );
            Helper.CheckNotNull( Body, nameof( Body ) );
            Helper.CheckNullableToken( EndT, nameof( EndT ), SqlTokenType.End );
            Helper.CheckBothNullOrNot( BeginT, nameof( BeginT ), EndT, nameof( EndT ) );
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlStoredProcedure( this, leading, children, trailing );
        }

        public StatementKnownName StatementKnownName => AlterOrCreateT.TokenType == SqlTokenType.Alter
                                    ? StatementKnownName.AlterProcedure
                                    : StatementKnownName.CreateProcedure;


        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

        public SqlTokenIdentifier AlterOrCreateT => _content.V1;

        public bool IsAlterKeyword => AlterOrCreateT.TokenType == SqlTokenType.Alter;

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

        public bool HasOptions => _content.V5 != null;

        public SqlWithOptions Options => _content.V5;

        public IEnumerable<ISqlNode> Header => _content.Skip( 1 ).Take( HasOptions ? 4 : 3 );

        public SqlTokenIdentifier AsT => _content.V6;

        public bool HasBeginEnd => _content.V7 != null;

        public SqlTokenIdentifier BeginT => _content.V7;

        public SqlStatementList Body => _content.V8;

        public SqlTokenIdentifier EndT => _content.V9;

        public SqlTokenTerminal StatementTerminator => _content.V10;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor ) => visitor.Visit( this );

        string ISqlServerObject.ToStringSignature( bool withOptions )
        {
            return withOptions ? Header.ToStringCompact() : _content.Skip( 1 ).Take( 3 ).ToStringCompact();
        }

        void ISqlServerObject.Write( StringBuilder b )
        {
            Write( SqlTextWriter.CreateDefault( b ) );
        }

        ISqlServerAlterOrCreateStatement ISqlServerAlterOrCreateStatement.ToggleAlterKeyword()
        {
            return (ISqlServerAlterOrCreateStatement)ReplaceContentNode( 0,
                    IsAlterKeyword
                        ? new SqlTokenIdentifier( SqlTokenType.Create, "create", AlterOrCreateT.LeadingTrivias, AlterOrCreateT.TrailingTrivias )
                        : new SqlTokenIdentifier( SqlTokenType.Alter, "alter", AlterOrCreateT.LeadingTrivias, AlterOrCreateT.TrailingTrivias ) );
        }
    }
}
