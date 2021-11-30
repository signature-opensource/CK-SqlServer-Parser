using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using CK.Core;

namespace CK.SqlServer.Parser
{
    public sealed class SqlTypeDeclDateAndTime : SqlNonTokenAutoWidth, ISqlUnifiedTypeDecl
    {
        readonly SNode<SqlTokenIdentifier, SqlTokenOpenPar, SqlTokenLiteralInteger, SqlTokenClosePar> _content;

        public SqlTypeDeclDateAndTime( SqlTokenIdentifier id, SqlTokenOpenPar opener = null, SqlTokenLiteralInteger secondScale = null, SqlTokenClosePar closer = null )
            : base( null, null )
        {
            _content = new SNode<SqlTokenIdentifier, SqlTokenOpenPar, SqlTokenLiteralInteger, SqlTokenClosePar>( id, opener, secondScale, closer );
             DbType = CheckContent();
        }

        SqlDbType CheckContent()
        {
            Helper.CheckNotNull( TypeIdentifierT, nameof( TypeIdentifierT ) );
            if( _content.Count > 1 )
            {
                Helper.CheckNotNull( Opener, nameof( Opener ) );
                Helper.CheckNotNull( SyntaxSecondScale, nameof( SyntaxSecondScale ) );
                if( SyntaxSecondScale.Value > 7 ) throw new ArgumentException( "Fractional seconds precision must be less or equal to 7.", nameof( SyntaxSecondScale ) );
                Helper.CheckNotNull( Closer, nameof( Closer ) );
            }
            SqlDbType? dbType = SqlKeyword.FromSqlTokenTypeToSqlDbType( TypeIdentifierT.TokenType );
            if( !dbType.HasValue
                || (dbType.Value != SqlDbType.DateTime2 && dbType.Value != SqlDbType.Time && dbType.Value != SqlDbType.DateTimeOffset && dbType.Value != SqlDbType.DateTime && dbType.Value != SqlDbType.Date && dbType.Value != SqlDbType.SmallDateTime) )
            {
                throw new ArgumentException( "Invalid date/time type (must be date, datetime, smalldatetime, datetime2, time or datetimeoffset).", nameof( TypeIdentifierT ) );
            }
            return dbType.Value;
        }

        internal SqlTypeDeclDateAndTime( SqlDbType dbType, SqlTokenIdentifier id, SqlTokenOpenPar openPar = null, SqlTokenLiteralInteger secondScale = null, SqlTokenClosePar closePar = null )
             : base( null, null )
        {
            _content = new SNode<SqlTokenIdentifier, SqlTokenOpenPar, SqlTokenLiteralInteger, SqlTokenClosePar>( id, openPar, secondScale, closePar );
            Debug.Assert( CheckContent() == dbType );
            DbType = dbType;
        }

        SqlTypeDeclDateAndTime( SqlTypeDeclDateAndTime o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null )
            {
                _content = o._content;
                DbType = o.DbType;
            }
            else
            {
                _content = new SNode<SqlTokenIdentifier, SqlTokenOpenPar, SqlTokenLiteralInteger, SqlTokenClosePar>( items );
                DbType = CheckContent();
            }
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlTypeDeclDateAndTime( this, leading, content, trailing );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

        public SqlTokenIdentifier TypeIdentifierT => _content.V1;

        public SqlDbType DbType { get; }

        public SqlTokenOpenPar Opener => _content.V2;

        public SqlTokenLiteralInteger SyntaxSecondScale => _content.V3;

        public SqlTokenClosePar Closer => _content.V4;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

        int ISqlServerUnifiedTypeDecl.SyntaxSize => -2;

        byte ISqlServerUnifiedTypeDecl.SyntaxPrecision => 0;

        byte ISqlServerUnifiedTypeDecl.SyntaxScale =>  0;

        int ISqlServerUnifiedTypeDecl.SyntaxSecondScale => _content.V3 != null ? _content.V3.Value : -1;

        string ISqlServerUnifiedTypeDecl.ToStringClean() => ChildrenNodes.ToStringCompact();

    }

}
