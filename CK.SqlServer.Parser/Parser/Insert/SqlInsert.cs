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
    public sealed class SqlInsert : SqlNode, ISqlNamedStatement
    {
        readonly SNode<SqlTokenIdentifier, SqlTokenIdentifier, SqlTokenTerminal> _content;

        public SqlInsert( SqlTokenIdentifier insertT, SqlTokenIdentifier target, SqlTokenTerminal terminator )
            : base( null, null )
        {
            _content = new SNode<SqlTokenIdentifier, SqlTokenIdentifier, SqlTokenTerminal>( insertT, target, terminator );
            CheckContent();
        }

        void CheckContent()
        {
            SNode.CheckToken( InsertT, nameof( InsertT ), SqlTokenType.Insert );
            SNode.CheckNotNull( Target, nameof( Target ) );
        }

        SqlInsert(SqlInsert o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _content = o._content;
            else
            {
                _content = new SNode<SqlTokenIdentifier, SqlTokenIdentifier, SqlTokenTerminal>( items );
                CheckContent();
            }
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlInsert( this, leading, children, trailing );
        }

        public StatementKnownName StatementKnownName => StatementKnownName.Insert;

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public SqlTokenIdentifier InsertT => _content.V1;

        public SqlTokenIdentifier Target => _content.V2;

        public SqlTokenTerminal StatementTerminator => _content.V3;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor ) => visitor.Visit( this );

    }


}
