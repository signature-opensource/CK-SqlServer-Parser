using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using CK.Core;
using System.Collections.Immutable;

namespace CK.SqlServer.Parser
{
    public sealed class SqlStoredProcedure : SqlNonToken, ISqlNamedStatement, ISqlServerStoredProcedure, ISqlParameterListHolder
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
            Helper.CheckNotNull( FullName, nameof( FullName ) );
            Helper.CheckNotNull( Parameters, nameof( Parameters ) );
            Helper.CheckToken( AsT, nameof( AsT ), SqlTokenType.As );
            Helper.CheckNullableToken( BeginT, nameof( BeginT ), SqlTokenType.Begin );
            Helper.CheckNotNull( Body, nameof( Body ) );
            Helper.CheckNullableToken( EndT, nameof( EndT ), SqlTokenType.End );
            Helper.CheckBothNullOrNot( BeginT, nameof( BeginT ), EndT, nameof( EndT ) );
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlStoredProcedure( this, leading, content, trailing );
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
        /// Gets the name of the procedure without the schema.
        /// </summary>
        public string Name => FullName.GetPartName( 1 );

        /// <summary>
        /// Gets the schema name or null if there is no schema.
        /// </summary>
        public string Schema => FullName.GetPartName( 2 );

        /// <summary>
        /// Gets the full name of the procedure (may start with the Schema).
        /// </summary>
        public string SchemaName => FullName.ToString();

        /// <summary>
        /// Gets the name of the procedure (may start with the Schema).
        /// </summary>
        public ISqlIdentifier FullName => _content.V3;

        /// <summary>
        /// Gets the parameters.
        /// </summary>
        public SqlParameterList Parameters => _content.V4;

        public SqlStoredProcedure SetParameters( SqlParameterList parameters ) => this.ReplaceContentNode( 3, parameters );

        ISqlServerParameterList ISqlServerCallableObject.Parameters => _content.V4.ModelParameters;

        ISqlServerObject ISqlServerObject.SetSchema(string name)
        {
            return this.ReplaceContentNode( 2, FullName.SetPartName( 2, name ) );
        }

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
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

        IEnumerable<ISqlServerComment> ISqlServerParsedText.HeaderComments => FullLeadingTrivias.Cast<ISqlServerComment>();

        string ISqlServerObject.ToStringSignature( bool withOptions )
        {
            return withOptions ? Header.ToStringCompact() : _content.Skip( 1 ).Take( 3 ).ToStringCompact();
        }

        void ISqlServerParsedText.Write( StringBuilder b ) => Write( SqlTextWriter.CreateDefault( b ) );

        ISqlServerAlterOrCreateStatement ISqlServerAlterOrCreateStatement.ToggleAlterKeyword()
        {
            return this.ReplaceContentNode( 0,
                            IsAlterKeyword
                                ? new SqlTokenIdentifier( SqlTokenType.Create, "create", AlterOrCreateT.LeadingTrivias, AlterOrCreateT.TrailingTrivias )
                                : new SqlTokenIdentifier( SqlTokenType.Alter, "alter", AlterOrCreateT.LeadingTrivias, AlterOrCreateT.TrailingTrivias ) );
        }

        ISqlParameterListHolder ISqlParameterListHolder.SetParameters( SqlParameterList parameters ) => SetParameters( parameters );

    }
}
