#region Proprietary License
/*----------------------------------------------------------------------------
* This file (CK.SqlServer.Parser\Parser\Expr\SqlExprTypeDeclSimple.cs) is part of CK-Database. 
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

namespace CK.SqlServer.Parser
{
    public class SqlExprTypeDeclSimple : SqlItem, ISqlExprUnifiedTypeDecl
    {
        public SqlExprTypeDeclSimple( SqlTokenIdentifier id )
            : this( null, CreateArray( id ), null )
        {
        }

        void InitFromIdentifier()
        {
            SqlDbType? dbType = SqlKeyword.FromSqlTokenTypeToSqlDbType( TypeIdentifierT.TokenType );
            if( !dbType.HasValue )
            {
                throw new ArgumentException( "Invalid type.", "id" );
            }
            DbType = dbType.Value;
        }

        internal SqlExprTypeDeclSimple( SqlTokenIdentifier id, SqlDbType dbType )
            : base( null, CreateArray( id ), null )
        {
            Debug.Assert( dbType == SqlKeyword.FromSqlTokenTypeToSqlDbType( id.TokenType ) );
            DbType = dbType;
        }

        SqlExprTypeDeclSimple( ImmutableList<SqlTrivia> leading, SqlNode[] items, ImmutableList<SqlTrivia> trailing )
            : base( leading, items, trailing )
        {
            InitFromIdentifier();
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IReadOnlyList<SqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlExprTypeDeclSimple( leading, EnsureArray( children ), trailing );
        }

        public SqlDbType DbType { get; private set; }

        public SqlTokenIdentifier TypeIdentifierT { get { return (SqlTokenIdentifier)Slots[0]; } }

        string ISqlServerUnifiedTypeDecl.ToStringClean() => ChildrenNodes.ToStringCompact();

        [DebuggerStepThrough]
        internal protected override SqlNode Accept( SqlItemVisitor visitor )
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

        int ISqlServerUnifiedTypeDecl.SyntaxSecondScale
        {
            get { return -1; }
        }
    }

}
