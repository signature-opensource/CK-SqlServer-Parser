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
    public sealed class SqlExecuteStringStatement : SqlNode, ISqlNamedStatement
    {
        readonly SNode<SqlTokenIdentifier, SqlEnclosedCommaList, SqlNodeList, SqlTokenTerminal> _content;

        public SqlExecuteStringStatement( SqlTokenIdentifier execT, SqlEnclosedCommaList arguments, SqlNodeList options = null, SqlTokenTerminal term = null )
            : base( null, null )
        {
            _content = new SNode<SqlTokenIdentifier, SqlEnclosedCommaList, SqlNodeList, SqlTokenTerminal>( execT, arguments, options, term );
            CheckContent();
        }

        void CheckContent()
        {
            SNode.CheckToken( ExecT, nameof( ExecT ), SqlTokenType.Execute );
            SNode.CheckNotNull( Arguments, nameof( Arguments ) );
        }

        SqlExecuteStringStatement( SqlExecuteStringStatement o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _content = o._content;
            else
            {
                _content = new SNode<SqlTokenIdentifier, SqlEnclosedCommaList, SqlNodeList, SqlTokenTerminal>( items );
                CheckContent();
            }
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlExecuteStringStatement( this, leading, children, trailing );
        }

        public StatementKnownName StatementKnownName => StatementKnownName.ExecuteString;

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public SqlTokenIdentifier ExecT => _content.V1;

        public SqlEnclosedCommaList Arguments => _content.V2;

        public SqlNodeList Options => _content.V3;

        public SqlTokenTerminal StatementTerminator => _content.V4;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor ) => visitor.Visit( this );

    }

}
