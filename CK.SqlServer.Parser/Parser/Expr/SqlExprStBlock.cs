#region Proprietary License
/*----------------------------------------------------------------------------
* This file (CK.SqlServer.Parser\Parser\Expr\SqlExprStBlock.cs) is part of CK-Database. 
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
    /// A block is defined by begin...end enclosing keywords.
    /// </summary>
    public class SqlExprStBlock : SqlExprBaseSt
    {
        public SqlExprStBlock( SqlTokenIdentifier begin, SqlExprStatementList body, SqlTokenIdentifier end, SqlTokenTerminal statementTerminator = null )
            : base( CreateArray<SqlNode>( begin, body, end ), statementTerminator )
        {
        }

        SqlExprStBlock( ImmutableList<SqlTrivia> leading, ISqlNode[] items, ImmutableList<SqlTrivia> trailing )
            : base( leading, items, trailing )
        {
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IReadOnlyList<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlExprStBlock( leading, EnsureArray( children ), trailing );
        }

        public SqlTokenIdentifier BeginT { get { return (SqlTokenIdentifier)Slots[0]; } }

        public SqlExprStatementList Body { get { return (SqlExprStatementList)Slots[1]; } }

        public SqlTokenIdentifier EndT { get { return (SqlTokenIdentifier)Slots[2]; } }

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor )
        {
            return visitor.Visit( this );
        }

    }


}
