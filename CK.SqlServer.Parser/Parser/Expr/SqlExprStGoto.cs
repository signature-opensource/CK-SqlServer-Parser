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
    public class SqlExprStGoto : SqlExprBaseSt
    {
        public SqlExprStGoto( SqlTokenIdentifier gotoToken, SqlTokenIdentifier target, SqlTokenTerminal terminator )
            : base( Build( gotoToken, target ),  terminator )
        {
        }

        SqlExprStGoto( ImmutableList<SqlTrivia> leading, ISqlNode[] items, ImmutableList<SqlTrivia> trailing )
            : base( leading, items, trailing )
        {
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IReadOnlyList<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlExprStGoto( leading, EnsureArray( children ), trailing );
        }

        static ISqlNode[] Build( SqlTokenIdentifier gotoToken, SqlTokenIdentifier target )
        {
            if( gotoToken == null || gotoToken.TokenType != SqlTokenType.Goto ) throw new ArgumentException( "gotoToken" );
            if( target == null ) throw new ArgumentException( "goto expects a target.", "target" );
            return CreateArray( gotoToken, target );
        }

        public SqlTokenIdentifier GotoT { get { return (SqlTokenIdentifier)Slots[0]; } }

        public SqlTokenIdentifier Target
        { 
            get { return (SqlTokenIdentifier)Slots[1]; } 
        }

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor )
        {
            return visitor.Visit( this );
        }

    }


}
