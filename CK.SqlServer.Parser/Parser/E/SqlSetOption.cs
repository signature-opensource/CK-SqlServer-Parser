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
    public sealed class SqlSetOption : SqlNode, ISqlStatement
    {
        readonly SNode<SqlTokenIdentifier, SqlNodeList, SqlTokenTerminal> _content;

        public SqlSetOption( SqlTokenIdentifier setToken, SqlNodeList list, SqlTokenTerminal terminator )
            : base( null, null )
        {
            _content = new SNode<SqlTokenIdentifier, SqlNodeList, SqlTokenTerminal>( setToken, list, terminator );
            CheckContent();
        }

        SqlSetOption( SqlSetOption o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _content = o._content;
            else
            {
                _content = new SNode<SqlTokenIdentifier, SqlNodeList, SqlTokenTerminal>( items );
                CheckContent();
            }
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IReadOnlyList<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlSetOption( this, leading, children, trailing );
        }

        void CheckContent()
        {
            if( SetT == null || SetT.TokenType != SqlTokenType.Set ) throw new ArgumentException( nameof( SetT ) );
            if( List == null ) throw new ArgumentException( nameof( List ) );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public SqlTokenIdentifier SetT => _content.V1;

        public SqlNodeList List => _content.V2;

        public SqlTokenTerminal StatementTerminator => _content.V3;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor ) => visitor.Visit( this );

    }


}
