#region Proprietary License
/*----------------------------------------------------------------------------
* This file (CK.SqlServer.Parser\Parser\Expr\SqlExprStLabelDef.cs) is part of CK-Database. 
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
    /// Label definition (a target for the goto).
    /// </summary>
    public class SqlExprStLabelDef : SqlExprBaseSt
    {
        public SqlExprStLabelDef( SqlTokenIdentifier id, SqlTokenTerminal colon, SqlTokenTerminal statementTerminator )
            : base( Build( id, colon ), statementTerminator )
        {
        }

        static ISqlNode[] Build( SqlTokenIdentifier id, SqlTokenTerminal colon )
        {
            if( id == null
                || id.IsQuoted
                || SqlKeyword.IsReservedKeyword( id.Name )
                || id.TrailingTrivias.Count > 0
                || colon == null
                || colon.TokenType != SqlTokenType.Colon
                || colon.LeadingTrivias.Count > 0 ) throw new ArgumentException( "Invalid 'label:' definition." );
            return CreateArray<SqlNode>( id, colon );
        }

        SqlExprStLabelDef( ImmutableList<SqlTrivia> leading, ISqlNode[] items, ImmutableList<SqlTrivia> trailing )
            : base( leading, items, trailing )
        {
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IReadOnlyList<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlExprStLabelDef( leading, EnsureArray( children ), trailing );
        }

        public SqlTokenIdentifier IdentifierT { get { return (SqlTokenIdentifier)Slots[0]; } }

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor )
        {
            return visitor.Visit( this );
        }

    }


}
