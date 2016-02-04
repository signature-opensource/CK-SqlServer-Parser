using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using CK.Core;
using System.Collections.Immutable;

namespace CK.SqlServer.Parser
{
    public sealed class SqlFunctionTable : SqlNode, ISqlNamedStatement, ISqlServerFunctionTable, ISqlParameterListHolder
    {
        readonly SNode<SqlTokenIdentifier,
            SqlTokenIdentifier,
            ISqlIdentifier,
            SqlParameterList,
            SqlTokenIdentifier,
            SqlTokenIdentifier,
            SqlTypeDeclTable,
            SqlWithOptions,
            SqlTokenIdentifier,
            SqlTokenIdentifier,
            SqlStatementList,
            SqlTokenIdentifier,
            SqlTokenTerminal> _content;

        public SqlFunctionTable( 
            SqlTokenIdentifier alterOrCreate, 
            SqlTokenIdentifier type,
            ISqlIdentifier name, 
            SqlParameterList parameters,
            SqlTokenIdentifier returnsT,
            SqlTokenIdentifier tableVariableNameT,
            SqlTypeDeclTable returnedTableType,
            SqlWithOptions options,
            SqlTokenIdentifier asToken,
            SqlTokenIdentifier begin,
            SqlStatementList bodyStatements, 
            SqlTokenIdentifier end, 
            SqlTokenTerminal term )
            : base( null, null )
        {
            _content = new SNode<SqlTokenIdentifier, SqlTokenIdentifier, ISqlIdentifier, SqlParameterList, SqlTokenIdentifier, SqlTokenIdentifier, SqlTypeDeclTable, SqlWithOptions, SqlTokenIdentifier, SqlTokenIdentifier, SqlStatementList, SqlTokenIdentifier, SqlTokenTerminal>(
                 alterOrCreate,
                 type,
                 name,
                 parameters,
                 returnsT,
                 tableVariableNameT,
                 returnedTableType,
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
            Helper.CheckToken( AlterOrCreateT, nameof( AlterOrCreateT ), SqlTokenType.Alter, SqlTokenType.Create );
            Helper.CheckToken( ObjectTypeT, nameof( ObjectTypeT ), SqlTokenType.Function );
            Helper.CheckNotNull( Parameters, nameof( Parameters ) );
            Helper.CheckToken( ReturnsT, nameof( ReturnsT ), SqlTokenType.Returns );
            Helper.CheckIsVariable( TableVariableName, nameof( TableVariableName ) );
            Helper.CheckNotNull( ReturnedTableType, nameof( ReturnedTableType ) );
            Helper.CheckNullableToken( AsT, nameof( AsT ), SqlTokenType.As );
            Helper.CheckToken( BeginT, nameof( BeginT ), SqlTokenType.Begin );
            Helper.CheckNotNull( BodyStatements, nameof( BodyStatements ) );
            Helper.CheckToken( EndT, nameof( EndT ), SqlTokenType.End );
        }

        SqlFunctionTable( SqlFunctionTable o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _content = o._content;
            else
            {
                _content = new SNode<SqlTokenIdentifier, SqlTokenIdentifier, ISqlIdentifier, SqlParameterList, SqlTokenIdentifier, SqlTokenIdentifier, SqlTypeDeclTable, SqlWithOptions, SqlTokenIdentifier, SqlTokenIdentifier, SqlStatementList, SqlTokenIdentifier, SqlTokenTerminal>( items );
                CheckContent();
            }
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlFunctionTable( this, leading, content, trailing );
        }

        public StatementKnownName StatementKnownName => AlterOrCreateT.TokenType == SqlTokenType.Alter
                                            ? StatementKnownName.AlterFunction
                                            : StatementKnownName.CreateFunction;

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

        public SqlFunctionTable SetParameters( SqlParameterList parameters ) => this.ReplaceContentNode( 3, parameters );

        public SqlTokenIdentifier ReturnsT => _content.V5;

        public SqlTokenIdentifier TableVariableName => _content.V6;

        public ISqlUnifiedTypeDecl ReturnedTableType => _content.V7;

        public bool HasOptions => _content.V8 != null;

        public SqlWithOptions Options => _content.V8;

        public IEnumerable<ISqlNode> Header => _content.Skip( 1 ).Take( HasOptions ? 7 : 6 );

        public SqlTokenIdentifier AsT => _content.V9;

        public SqlTokenIdentifier BeginT => _content.V10;

        public SqlStatementList BodyStatements => _content.V11;

        public SqlTokenIdentifier EndT => _content.V12;

        public SqlTokenTerminal StatementTerminator => _content.V13;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

        ISqlServerParameterList ISqlServerCallableObject.Parameters => _content.V4.ModelParameters;

        SqlServerObjectType ISqlServerObject.ObjectType => SqlServerObjectType.MultiStatementTableFunction;

        string ISqlServerObject.ToStringSignature( bool withOptions )
        {
            return withOptions ? Header.ToStringCompact() : _content.Skip( 1 ).Take( 6 ).ToStringCompact();
        }

        void ISqlServerObject.Write( StringBuilder b )
        {
            Write( SqlTextWriter.CreateDefault( b ) );
        }

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


