using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CK.SqlServer.Parser
{
    using CNode = SNode<
                    SqlTokenIdentifier,
                    SqlTokenIdentifier,
                    ISqlCursorDefinition,
                    SqlTokenTerminal>;

    public sealed class SqlDeclareCursor : SqlNonTokenAutoWidth, ISqlNamedStatement
    {
        readonly CNode _content;

        public SqlDeclareCursor( SqlTokenIdentifier declareToken, SqlTokenIdentifier cursorName, ISqlCursorDefinition cursor, SqlTokenTerminal terminator )
            : base( null, null )
        {
            _content = new CNode( declareToken, cursorName, cursor, terminator );
            CheckContent();
        }
     
        SqlDeclareCursor( SqlDeclareCursor o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
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
            return new SqlDeclareCursor( this, leading, content, trailing );
        }

        void CheckContent()
        {
            Helper.CheckToken( DeclareT, nameof( DeclareT ), SqlTokenType.Declare );
            Helper.CheckNotNull( CursorName, nameof( CursorName ) );
            Helper.CheckNotNull( Cursor, nameof( Cursor ) );
        }

        public StatementKnownName StatementKnownName => StatementKnownName.DeclareCursor;

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

        public SqlTokenIdentifier DeclareT => _content.V1;

        public SqlTokenIdentifier CursorName => _content.V2;

        public ISqlCursorDefinition Cursor => _content.V3;

        public SqlTokenTerminal StatementTerminator => _content.V4;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

    }


}
