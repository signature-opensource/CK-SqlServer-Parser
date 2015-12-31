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
    public sealed class SqlReturn : SqlNode, ISqlNamedStatement
    {
        readonly SNode<SqlTokenIdentifier, ISqlNode, SqlTokenTerminal> _content;

        public SqlReturn( SqlTokenIdentifier returnToken, ISqlNode value, SqlTokenTerminal terminator )
            : base( null, null )
        {
            _content = new SNode<SqlTokenIdentifier,ISqlNode,SqlTokenTerminal>( returnToken, value, terminator );
            CheckContent();
        }

        SqlReturn( SqlReturn o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _content = o._content;
            else
            {
                _content = new SNode<SqlTokenIdentifier, ISqlNode, SqlTokenTerminal>( items );
                CheckContent();
            }
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlReturn( this, leading, children, trailing );
        }

        void CheckContent()
        {
            SNode.CheckToken( ReturnT, nameof( ReturnT ), SqlTokenType.Return );
        }

        public StatementKnownName StatementKnownName => StatementKnownName.Return;

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public SqlTokenIdentifier ReturnT => _content.V1;

        /// <summary>
        /// Gets the returned value. Can be null.
        /// </summary>
        public ISqlNode Value => _content.V2;

        public SqlTokenTerminal StatementTerminator => _content.V3;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor ) => visitor.Visit( this );

    }


}
