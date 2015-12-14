#region Proprietary License
/*----------------------------------------------------------------------------
* This file (CK.SqlServer.Parser\Parser\Expr\SqlExprStReturn.cs) is part of CK-Database. 
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
    /// 
    /// </summary>
    public class SqlExprStDeclare : SqlExprBaseSt
    {
        public SqlExprStDeclare( SqlTokenIdentifier declareToken, SqlExprDeclareList declarations, SqlTokenTerminal terminator )
            : base( Build( declareToken, declarations ),  terminator )
        {
        }

        SqlExprStDeclare( ImmutableList<SqlTrivia> leading, SqlNode[] items, ImmutableList<SqlTrivia> trailing )
            : base( leading, items, trailing )
        {
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IReadOnlyList<SqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlExprStDeclare( leading, EnsureArray( children ), trailing );
        }

        static SqlNode[] Build( SqlTokenIdentifier declareToken, SqlExprDeclareList declarations )
        {
            if( declareToken == null || declareToken.TokenType != SqlTokenType.Declare ) throw new ArgumentException( "declareToken" );
            if( declarations == null ) throw new ArgumentException( "declarations" );
            return CreateArray<SqlNode>( declareToken, declarations );
        }

        public SqlTokenIdentifier DeclareT { get { return (SqlTokenIdentifier)Slots[0]; } }

        public SqlExprDeclareList Declarations { get { return (SqlExprDeclareList)Slots[1]; } }

        [DebuggerStepThrough]
        internal protected override T Accept<T>( ISqlItemVisitor<T> visitor )
        {
            return visitor.Visit( this );
        }

    }


}
