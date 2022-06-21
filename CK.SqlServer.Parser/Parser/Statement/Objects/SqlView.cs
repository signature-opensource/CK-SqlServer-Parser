using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CK.SqlServer.Parser
{
    using CNode = SNode<SqlCreateOrAlter,
                        SqlTokenIdentifier,
                        ISqlIdentifier,
                        SqlEnclosedIdentifierCommaList,
                        SqlNodeList,
                        SqlTokenIdentifier,
                        ISqlNode,
                        SqlNodeList,
                        SqlTokenTerminal>;

    public sealed class SqlView : SqlNonTokenAutoWidth, ISqlNamedStatement, ISqlServerView, ISqlFullNameHolder, ISqlServerObjectOptions
    {
        readonly CNode _content;
        // Cached formal columns.
        IReadOnlyList<string> _formalColumnList;

        public SqlView( SqlCreateOrAlter createOrAlter,
                        SqlTokenIdentifier type,
                        ISqlIdentifier name,
                        SqlEnclosedIdentifierCommaList columns,
                        SqlNodeList options,
                        SqlTokenIdentifier asToken,
                        ISqlNode select,
                        SqlNodeList withCheckOption,
                        SqlTokenTerminal term )
            : base( null, null )
        {
            _content = new CNode( createOrAlter,
                                  type,
                                  name,
                                  columns,
                                  options,
                                  asToken,
                                  select,
                                  withCheckOption,
                                  term );
            CheckContent();
        }

        void CheckContent()
        {
            Helper.CheckNotNull( CreateOrAlter, nameof( CreateOrAlter ) );
            Helper.CheckToken( ObjectTypeT, nameof( ObjectTypeT ), SqlTokenType.View );
            Helper.CheckNotNull( FullName, nameof( FullName ) );
            Helper.CheckNullableToken( AsT, nameof( AsT ), SqlTokenType.As );
            Helper.CheckNotNull( Select, nameof( Select ) );
        }

        SqlView( ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            _content = new CNode( items );
            CheckContent();
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlView( leading, content, trailing );
        }

        public StatementKnownName StatementKnownName
                                        => CreateOrAlter.StatementPrefix == CreateOrAlterStatementPrefix.Alter
                                            ? StatementKnownName.AlterView
                                            : (CreateOrAlter.StatementPrefix == CreateOrAlterStatementPrefix.Create
                                                ? StatementKnownName.CreateView
                                                : StatementKnownName.CreateOrAlterView);


        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

        public SqlCreateOrAlter CreateOrAlter => _content.V1;

        public SqlTokenIdentifier ObjectTypeT => _content.V2;

        /// <summary>
        /// Gets the name of the view without the schema.
        /// </summary>
        public string Name => FullName.GetPartName( 1 );

        /// <summary>
        /// Gets the schema name or null if there is no schema.
        /// </summary>
        public string Schema => FullName.GetPartName( 2 );

        /// <summary>
        /// Gets the full name of the view (may start with the Schema).
        /// </summary>
        public string SchemaName => FullName.ToStringHyperCompact();

        public ISqlIdentifier FullName => _content.V3;

        ISqlServerObject ISqlServerObject.SetSchema( string name )
        {
            return this.ReplaceContentNode( 2, FullName.SetPartName( 2, name ) );
        }

        SqlServerObjectType ISqlServerObject.ObjectType => SqlServerObjectType.View;

        public bool HasColumnNames => _content.V4 != null;

        public bool HasOptions => _content.V5 != null;

        public SqlEnclosedIdentifierCommaList ColumnNames => _content.V4;

        public SqlNodeList Options => _content.V5;

        public IEnumerable<ISqlNode> Header => _content.Skip( 1 ).Take( HasOptions ? 4 : 3 );

        public SqlTokenIdentifier AsT => _content.V6;

        public ISqlNode Select => _content.V7;

        public SqlNodeList WithCheckOption => _content.V8;

        public SqlTokenTerminal StatementTerminator => _content.V9;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

        IEnumerable<ISqlServerComment> ISqlServerParsedText.HeaderComments => FullLeadingTrivias.Cast<ISqlServerComment>();

        bool ISqlServerObjectOptions.SchemaBinding => HasOptions
                                                        ? Options.AllTokens.Any( t => t.TokenType == SqlTokenType.SchemaBinding )
                                                        : false;

        ISqlServerObjectOptions ISqlServerObject.Options => this;

        string ISqlServerObject.ToStringSignature( bool withOptions )
        {
            return withOptions ? Header.ToStringCompact() : _content.Skip( 1 ).Take( 3 ).ToStringCompact();
        }

        void ISqlServerParsedText.Write( StringBuilder b ) => Write( SqlTextWriter.CreateDefault( b ) );

        CreateOrAlterStatementPrefix ISqlServerAlterOrCreateStatement.StatementPrefix => CreateOrAlter.StatementPrefix;

        public ISqlServerAlterOrCreateStatement WithStatementPrefix( CreateOrAlterStatementPrefix prefix )
        {
            var newOne = CreateOrAlter.WithStatementPrefix( prefix );
            return newOne != CreateOrAlter ? this.ReplaceContentNode( 0, newOne ) : this;
        }

        IReadOnlyList<string> ISqlServerView.FormalColumnList
        {
            get
            {
                if( _formalColumnList == null && HasColumnNames )
                {
                    _formalColumnList = ColumnNames.Select( id => id.ToString() ).ToArray();
                }
                return _formalColumnList;
            }
        }

    }
}
