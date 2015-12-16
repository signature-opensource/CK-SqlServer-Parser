#region Proprietary License
/*----------------------------------------------------------------------------
* This file (CK.SqlServer.Parser\Parser\Expr\SqlExprStIf.cs) is part of CK-Database. 
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
    public class SqlExprStIf : SqlExprBaseSt
    {
        public SqlExprStIf( SqlTokenIdentifier ifToken, SqlExpr condition, SqlExprBaseSt thenStatement, SqlTokenIdentifier elseToken, SqlExprBaseSt elseStatement, SqlTokenTerminal terminator )
            : base( Build( ifToken, condition, thenStatement, elseToken, elseStatement ),  terminator )
        {
        }

        SqlExprStIf( ImmutableList<SqlTrivia> leading, ISqlNode[] items, ImmutableList<SqlTrivia> trailing )
            : base( leading, items, trailing )
        {
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IReadOnlyList<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlExprStIf( leading, EnsureArray( children ), trailing );
        }

        static ISqlNode[] Build( SqlTokenIdentifier ifToken, SqlExpr condition, SqlExprBaseSt thenStatement, SqlTokenIdentifier elseToken, SqlExprBaseSt elseStatement )
        {
            if( ifToken == null || !ifToken.NameEquals( "if" ) ) throw new ArgumentException( "ifToken" );
            if( condition == null ) throw new ArgumentNullException( "condition" );
            if( thenStatement == null ) throw new ArgumentNullException( "thenStatement" );
            if( (elseToken == null) != (elseStatement == null) ) throw new ArgumentException( "An else token requires and is required by an else statement." );

            return elseToken != null 
                    ? CreateArray<SqlNode>( ifToken, condition, thenStatement, elseToken, elseStatement ) 
                    : CreateArray<SqlNode>( ifToken, condition, thenStatement );
        }

        public SqlTokenIdentifier IfT { get { return (SqlTokenIdentifier)Slots[0]; } }
        public SqlExpr Condition { get { return (SqlExpr)Slots[1]; } }
        public SqlExprBaseSt ThenStatement { get { return (SqlExprBaseSt)Slots[2]; } }
        public bool HasElse { get { return Slots.Length > 3; } }
        public SqlTokenIdentifier ElseT { get { return HasElse ? (SqlTokenIdentifier)Slots[3] : null; } }
        public SqlExprBaseSt ElseStatement { get { return HasElse ? (SqlExprBaseSt)Slots[4] : null; } }

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor )
        {
            return visitor.Visit( this );
        }

    }


}
