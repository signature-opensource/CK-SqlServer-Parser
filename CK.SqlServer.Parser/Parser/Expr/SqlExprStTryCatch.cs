#region Proprietary License
/*----------------------------------------------------------------------------
* This file (CK.SqlServer.Parser\Parser\Expr\SqlExprStTryCatch.cs) is part of CK-Database. 
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

namespace CK.SqlServer.Parser
{
    /// <summary>
    /// A try/catch block is defined by begin try...end try begin catch...end catch.
    /// </summary>
    public class SqlExprStTryCatch : SqlExprBaseSt
    {
        public SqlExprStTryCatch( SqlTokenList<SqlTokenIdentifier> beginTry, 
                                  SqlExprStatementList body, 
                                  SqlTokenList<SqlTokenIdentifier> endTryBeginCatch, 
                                  SqlExprStatementList bodycatch, 
                                  SqlTokenList<SqlTokenIdentifier>  endCatch,
                                  SqlTokenTerminal statementTerminator = null )
            : base( CreateArray( beginTry, body, endTryBeginCatch, bodycatch, endCatch ), statementTerminator )
        {
        }

        public SqlTokenList<SqlTokenIdentifier> BeginTry { get { return (SqlTokenList<SqlTokenIdentifier>)Slots[0]; } }
        
        public SqlExprStatementList Body { get { return (SqlExprStatementList)Slots[1]; } }

        public SqlTokenList<SqlTokenIdentifier> EndTryBeginCatch { get { return (SqlTokenList<SqlTokenIdentifier>)Slots[2]; } }

        public SqlExprStatementList BodyCatch { get { return (SqlExprStatementList)Slots[3]; } }

        public SqlTokenList<SqlTokenIdentifier> EndCatch { get { return (SqlTokenList<SqlTokenIdentifier>)Slots[4]; } }

        [DebuggerStepThrough]
        internal protected override T Accept<T>( ISqlItemVisitor<T> visitor )
        {
            return visitor.Visit( this );
        }

    }


}
