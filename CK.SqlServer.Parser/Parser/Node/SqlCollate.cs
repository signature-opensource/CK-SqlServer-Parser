using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;

namespace CK.SqlServer.Parser
{
    public sealed class SqlCollate : SqlNode
    {
        readonly SNode<ISqlNode, SqlTokenIdentifier, SqlTokenIdentifier> _content;

        public SqlCollate( ISqlNode left, SqlTokenIdentifier collateT, SqlTokenIdentifier nameT )
            : base( null, null )
        {
            _content = new SNode<ISqlNode, SqlTokenIdentifier, SqlTokenIdentifier>( left, collateT, nameT );
            CheckContent();
        }

        void CheckContent()
        {
            Helper.CheckNotNull( Left, nameof( Left ) );
            Helper.CheckToken( CollateT, nameof( CollateT ), SqlTokenType.Collate );
            Helper.CheckNotNull( CollationName, nameof( CollationName ) );
        }

        SqlCollate( SqlCollate o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _content = o._content;
            else
            {
                _content = new SNode<ISqlNode, SqlTokenIdentifier, SqlTokenIdentifier>( items );
                CheckContent();
            }
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlCollate( this, leading, content, trailing );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

        public ISqlNode Left => _content.V1;

        public SqlTokenIdentifier CollateT => _content.V2;

        public SqlTokenIdentifier CollationName => _content.V3;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

    }

}
