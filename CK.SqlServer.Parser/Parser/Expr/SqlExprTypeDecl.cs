#region Proprietary License
/*----------------------------------------------------------------------------
* This file (CK.SqlServer.Parser\Parser\Expr\SqlExprTypeDecl.cs) is part of CK-Database. 
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
    /// <summary>
    /// Wrapper for <see cref="ActualType">actual type</see> information (such as nvarchar(45), decimal(15,4), or datetime).
    /// </summary>
    public class SqlExprTypeDecl : SqlItem
    {
        public SqlExprTypeDecl( ISqlExprUnifiedTypeDecl actualType )
            : this( null, CreateArray( (SqlNode)actualType ), null )
        {
            if( actualType == null ) throw new ArgumentNullException( "actualType" );
        }

        SqlExprTypeDecl( ImmutableList<SqlTrivia> leading, SqlNode[] items, ImmutableList<SqlTrivia> trailing )
            : base( leading, items, trailing )
        {
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IReadOnlyList<SqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlExprTypeDecl( leading, EnsureArray( children ), trailing );
        }

        /// <summary>
        /// Gets a unified type for different kind of type declaration.
        /// </summary>
        public ISqlExprUnifiedTypeDecl ActualType { get { return (ISqlExprUnifiedTypeDecl)Slots[0]; } }

        [DebuggerStepThrough]
        internal protected override T Accept<T>( ISqlItemVisitor<T> visitor )
        {
            return visitor.Visit( this );
        }

    }

}
