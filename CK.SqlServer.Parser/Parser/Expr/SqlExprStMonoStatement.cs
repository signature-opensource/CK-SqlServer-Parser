#region Proprietary License
/*----------------------------------------------------------------------------
* This file (CK.SqlServer.Parser\Parser\Expr\SqlExprStMonoStatement.cs) is part of CK-Database. 
* Copyright © 2007-2014, Invenietis <http://www.invenietis.com>. All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using CK.Core;
using System.Collections.Immutable;

namespace CK.SqlServer.Parser
{
    /// <summary>
    /// Mono identifier statements are "continue" or "break".
    /// </summary>
    public class SqlExprStMonoStatement : SqlExprBaseSt
    {
        public SqlExprStMonoStatement( SqlTokenIdentifier id, SqlTokenTerminal statementTerminator = null )
            : base( CreateArray( id ), statementTerminator )
        {
            if( id == null ) throw new ArgumentNullException( "id" );
        }

        SqlExprStMonoStatement( ImmutableList<SqlTrivia> leading, SqlNode[] items, ImmutableList<SqlTrivia> trailing )
            : base( leading, items, trailing )
        {
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IReadOnlyList<SqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlExprStMonoStatement( leading, EnsureArray( children ), trailing );
        }

        public SqlTokenIdentifier IdentifierT { get { return (SqlTokenIdentifier)Slots[0]; } }

        [DebuggerStepThrough]
        internal protected override T Accept<T>( ISqlItemVisitor<T> visitor )
        {
            return visitor.Visit( this );
        }

    }


}
