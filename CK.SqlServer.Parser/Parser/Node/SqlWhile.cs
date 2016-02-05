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
    public sealed class SqlWhile : SqlNode, ISqlNamedStatement
    {
        readonly SNode<SqlTokenIdentifier, ISqlNode, ISqlStatement, SqlTokenTerminal> _content;

        public SqlWhile( SqlTokenIdentifier whileT, ISqlNode condition, ISqlStatement statement, SqlTokenTerminal terminator )
            : base( null, null )
        {
            _content = new SNode<SqlTokenIdentifier, ISqlNode, ISqlStatement, SqlTokenTerminal>( whileT, condition, statement, terminator );
            CheckContent();
        }

        void CheckContent()
        {
            Helper.CheckToken( WhileT, nameof( WhileT ), SqlTokenType.While );
            Helper.CheckNotNull( Condition, nameof( Condition ) );
            Helper.CheckNotNull( Statement, nameof( Statement ) );
        }

        SqlWhile( SqlWhile o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _content = o._content;
            else
            {
                _content = new SNode<SqlTokenIdentifier, ISqlNode, ISqlStatement, SqlTokenTerminal>( items );
                CheckContent();
            }
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlWhile( this, leading, content, trailing );
        }

        public StatementKnownName StatementKnownName => StatementKnownName.While;

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

        public SqlTokenIdentifier WhileT => _content.V1;

        public ISqlNode Condition => _content.V2;

        public ISqlStatement Statement => _content.V3;

        public SqlTokenTerminal StatementTerminator => _content.V4;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

    }


}
