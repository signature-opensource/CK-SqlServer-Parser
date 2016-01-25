using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using CK.Core;
using System.Collections.Immutable;

namespace CK.SqlServer.Parser
{
   public sealed class SqlDeclareCursor : SqlNode, ISqlNamedStatement
    {
        readonly SNode<SqlTokenIdentifier, SqlTokenIdentifier, ISqlCursorDefinition, SqlTokenTerminal> _content;

        public SqlDeclareCursor( SqlTokenIdentifier declareToken, SqlTokenIdentifier cursorName, ISqlCursorDefinition cursor, SqlTokenTerminal terminator )
            : base( null, null )
        {
            _content = new SNode<SqlTokenIdentifier, SqlTokenIdentifier, ISqlCursorDefinition, SqlTokenTerminal>( declareToken, cursorName, cursor, terminator );
            CheckContent();
        }
     
        SqlDeclareCursor( SqlDeclareCursor o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _content = o._content;
            else
            {
                _content = new SNode<SqlTokenIdentifier, SqlTokenIdentifier, ISqlCursorDefinition, SqlTokenTerminal>( items );
                CheckContent();
            }
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlDeclareCursor( this, leading, children, trailing );
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
        internal protected override ISqlNode Accept( SqlItemVisitor visitor ) => visitor.Visit( this );

    }


}
