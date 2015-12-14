#region Proprietary License
/*----------------------------------------------------------------------------
* This file (CK.SqlServer.Parser\Parser\Expr\SqlExprIdentifier.cs) is part of CK-Database. 
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
    /// Mono identifier (wraps one <see cref="SqlTokenIdentifier"/>).
    /// </summary>
    public class SqlExprIdentifier : SqlExprBaseMonoToken<SqlTokenIdentifier>, ISqlIdentifier
    {
        public SqlExprIdentifier( SqlTokenIdentifier t )
            : base( t )
        {
        }

        internal SqlExprIdentifier( ImmutableList<SqlTrivia> leading, SqlNode[] items, ImmutableList<SqlTrivia> trailing )
            : base( leading, items, trailing )
        {
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IReadOnlyList<SqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlExprIdentifier( leading, EnsureArray( children ), trailing );
        }

        public string Name { get { return Token.Name; } }

        public bool IsVariable { get { return Token.IsVariable; } }

        SqlTokenIdentifier ISqlIdentifier.IdentifierAt( int index )
        {
            if( index != 0 ) throw new ArgumentOutOfRangeException();
            return Token;
        }

        int ISqlIdentifier.IdentifiersCount
        {
            get { return 1; }
        }

        IEnumerable<SqlTokenIdentifier> ISqlIdentifier.Identifiers
        {
            get { return ItemsWithoutParenthesis.Cast<SqlTokenIdentifier>(); }
        }

        [DebuggerStepThrough]
        internal protected override T Accept<T>( ISqlItemVisitor<T> visitor )
        {
            return visitor.Visit( this );
        }

    }


}
