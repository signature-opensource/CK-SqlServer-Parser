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
    public sealed class SqlSetOption : SqlNode, ISqlNamedStatement
    {
        readonly SNode<SqlTokenIdentifier, ISqlNode, SqlTokenTerminal> _content;

        public SqlSetOption( SqlTokenIdentifier setToken, ISqlNode options, SqlTokenTerminal terminator )
            : base( null, null )
        {
            _content = new SNode<SqlTokenIdentifier, ISqlNode, SqlTokenTerminal>( setToken, options, terminator );
            CheckContent();
        }

        SqlSetOption( SqlSetOption o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
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
            return new SqlSetOption( this, leading, children, trailing );
        }

        void CheckContent()
        {
            Helper.CheckToken( SetT, nameof( SetT ), SqlTokenType.Set );
            Helper.CheckNotNull( Options, nameof( Options ) );
        }

        public StatementKnownName StatementKnownName => StatementKnownName.SetOption;

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

        public SqlTokenIdentifier SetT => _content.V1;

        public ISqlNode Options => _content.V2;

        public SqlTokenTerminal StatementTerminator => _content.V3;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor ) => visitor.Visit( this );

    }


}
