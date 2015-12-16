#region Proprietary License
/*----------------------------------------------------------------------------
* This file (CK.SqlServer.Parser\Parser\Expr\SqlExprTypeDeclWithSize.cs) is part of CK-Database. 
* Copyright © 2007-2014, Invenietis <http://www.invenietis.com>. All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

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
    public class SqlExprTypeDeclWithSize : SqlItem, ISqlExprUnifiedTypeDecl
    {
        public SqlExprTypeDeclWithSize( SqlTokenIdentifier id )
            : base( null, CreateArray( id ), null )
        {
            if( id == null ) throw new ArgumentNullException( "id" );
            InitFromSingleIdentifier();
        }

        public SqlExprTypeDeclWithSize( SqlTokenIdentifier id, SqlTokenTerminal openPar, SqlToken size, SqlTokenTerminal closePar )
            : base( null, CreateArray( id, openPar, size, closePar ), null )
        {
            if( id == null ) throw new ArgumentNullException( "id" );
            if( openPar == null ) throw new ArgumentNullException( "openPar" );
            if( openPar.TokenType != SqlTokenType.OpenPar ) throw new ArgumentException( "Must be '('.", "openPar" );
            if( size == null ) throw new ArgumentNullException( "size" );
            if( !(size is SqlTokenLiteralInteger && ((SqlTokenLiteralInteger)size).Value > 0) 
                && !(size is SqlTokenIdentifier && ((SqlTokenIdentifier)size).TokenType == SqlTokenType.Max) ) throw new ArgumentException( "Size must be an integer greater than 0 or max.", "size" );
            if( closePar == null ) throw new ArgumentNullException( "closePar" );
            if( closePar.TokenType != SqlTokenType.ClosePar ) throw new ArgumentException( "Must be ')'.", "closePar" );
            InitFromSingleIdentifier();
            InitSyntaxSize();
        }

        internal SqlExprTypeDeclWithSize( SqlTokenIdentifier id, SqlDbType dbType )
            : base( null, CreateArray( id ), null )
        {
            Debug.Assert( id != null );
            Debug.Assert( dbType == SqlKeyword.FromSqlTokenTypeToSqlDbType( id.TokenType ).Value );
            Debug.Assert( dbType == SqlDbType.Char || dbType != SqlDbType.VarChar || dbType != SqlDbType.NChar || dbType != SqlDbType.NVarChar || dbType != SqlDbType.Binary || dbType != SqlDbType.VarBinary );
            DbType = dbType;
            SyntaxSize = 0;
        }

        internal SqlExprTypeDeclWithSize( SqlTokenIdentifier id, SqlTokenTerminal openPar, SqlToken size, SqlTokenTerminal closePar, SqlDbType dbType )
             : base( null, CreateArray( id, openPar, size, closePar ), null )
        {
            Debug.Assert( id != null && openPar != null && size != null && closePar != null );
            Debug.Assert( openPar.TokenType == SqlTokenType.OpenPar && closePar.TokenType == SqlTokenType.ClosePar );
            Debug.Assert( dbType == SqlKeyword.FromSqlTokenTypeToSqlDbType( id.TokenType ).Value );
            Debug.Assert( dbType == SqlDbType.Char || dbType != SqlDbType.VarChar || dbType != SqlDbType.NChar || dbType != SqlDbType.NVarChar || dbType != SqlDbType.Binary || dbType != SqlDbType.VarBinary );
            Debug.Assert( (size is SqlTokenLiteralInteger && ((SqlTokenLiteralInteger)size).Value > 0) || (size is SqlTokenIdentifier && ((SqlTokenIdentifier)size).TokenType == SqlTokenType.Max) );
            DbType = dbType;
            InitSyntaxSize();
        }

        void InitFromSingleIdentifier()
        {
            SqlDbType? dbType = SqlKeyword.FromSqlTokenTypeToSqlDbType( TypeIdentifierT.TokenType );
            if( !dbType.HasValue || (dbType != SqlDbType.Char && dbType != SqlDbType.VarChar && dbType != SqlDbType.NChar && dbType != SqlDbType.NVarChar && dbType != SqlDbType.Binary && dbType != SqlDbType.VarBinary) )
            {
                throw new ArgumentException( "Expected char, varchar, nchar, nvarchar, binary, varbinary.", "id" );
            }
            DbType = dbType.Value;
        }

        void InitSyntaxSize()
        {
            var sz = Slots[2];
            SyntaxSize = sz is SqlTokenLiteralInteger ? ((SqlTokenLiteralInteger)sz).Value : -1;
        }

        SqlExprTypeDeclWithSize( ImmutableList<SqlTrivia> leading, ISqlNode[] items, ImmutableList<SqlTrivia> trailing )
            : base( leading, items, trailing )
        {
            InitFromSingleIdentifier();
            if( Slots.Length == 4 ) InitSyntaxSize();
            else if( Slots.Length != 1 ) throw new ArgumentException( "invalid sized type." );
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IReadOnlyList<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlExprTypeDeclWithSize( leading, EnsureArray( children ), trailing );
        }

        public SqlTokenIdentifier TypeIdentifierT { get { return (SqlTokenIdentifier)Slots[0]; } }

        public SqlDbType DbType { get; private set; }

        public int SyntaxSize { get; private set; }

        string ISqlServerUnifiedTypeDecl.ToStringClean() => ChildrenNodes.ToStringCompact();

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor )
        {
            return visitor.Visit( this );
        }

        byte ISqlServerUnifiedTypeDecl.SyntaxPrecision
        {
            get { return 0; }
        }

        byte ISqlServerUnifiedTypeDecl.SyntaxScale
        {
            get { return 0; }
        }

        int ISqlServerUnifiedTypeDecl.SyntaxSecondScale
        {
            get { return -1; }
        }

    }

}
