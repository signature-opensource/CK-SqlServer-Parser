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
    public class SqlExprTypeDeclDateAndTime : SqlItem, ISqlExprUnifiedTypeDecl
    {
        public SqlExprTypeDeclDateAndTime( SqlTokenIdentifier id )
            : base( null, CreateArray<SqlNode>( id ), null )
        {
            if( id == null ) throw new ArgumentNullException( "token" );
            InitFromSingleToken();
        }

        public SqlExprTypeDeclDateAndTime( SqlTokenIdentifier id, SqlTokenTerminal openPar, SqlTokenLiteralInteger secondScale, SqlTokenTerminal closePar )
            : base( null, CreateArray<SqlNode>( id, openPar, secondScale, closePar ), null )
        {
            if( id == null ) throw new ArgumentNullException( "id" );
            if( openPar == null ) throw new ArgumentNullException( "openPar" );
            if( openPar.TokenType != SqlTokenType.OpenPar ) throw new ArgumentException( "Must be '('.", "openPar" );
            if( secondScale == null ) throw new ArgumentNullException( "secondScale" );
            if( secondScale.Value > 7 ) throw new ArgumentException( "Fractional seconds precision must be less or equal to 7.", "secondScale" );
            if( closePar == null ) throw new ArgumentNullException( "closePar" );
            if( closePar.TokenType != SqlTokenType.ClosePar ) throw new ArgumentException( "Must be ')'.", "closePar" );
            InitWithSecondScale();
        }

        void InitFromSingleToken()
        {
            SqlDbType? dbType = SqlKeyword.FromSqlTokenTypeToSqlDbType( TypeIdentifierT.TokenType );
            if( !dbType.HasValue
                || (dbType.Value != SqlDbType.DateTime2 && dbType.Value != SqlDbType.Time && dbType.Value != SqlDbType.DateTimeOffset && dbType.Value != SqlDbType.DateTime && dbType.Value != SqlDbType.Date && dbType.Value != SqlDbType.SmallDateTime) )
            {
                throw new ArgumentException( "Invalid date/time type (must be date, datetime, smalldatetime, datetime2, time or datetimeoffset).", "id" );
            }
            DbType = dbType.Value;
            SyntaxSecondScale = -1;
        }

        void InitWithSecondScale()
        {
            SqlDbType? dbType = SqlKeyword.FromSqlTokenTypeToSqlDbType( TypeIdentifierT.TokenType );
            if( !dbType.HasValue || (dbType.Value != SqlDbType.DateTime2 && dbType.Value != SqlDbType.Time && dbType.Value != SqlDbType.DateTimeOffset) )
            {
                throw new ArgumentException( "Invalid date/time type (must be datetime2, time or datetimeoffset).", "id" );
            }
            DbType = dbType.Value;
            SyntaxSecondScale = ((SqlTokenLiteralInteger)Slots[2]).Value;
        }

        internal SqlExprTypeDeclDateAndTime( SqlTokenIdentifier id, SqlTokenTerminal openPar, SqlTokenLiteralInteger secondScale, SqlTokenTerminal closePar, SqlDbType dbType )
             : base( null, CreateArray<SqlNode>( id, openPar, secondScale, closePar ), null )
        {
            Debug.Assert( id != null && openPar != null && secondScale != null && closePar != null );
            Debug.Assert( openPar.TokenType == SqlTokenType.OpenPar && closePar.TokenType == SqlTokenType.ClosePar );
            Debug.Assert( dbType == SqlKeyword.FromSqlTokenTypeToSqlDbType( id.TokenType ).Value && (dbType == SqlDbType.DateTime2 || dbType == SqlDbType.Time || dbType == SqlDbType.DateTimeOffset) );
            Debug.Assert( secondScale.Value >= 0 && secondScale.Value <= 7 );
            DbType = dbType;
            SyntaxSecondScale = secondScale.Value;
        }

        internal SqlExprTypeDeclDateAndTime( SqlTokenIdentifier id, SqlDbType dbType )
             : base( null, CreateArray<SqlNode>( id ), null )
        {
            Debug.Assert( id != null );
            Debug.Assert( SqlKeyword.FromSqlTokenTypeToSqlDbType( id.TokenType ).Value == dbType );
            DbType = dbType;
            SyntaxSecondScale = -1;
        }

        SqlExprTypeDeclDateAndTime( ImmutableList<SqlTrivia> leading, ISqlNode[] children, ImmutableList<SqlTrivia> trailing )
            : base( leading, children, trailing )
        {
            if( Slots.Length == 1 )
            {
                InitFromSingleToken();
            }
            else if( Slots.Length == 4 )
            {
                InitWithSecondScale();
            }
            else throw new ArgumentException( "Invalid datetime." );
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IReadOnlyList<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlExprTypeDeclDateAndTime( leading, EnsureArray( children ), trailing );
        }

        public SqlTokenIdentifier TypeIdentifierT { get { return (SqlTokenIdentifier)Slots[0]; } }

        public SqlDbType DbType { get; private set; }

        public int SyntaxSecondScale { get; private set; }

        string ISqlServerUnifiedTypeDecl.ToStringClean() => ChildrenNodes.ToStringCompact();

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor )
        {
            return visitor.Visit( this );
        }

        int ISqlServerUnifiedTypeDecl.SyntaxSize
        {
            get { return -2; }
        }

        byte ISqlServerUnifiedTypeDecl.SyntaxPrecision
        {
            get { return 0; }
        }

        byte ISqlServerUnifiedTypeDecl.SyntaxScale
        {
            get { return 0; }
        }

    }

}
