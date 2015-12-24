#region Proprietary License
/*----------------------------------------------------------------------------
* This file (CK.SqlServer.Parser\Parser\Expr\SqlExprTypeDeclDateAndTime.cs) is part of CK-Database. 
* Copyright © 2007-2014, Invenietis <http://www.invenietis.com>. All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using CK.Core;

namespace CK.SqlServer.Parser
{
    public sealed class SqlTypeDeclDateAndTime : SqlNode, ISqlUnifiedTypeDecl
    {
        readonly SNode<SqlTokenIdentifier, SqlTokenOpenPar, SqlTokenLiteralInteger, SqlTokenClosePar> _content;

        public SqlTypeDeclDateAndTime( SqlTokenIdentifier id, SqlTokenOpenPar openPar = null, SqlTokenLiteralInteger secondScale = null, SqlTokenClosePar closePar = null )
            : base( null, null )
        {
            _content = new SNode<SqlTokenIdentifier, SqlTokenOpenPar, SqlTokenLiteralInteger, SqlTokenClosePar>( id, openPar, secondScale, closePar );
             DbType = CheckContent();
        }

        SqlDbType CheckContent()
        {
            SNode.CheckNotNull( TypeIdentifierT, nameof( TypeIdentifierT ) );
            if( _content.Count > 1 )
            {
                SNode.CheckNotNull( OpenPar, nameof( OpenPar ) );
                SNode.CheckNotNull( SyntaxSecondScale, nameof( SyntaxSecondScale ) );
                if( SyntaxSecondScale.Value > 7 ) throw new ArgumentException( "Fractional seconds precision must be less or equal to 7.", nameof( SyntaxSecondScale ) );
                SNode.CheckNotNull( ClosePar, nameof( ClosePar ) );
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

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IReadOnlyList<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlTypeDeclDateAndTime( this, leading, children, trailing );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public SqlTokenIdentifier TypeIdentifierT => _content.V1;

        public SqlDbType DbType { get; }

        public SqlTokenOpenPar OpenPar => _content.V2;

        public SqlTokenLiteralInteger SyntaxSecondScale => _content.V3;

        public SqlTokenClosePar ClosePar => _content.V4;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor ) => visitor.Visit( this );

        int ISqlServerUnifiedTypeDecl.SyntaxSize => -2;

        byte ISqlServerUnifiedTypeDecl.SyntaxPrecision => 0;

        byte ISqlServerUnifiedTypeDecl.SyntaxScale =>  0;

        int ISqlServerUnifiedTypeDecl.SyntaxSecondScale => _content.V3 != null ? _content.V3.Value : -1;

        string ISqlServerUnifiedTypeDecl.ToStringClean() => ChildrenNodes.ToStringCompact();

    }

}
