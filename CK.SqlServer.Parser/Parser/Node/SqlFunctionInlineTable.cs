using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using CK.Core;
using System.Collections.Immutable;

namespace CK.SqlServer.Parser
{
    public sealed class SqlFunctionInlineTable : SqlNonToken, 
                                                    ISqlNamedStatement, 
                                                    ISqlFullNameHolder,
                                                    ISqlParameterListHolder, 
                                                    ISqlServerFunctionInlineTable
    {
        readonly SNode<
            SqlTokenIdentifier,
            SqlTokenIdentifier,
            ISqlIdentifier,
            SqlParameterList,
            SqlTokenIdentifier,
            SqlTokenIdentifier,
            SqlWithOptions,
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
            SqlWithOptions options,
            SqlTokenIdentifier asToken,
            SqlTokenIdentifier returnToken,
            ISqlNode select, 
            SqlTokenTerminal term )
            : base( null, null ) 
        {
            _content = new SNode<SqlTokenIdentifier, SqlTokenIdentifier, ISqlIdentifier, SqlParameterList, SqlTokenIdentifier, SqlTokenIdentifier, SqlWithOptions, SqlTokenIdentifier, SqlTokenIdentifier, ISqlNode, SqlTokenTerminal>(
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
            Helper.CheckToken( AlterOrCreateT, nameof( AlterOrCreateT ), SqlTokenType.Alter, SqlTokenType.Create );
            Helper.CheckToken( ObjectTypeT, nameof( ObjectTypeT ), SqlTokenType.Function );
            Helper.CheckNotNull( Parameters, nameof( Parameters ) );
            Helper.CheckToken( ReturnsT, nameof( ReturnsT ), SqlTokenType.Returns );
            Helper.CheckNotNull( TableT, nameof( TableT ) );
            Helper.CheckNullableToken( AsT, nameof( AsT ), SqlTokenType.As );
            Helper.CheckNotNull( Select, nameof( Select ) );
        }

        SqlFunctionInlineTable( SqlFunctionInlineTable o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _content = o._content;
            else
            {
                _content = new SNode<SqlTokenIdentifier, SqlTokenIdentifier, ISqlIdentifier, SqlParameterList, SqlTokenIdentifier, SqlTokenIdentifier, SqlWithOptions, SqlTokenIdentifier, SqlTokenIdentifier, ISqlNode, SqlTokenTerminal>( items );
                CheckContent();
            }
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlFunctionInlineTable( this, leading, content, trailing );
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
        /// Gets the name of the function without schema.
        /// </summary>
        public string Name => FullName.GetPartName( 1 );

        /// <summary>
        /// Gets the schema name or null if there is no schema.
        /// </summary>
        public string Schema => FullName.GetPartName( 2 );

        /// <summary>
        /// Gets the full name of the function (may start with the Schema).
        /// </summary>
        public string SchemaName => FullName.ToStringHyperCompact();

        /// <summary>
        /// Gets the name of the function (may start with the Schema).
        /// </summary>
        public ISqlIdentifier FullName => _content.V3;

        public SqlFunctionInlineTable SetParameters( SqlParameterList parameters ) => this.ReplaceContentNode( 3, parameters );

        public SqlParameterList Parameters => _content.V4;

        public SqlTokenIdentifier ReturnsT => _content.V5;

        public SqlTokenIdentifier TableT => _content.V6;

        public bool HasOptions => _content.V7 != null;

        public SqlWithOptions Options => _content.V7;

        public IEnumerable<ISqlNode> Header => _content.Skip( 1 ).Take( HasOptions ? 6 : 5 );

        public SqlTokenIdentifier AsT => _content.V8;

        public SqlTokenIdentifier ReturnT => _content.V9;

        public ISqlNode Select => _content.V10;

        public SqlTokenTerminal StatementTerminator => _content.V11;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

        ISqlServerParameterList ISqlServerCallableObject.Parameters => _content.V4.ModelParameters;

        ISqlServerObject ISqlServerObject.SetSchema( string name )
        {
            return this.ReplaceContentNode( 2, FullName.SetPartName( 2, name ) );
        }
        SqlServerObjectType ISqlServerObject.ObjectType => SqlServerObjectType.InlineTableFunction;

        IEnumerable<ISqlServerComment> ISqlServerParsedText.HeaderComments
        {
            get { return FullLeadingTrivias.Where( t => t.TokenType != SqlTokenType.None ).Cast<ISqlServerComment>(); }
        }


        string ISqlServerObject.ToStringSignature( bool withOptions )
        {
            return withOptions ? Header.ToStringCompact() : _content.Skip( 1 ).Take( 5 ).ToStringCompact();
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
