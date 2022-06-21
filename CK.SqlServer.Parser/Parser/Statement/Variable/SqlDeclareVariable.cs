using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CK.SqlServer.Parser
{
    using CNode = SNode<SqlTokenIdentifier, SqlVariableDeclarationList, SqlTokenTerminal>;
    /// <summary>
    /// 
    /// </summary>
    public class SqlDeclareVariable : SqlNonTokenAutoWidth, ISqlNamedStatement
    {
        readonly CNode _content;

        public SqlDeclareVariable( SqlTokenIdentifier declareToken, SqlVariableDeclarationList declarations, SqlTokenTerminal terminator )
            : base( null, null )
        {
            _content = new CNode( declareToken, declarations, terminator );
            CheckContent();
        }

        SqlDeclareVariable( SqlDeclareVariable o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _content = o._content;
            else
            {
                _content = new CNode( items );
                CheckContent();
            }
        }

        void CheckContent()
        {
            Helper.CheckToken( DeclareT, nameof( DeclareT ), SqlTokenType.Declare );
            Helper.CheckNotNull( Declarations, nameof( Declarations ) );
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlDeclareVariable( this, leading, content, trailing );
        }

        public StatementKnownName StatementKnownName => StatementKnownName.DeclareVariable;

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

        public SqlTokenIdentifier DeclareT => _content.V1;

        public SqlVariableDeclarationList Declarations => _content.V2;

        public SqlTokenTerminal StatementTerminator => _content.V3;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );
    }

}

