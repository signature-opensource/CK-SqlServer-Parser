using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using CK.Core;
using System.Collections.Immutable;

namespace CK.SqlServer.Parser
{
    using CNode = SNode<SqlCreateOrAlter,
                        SqlTokenIdentifier,
                        ISqlIdentifier,
                        SqlParameterList,
                        SqlTokenIdentifier,
                        ISqlUnifiedTypeDecl,
                        SqlWithOptions,
                        SqlTokenIdentifier,
                        SqlTokenIdentifier,
                        SqlStatementList,
                        SqlTokenIdentifier,
                        SqlTokenTerminal>;

    public sealed class SqlFunctionScalar : SqlNonTokenAutoWidth, 
                                                ISqlNamedStatement, 
                                                ISqlFullNameHolder,
                                                ISqlParameterListHolder,
                                                ISqlServerFunctionScalar,
                                                ISqlServerObjectOptions
    {
        readonly CNode _content;

        public SqlFunctionScalar( 
            SqlCreateOrAlter createOrAlter, 
            SqlTokenIdentifier type,
            ISqlIdentifier name, 
            SqlParameterList parameters,
            SqlTokenIdentifier returns,
            ISqlUnifiedTypeDecl returnScalarType,
            SqlWithOptions options,
            SqlTokenIdentifier asToken,
            SqlTokenIdentifier begin,
            SqlStatementList bodyStatements, 
            SqlTokenIdentifier end, 
            SqlTokenTerminal term )
            : base( null, null )
        {
            _content = new CNode(
                 createOrAlter,
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
            Helper.CheckNotNull( CreateOrAlter, nameof( CreateOrAlter ) );
            Helper.CheckToken( ObjectTypeT, nameof( ObjectTypeT ), SqlTokenType.Function );
            Helper.CheckNotNull( Parameters, nameof( Parameters ) );
            Helper.CheckToken( ReturnsT, nameof( ReturnsT ), SqlTokenType.Returns );
            Helper.CheckNotNull( ReturnedType, nameof( ReturnedType ) );
            Helper.CheckNullableToken( AsT, nameof( AsT ), SqlTokenType.As );
            Helper.CheckToken( BeginT, nameof( BeginT ), SqlTokenType.Begin );
            Helper.CheckNotNull( BodyStatements, nameof( BodyStatements ) );
            Helper.CheckToken( EndT, nameof( EndT ), SqlTokenType.End );
        }

        SqlFunctionScalar( SqlFunctionScalar o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _content = o._content;
            else
            {
                _content = new CNode( items );
                CheckContent();
            }
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlFunctionScalar( this, leading, content, trailing );
        }

        public StatementKnownName StatementKnownName
                                        => CreateOrAlter.StatementPrefix == CreateOrAlterStatementPrefix.Alter
                                            ? StatementKnownName.AlterFunction
                                            : (CreateOrAlter.StatementPrefix == CreateOrAlterStatementPrefix.Create
                                                ? StatementKnownName.CreateFunction
                                                : StatementKnownName.CreateOrAlterFunction);

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

        public SqlCreateOrAlter CreateOrAlter => _content.V1;

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

        /// <summary>
        /// Gets the parameters.
        /// </summary>
        public SqlParameterList Parameters => _content.V4;

        public SqlFunctionScalar SetParameters( SqlParameterList parameters ) => this.ReplaceContentNode( 3, parameters );

        public SqlTokenIdentifier ReturnsT => _content.V5;

        public ISqlUnifiedTypeDecl ReturnedType => _content.V6;

        public bool HasOptions => _content.V7 != null;

        public SqlWithOptions Options => _content.V7;

        public IEnumerable<ISqlNode> Header => _content.Skip( 1 ).Take( HasOptions ? 6 : 5 );

        public SqlTokenIdentifier AsT => _content.V8;

        public SqlTokenIdentifier BeginT => _content.V9;

        public SqlStatementList BodyStatements => _content.V10;

        public SqlTokenIdentifier EndT => _content.V11;

        public SqlTokenTerminal StatementTerminator => _content.V12;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

        ISqlServerUnifiedTypeDecl ISqlServerFunctionScalar.ReturnType => _content.V6;

        ISqlServerParameterList ISqlServerCallableObject.Parameters => _content.V4.ModelParameters;

        ISqlServerObject ISqlServerObject.SetSchema( string name )
        {
            return this.ReplaceContentNode( 2, FullName.SetPartName( 2, name ) );
        }

        SqlServerObjectType ISqlServerObject.ObjectType => SqlServerObjectType.ScalarFunction;

        IEnumerable<ISqlServerComment> ISqlServerParsedText.HeaderComments
        {
            get { return FullLeadingTrivias.Where( t => t.TokenType != SqlTokenType.None ).Cast<ISqlServerComment>(); }
        }

        bool ISqlServerObjectOptions.SchemaBinding => HasOptions
                                                        ? Options.AllTokens.Any( t => t.TokenType == SqlTokenType.SchemaBinding )
                                                        : false;

        ISqlServerObjectOptions ISqlServerObject.Options => this;

        string ISqlServerObject.ToStringSignature( bool withOptions )
        {
            return withOptions ? Header.ToStringCompact() : _content.Skip( 1 ).Take( 5 ).ToStringCompact();
        }

        void ISqlServerParsedText.Write( StringBuilder b ) => Write( SqlTextWriter.CreateDefault( b ) );

        CreateOrAlterStatementPrefix ISqlServerAlterOrCreateStatement.StatementPrefix => CreateOrAlter.StatementPrefix;

        public ISqlServerAlterOrCreateStatement WithStatementPrefix( CreateOrAlterStatementPrefix prefix )
        {
            var newOne = CreateOrAlter.WithStatementPrefix( prefix );
            return newOne != CreateOrAlter ? this.ReplaceContentNode( 0, newOne ) : this;
        }

        ISqlParameterListHolder ISqlParameterListHolder.SetParameters( SqlParameterList parameters ) => SetParameters( parameters );

    }
}


