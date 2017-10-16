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
    public sealed class SqlTypeDeclSimple : SqlNonTokenAutoWidth, ISqlUnifiedTypeDecl
    {
        readonly SNode<SqlTokenIdentifier> _content;

        public SqlTypeDeclSimple( SqlTokenIdentifier id )
            : base( null, null )
        {
            _content = new SNode<SqlTokenIdentifier>( id );
            DbType = CheckContent();
        }

        SqlDbType CheckContent()
        {
            Helper.CheckNotNull( TypeIdentifierT, nameof( TypeIdentifierT ) );
            SqlDbType? dbType = SqlKeyword.FromSqlTokenTypeToSqlDbType( TypeIdentifierT.TokenType );
            if( !dbType.HasValue )
            {
                throw new ArgumentException( "Invalid type.", nameof( TypeIdentifierT ) );
            }
            return dbType.Value;
        }

        internal SqlTypeDeclSimple( SqlDbType dbType, SqlTokenIdentifier id )
            : base( null, null )
        {
            _content = new SNode<SqlTokenIdentifier>( id );
            Debug.Assert( dbType == CheckContent() );
            DbType = dbType;
        }

        SqlTypeDeclSimple( SqlTypeDeclSimple o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null )
            {
                _content = o._content;
                DbType = o.DbType;
            }
            else
            {
                _content = new SNode<SqlTokenIdentifier>( items );
                DbType = CheckContent();
            }
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlTypeDeclSimple( this, leading, content, trailing );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

        public SqlDbType DbType { get; }

        public SqlTokenIdentifier TypeIdentifierT => _content.V;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

        string ISqlServerUnifiedTypeDecl.ToStringClean() => ChildrenNodes.ToStringCompact();

        int ISqlServerUnifiedTypeDecl.SyntaxSize => -2;

        byte ISqlServerUnifiedTypeDecl.SyntaxPrecision => 0;

        byte ISqlServerUnifiedTypeDecl.SyntaxScale => 0;

        int ISqlServerUnifiedTypeDecl.SyntaxSecondScale => 1;

    }

}
