#region Proprietary License
/*----------------------------------------------------------------------------
* This file (CK.SqlServer.Parser\Parser\Expr\SqlExprUnmodeledItems.cs) is part of CK-Database. 
* Copyright © 2007-2014, Invenietis <http://www.invenietis.com>. All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using CK.Core;
using System.Diagnostics;
using System.Globalization;
using System.Collections.Immutable;

namespace CK.SqlServer.Parser
{
    public class SqlExprUnmodeledItems : SqlItem
    {
        public SqlExprUnmodeledItems( IEnumerable<SqlNode> items )
            : base( null, Build( items ), null )
        {
        }

        SqlExprUnmodeledItems( ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, Build( items ), trailing )
        {
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IReadOnlyList<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlExprUnmodeledItems( leading, EnsureArray( children ), trailing );
        }

        static ISqlNode[] Build( IEnumerable<ISqlNode> items )
        {
            if( items == null ) throw new ArgumentNullException( "items" );
            ISqlNode[] t = items.ToArray();
            if( t.Length == 0 ) throw new ArgumentException( "items" );
            return t;
        }

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor )
        {
            return visitor.Visit( this );
        }

    }

}
