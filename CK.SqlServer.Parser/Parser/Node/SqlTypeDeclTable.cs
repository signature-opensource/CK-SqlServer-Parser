using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;

namespace CK.SqlServer.Parser
{
    public sealed class SqlTypeDeclTable : SqlNode, ISqlUnifiedTypeDecl
    {
        readonly SNode<SqlTokenIdentifier, SqlTokenOpenPar, ISqlNode, SqlTokenClosePar> _content;

        public SqlTypeDeclTable( SqlTokenIdentifier tableT, SqlTokenOpenPar opener, ISqlNode content, SqlTokenClosePar closer )
            : base( null, null )
        {
            _content = new SNode<SqlTokenIdentifier, SqlTokenOpenPar, ISqlNode, SqlTokenClosePar>( tableT, opener, content, closer );
        }

        void CheckContent()
        {
            SNode.CheckNotNull( TableT, nameof( TableT ) );
            SNode.CheckNotNull( Opener, nameof( Opener ) );
            SNode.CheckNotNull( Content, nameof( Content ) );
            SNode.CheckNotNull( Closer, nameof( Closer ) );
        }

        SqlTypeDeclTable( SqlTypeDeclTable o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _content = o._content;
            else
            {
                _content = new SNode<SqlTokenIdentifier, SqlTokenOpenPar, ISqlNode, SqlTokenClosePar>( items );
            }
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlTypeDeclTable( this, leading, children, trailing );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public SqlDbType DbType => SqlDbType.Structured;

        public SqlTokenIdentifier TableT => _content.V1;

        public SqlTokenOpenPar Opener => _content.V2;

        public ISqlNode Content => _content.V3;

        public SqlTokenClosePar Closer => _content.V4;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor ) => visitor.Visit( this );

        string ISqlServerUnifiedTypeDecl.ToStringClean() => ChildrenNodes.ToStringCompact();

        int ISqlServerUnifiedTypeDecl.SyntaxSize => -2;

        byte ISqlServerUnifiedTypeDecl.SyntaxPrecision => 0;

        byte ISqlServerUnifiedTypeDecl.SyntaxScale => 0;

        int ISqlServerUnifiedTypeDecl.SyntaxSecondScale => 1;
    }

}
