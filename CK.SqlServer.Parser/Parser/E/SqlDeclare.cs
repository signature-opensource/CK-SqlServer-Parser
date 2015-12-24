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
    /// <summary>
    /// 
    /// </summary>
    public class SqlDeclare : SqlNode, ISqlStatement
    {
        readonly SNode<SqlTokenIdentifier, SqlDeclareVariableList, SqlTokenTerminal> _content;

        public SqlDeclare( SqlTokenIdentifier declareToken, SqlDeclareVariableList declarations, SqlTokenTerminal terminator )
            : base( null, null )
        {
            _content = new SNode<SqlTokenIdentifier, SqlDeclareVariableList, SqlTokenTerminal>( declareToken, declarations, terminator );
            CheckContent();
        }

        SqlDeclare( SqlDeclare o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _content = o._content;
            else
            {
                _content = new SNode<SqlTokenIdentifier, SqlDeclareVariableList, SqlTokenTerminal>( items );
                CheckContent();
            }
        }

        void CheckContent()
        {
            SNode.CheckToken( DeclareT, nameof( DeclareT ), SqlTokenType.Declare );
            SNode.CheckNotNull( Declarations, nameof( Declarations ) );
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IReadOnlyList<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlDeclare( this, leading, children, trailing );
        }
        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public SqlTokenIdentifier DeclareT => _content.V1;

        public SqlDeclareVariableList Declarations => _content.V2;

        public SqlTokenTerminal StatementTerminator => _content.V3;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor ) => visitor.Visit( this );
    }

}

