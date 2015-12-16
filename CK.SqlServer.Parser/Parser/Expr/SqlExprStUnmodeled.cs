#region Proprietary License
/*----------------------------------------------------------------------------
* This file (CK.SqlServer.Parser\Parser\Expr\SqlExprStUnmodeled.cs) is part of CK-Database. 
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
    /// Captures any statement: it can be any <see cref="SqlExpr"/> or <see cref="SqlNoExpr"/>.
    /// </summary>
    public class SqlExprStUnmodeled : SqlExprBaseSt
    {
        public SqlExprStUnmodeled( SqlItem content, SqlTokenTerminal statementTerminator = null )
            : base( CreateArray( content ), statementTerminator )
        {
            if( content == null ) throw new ArgumentNullException( "content" );
        }

        SqlExprStUnmodeled( ImmutableList<SqlTrivia> leading, SqlNode[] items, ImmutableList<SqlTrivia> trailing )
            : base( leading, items, trailing )
        {
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IReadOnlyList<SqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlExprStUnmodeled( leading, EnsureArray( children ), trailing );
        }

        public SqlItem Content { get { return (SqlItem)Slots[0]; } }

        [DebuggerStepThrough]
        internal protected override SqlNode Accept( SqlItemVisitor visitor )
        {
            return visitor.Visit( this );
        }

    }


}
