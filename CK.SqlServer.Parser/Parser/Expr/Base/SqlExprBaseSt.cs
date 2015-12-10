#region Proprietary License
/*----------------------------------------------------------------------------
* This file (CK.SqlServer.Parser\Parser\Expr\Base\SqlExprBaseSt.cs) is part of CK-Database. 
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
    /// Base for all statements. It is a <see cref="SqlNoExpr"/> that handles the mandatory 
    /// statement terminator ';' that is required by ANSI SQL and future Sql Server versions.
    /// </summary>
    public abstract class SqlExprBaseSt : SqlItem
    {
        readonly SqlTokenTerminal _stmtTerminator;

        protected SqlExprBaseSt( IList<SqlNode> content, SqlTokenTerminal statementTerminator = null, ImmutableList<SqlTrivia> leading = null, ImmutableList<SqlTrivia> trailing = null )
            : this( leading, Build( content, statementTerminator ), trailing )
        {
        }

        private static SqlNode[] Build( IList<SqlNode> content, SqlTokenTerminal statementTerminator )
        {
            if( statementTerminator != null )
            {
                if( statementTerminator.TokenType != SqlTokenType.SemiColon ) throw new ArgumentException( "Statement terminator (;) expected.", "statementTerminator" );
                return CreateArray( content, content.Count, statementTerminator );
            }
            return content.ToArray();
        }

        protected SqlExprBaseSt( ImmutableList<SqlTrivia> leading, SqlNode[] slots, ImmutableList<SqlTrivia> trailing )
            : base( leading, slots, trailing )
        {
            _stmtTerminator = slots[slots.Length-1] as SqlTokenTerminal;
            if( _stmtTerminator != null && _stmtTerminator.TokenType != SqlTokenType.SemiColon ) _stmtTerminator = null;
        }

        protected int SlotsLengthWithoutTerminator
        {
            get { return _stmtTerminator != null ? Slots.Length - 1 : Slots.Length; }
        }

        public SqlTokenTerminal StatementTerminator { get { return _stmtTerminator; } }

        public IEnumerable<SqlNode> ComponentsWithoutTerminator
        {
            get { return _stmtTerminator != null ? Slots.Take( Slots.Length - 1 ) : Slots; }
        }
    }


}
