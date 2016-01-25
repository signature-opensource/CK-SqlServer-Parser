using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using CK.Core;
using System.Collections.Immutable;

namespace CK.SqlServer.Parser
{
    public sealed class SqlTypeDeclWithSize : SqlNode, ISqlUnifiedTypeDecl
    {
        readonly SNode<SqlTokenIdentifier, SqlTokenOpenPar, SqlToken, SqlTokenClosePar> _content;

        public SqlTypeDeclWithSize( SqlTokenIdentifier id )
            : base( null, null )
        {
            _content = new SNode<SqlTokenIdentifier, SqlTokenOpenPar, SqlToken, SqlTokenClosePar>( id, null, null, null );
            DbType = CheckContent();
        }
        public SqlTypeDeclWithSize( SqlTokenIdentifier id, SqlTokenOpenPar opener, SqlToken size, SqlTokenClosePar closer )
            : base( null, null )
        {
            _content = new SNode<SqlTokenIdentifier, SqlTokenOpenPar, SqlToken, SqlTokenClosePar>( id, opener, size, closer );
            DbType = CheckContent();
        }

        internal SqlTypeDeclWithSize( SqlDbType dbType, SqlTokenIdentifier id )
            : base( null, null )
        {
            _content = new SNode<SqlTokenIdentifier, SqlTokenOpenPar, SqlToken, SqlTokenClosePar>( id, null, null, null );
            Debug.Assert( dbType == CheckContent() );
            DbType = dbType;
        }

        internal SqlTypeDeclWithSize( SqlDbType dbType, SqlTokenIdentifier id, SqlTokenOpenPar opener, SqlToken size, SqlTokenClosePar closer )
             : base( null, null )
        {
            _content = new SNode<SqlTokenIdentifier, SqlTokenOpenPar, SqlToken, SqlTokenClosePar>( id, opener, size, closer );
            Debug.Assert( dbType == CheckContent() );
            DbType = dbType;
        }

        SqlDbType CheckContent()
        {
            Helper.CheckNotNull( TypeIdentifierT, nameof( TypeIdentifierT ) );
            if( _content.Count > 1 )
            {
                Helper.CheckNotNull( Opener, nameof( Opener ) );
                Helper.CheckNotNull( Size, nameof( Size ) );
                if( !(Size is SqlTokenLiteralInteger && ((SqlTokenLiteralInteger)Size).Value > 0)
                    && !(Size is SqlTokenIdentifier && ((SqlTokenIdentifier)Size).TokenType == SqlTokenType.Max) )
                {
                    throw new ArgumentException( "Size must be an integer greater than 0 or max.", nameof( Size ) );
                }
                Helper.CheckNotNull( Closer, nameof( Closer ) );
            }
            SqlDbType? dbType = SqlKeyword.FromSqlTokenTypeToSqlDbType( TypeIdentifierT.TokenType );
            if( !dbType.HasValue 
                || (dbType != SqlDbType.Char 
                    && dbType != SqlDbType.VarChar 
                    && dbType != SqlDbType.NChar 
                    && dbType != SqlDbType.NVarChar
                    && dbType != SqlDbType.Float
                    && dbType != SqlDbType.Binary
                    && dbType != SqlDbType.VarBinary) )
            {
                throw new ArgumentException( "Expected char, varchar, nchar, nvarchar, binary, varbinary.", nameof( TypeIdentifierT ) );
            }
            return dbType.Value;
        }

        SqlTypeDeclWithSize( SqlTypeDeclWithSize o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null )
            {
                _content = o._content;
                DbType = o.DbType;
            }
            else
            {
                _content = new SNode<SqlTokenIdentifier, SqlTokenOpenPar, SqlToken, SqlTokenClosePar>( items );
                DbType = CheckContent();
            }
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlTypeDeclWithSize( this, leading, children, trailing );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

        public SqlTokenIdentifier TypeIdentifierT => _content.V1;

        public SqlDbType DbType { get; }

        public SqlTokenOpenPar Opener => _content.V2;

        public SqlToken Size => _content.V3;

        public SqlTokenClosePar Closer => _content.V4;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor ) => visitor.Visit( this );

        int ISqlServerUnifiedTypeDecl.SyntaxSize => Size == null 
                                                        ? 0
                                                        : (Size is SqlTokenLiteralInteger 
                                                            ? ((SqlTokenLiteralInteger)Size).Value 
                                                            : -1);

        string ISqlServerUnifiedTypeDecl.ToStringClean() => ChildrenNodes.ToStringCompact();

        byte ISqlServerUnifiedTypeDecl.SyntaxPrecision => 0;

        byte ISqlServerUnifiedTypeDecl.SyntaxScale => 0;

        int ISqlServerUnifiedTypeDecl.SyntaxSecondScale => -1; 

    }

}
