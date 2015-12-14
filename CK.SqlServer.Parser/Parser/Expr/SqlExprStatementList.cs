#region Proprietary License
/*----------------------------------------------------------------------------
* This file (CK.SqlServer.Parser\Parser\Expr\SqlExprStatementList.cs) is part of CK-Database. 
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
    /// List of <see cref="SqlExprBaseSt">statements</see>. 
    /// It is not a statement itself: the <see cref="SqlExprStBlock"/> is the composite statement (begin...end).
    /// </summary>
    public class SqlExprStatementList : SqlItem, IReadOnlyList<SqlExprBaseSt>
    {
        public SqlExprStatementList( IEnumerable<SqlExprBaseSt> statements )
            : this( null, statements.ToArray(), null )
        {
        }

        SqlExprStatementList( ImmutableList<SqlTrivia> leading, SqlNode[] items, ImmutableList<SqlTrivia> trailing )
            : base( leading, items, trailing )
        {
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IReadOnlyList<SqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlExprStatementList( leading, EnsureArray( children ), trailing );
        }



        [DebuggerStepThrough]
        internal protected override T Accept<T>( ISqlItemVisitor<T> visitor )
        {
            return visitor.Visit( this );
        }

        public SqlExprBaseSt this[int index]
        {
            get { return (SqlExprBaseSt)Slots[index]; }
        }

        public int Count
        {
            get { return Slots.Length; }
        }

        public IEnumerator<SqlExprBaseSt> GetEnumerator()
        {
            return Slots.Cast<SqlExprBaseSt>().GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return Slots.GetEnumerator();
        }

    }


}
