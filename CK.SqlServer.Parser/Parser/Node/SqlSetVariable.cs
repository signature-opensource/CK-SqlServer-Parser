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
    public sealed class SqlSetVariable : SqlNonTokenAutoWidth, ISqlNamedStatement
    {
        readonly SNode<SqlTokenIdentifier, SqlTokenIdentifier, SqlTokenTerminal, ISqlNode, SqlTokenTerminal> _content;

        public SqlSetVariable( SqlTokenIdentifier setToken, SqlTokenIdentifier variable, SqlTokenTerminal assignT, ISqlNode right, SqlTokenTerminal terminator )
            : base( null, null )
        {
            _content = new SNode<SqlTokenIdentifier, SqlTokenIdentifier, SqlTokenTerminal, ISqlNode, SqlTokenTerminal>(
                setToken,
                variable,
                assignT,
                right,
                terminator );
            CheckContent();
        }

        SqlSetVariable( SqlSetVariable o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _content = o._content;
            else
            {
                _content = new SNode<SqlTokenIdentifier, SqlTokenIdentifier, SqlTokenTerminal, ISqlNode, SqlTokenTerminal>( items );
                CheckContent();
            }
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlSetVariable( this, leading, content, trailing );
        }

        void CheckContent()
        {
            if( SetT == null || SetT.TokenType != SqlTokenType.Set ) throw new ArgumentException( nameof( SetT ) );
            if( Variable == null ) throw new ArgumentException( nameof( Variable ) );
            if( AssignT == null || (AssignT.TokenType & SqlTokenType.IsAssignOperator) == 0 ) throw new ArgumentException( nameof( AssignT ) );
            if( Value == null ) throw new ArgumentException( nameof( Value ) );
        }

        public StatementKnownName StatementKnownName => StatementKnownName.SetVariable;

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

        public SqlTokenIdentifier SetT => _content.V1;

        public SqlTokenIdentifier Variable => _content.V2;

        public SqlTokenTerminal AssignT => _content.V3;

        public ISqlNode Value => _content.V4;

        public SqlTokenTerminal StatementTerminator => _content.V5;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

    }

}
