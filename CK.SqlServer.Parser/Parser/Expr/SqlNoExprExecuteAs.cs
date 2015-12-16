#region Proprietary License
/*----------------------------------------------------------------------------
* This file (CK.SqlServer.Parser\Parser\Expr\SqlNoExprExecuteAs.cs) is part of CK-Database. 
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
    public class SqlNoExprExecuteAs : SqlItem
    {
        public SqlNoExprExecuteAs( SqlTokenIdentifier execToken, SqlTokenIdentifier asToken, SqlToken right )
            : this( null, Build( execToken, asToken, right ), null )
        {
        }

        static SqlNode[] Build( SqlTokenIdentifier execT, SqlTokenIdentifier asT, SqlToken rightT )
        {
            if( execT == null || execT.TokenType != SqlTokenType.Execute ) throw new ArgumentException( "execT" );
            if( asT == null || asT.TokenType != SqlTokenType.As ) throw new ArgumentException( "asT" );
            if( rightT == null ) throw new ArgumentNullException( "rightT" );
            return new SqlNode[]{ execT, asT, rightT };
        }

        SqlNoExprExecuteAs( ImmutableList<SqlTrivia> leading, SqlNode[] items, ImmutableList<SqlTrivia> trailing )
            : base( leading, items, trailing )
        {
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IReadOnlyList<SqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlNoExprExecuteAs( leading, EnsureArray( children ), trailing );
        }

        public SqlTokenIdentifier ExecT { get { return (SqlTokenIdentifier)Slots[0]; } }

        protected SqlTokenIdentifier AsT { get { return (SqlTokenIdentifier)Slots[1]; } }

        public SqlToken RightT { get { return (SqlToken)Slots[2]; } }

        [DebuggerStepThrough]
        internal protected override SqlNode Accept( SqlItemVisitor visitor )
        {
            return visitor.Visit( this );
        }
    }

}
