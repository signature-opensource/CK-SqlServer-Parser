#region Proprietary License
/*----------------------------------------------------------------------------
* This file (CK.SqlServer.Parser\Parser\Expr\Select\SelectOrderByColumnList.cs) is part of CK-Database. 
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
    /// List of comma separated <see cref="SelectOrderByColumn"/>
    /// </summary>
    public class SelectOrderByColumnList : SqlNoExprList<SelectOrderByColumn>
    {
        public SelectOrderByColumnList( IList<SqlNode> components )
            : base( components )
        {
        }

        internal SelectOrderByColumnList( ImmutableList<SqlTrivia> leading, SqlNode[] items, ImmutableList<SqlTrivia> trailing )
            : base( leading, items, trailing )
        {
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IReadOnlyList<SqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SelectOrderByColumnList( leading, EnsureArray( children ), trailing );
        }

        [DebuggerStepThrough]
        internal protected override SqlNode Accept( SqlItemVisitor visitor )
        {
            return visitor.Visit( this );
        }
    }


}
