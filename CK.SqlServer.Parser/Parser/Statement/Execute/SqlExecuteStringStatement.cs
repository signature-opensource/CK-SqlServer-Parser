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
    using CNode = SNode<SqlTokenIdentifier, SqlEnclosedCommaList, SqlNodeList, SqlTokenTerminal>;

    public sealed class SqlExecuteStringStatement : SqlNonTokenAutoWidth, ISqlExecuteStatement
    {
        readonly CNode _content;

        public SqlExecuteStringStatement( SqlTokenIdentifier execT, SqlEnclosedCommaList arguments, SqlNodeList options = null, SqlTokenTerminal term = null )
            : base( null, null )
        {
            _content = new CNode( execT, arguments, options, term );
            CheckContent();
        }

        void CheckContent()
        {
            Helper.CheckToken( ExecT, nameof( ExecT ), SqlTokenType.Execute );
            Helper.CheckNotNull( Arguments, nameof( Arguments ) );
        }

        SqlExecuteStringStatement( SqlExecuteStringStatement o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
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
            return new SqlExecuteStringStatement( this, leading, content, trailing );
        }

        bool ISqlExecuteStatement.IsExecuteString => true;

        public StatementKnownName StatementKnownName => StatementKnownName.ExecuteString;

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

        public SqlTokenIdentifier ExecT => _content.V1;

        public SqlEnclosedCommaList Arguments => _content.V2;

        public SqlNodeList Options => _content.V3;

        public SqlTokenTerminal StatementTerminator => _content.V4;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

    }

}
