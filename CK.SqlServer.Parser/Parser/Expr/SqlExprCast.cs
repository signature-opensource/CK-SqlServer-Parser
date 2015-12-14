#region Proprietary License
/*----------------------------------------------------------------------------
* This file (CK.SqlServer.Parser\Parser\Expr\SqlExprCast.cs) is part of CK-Database. 
* Copyright © 2007-2014, Invenietis <http://www.invenietis.com>. All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using CK.Core;
using System.Collections.Immutable;

namespace CK.SqlServer.Parser
{
    public class SqlExprCast : SqlExpr
    {
        public SqlExprCast( SqlTokenIdentifier castT, SqlTokenOpenPar openPar, SqlExpr e, SqlTokenIdentifier asT, SqlExprTypeDecl type, SqlTokenClosePar closePar )
            : this( null, Build( castT, openPar, e, asT, type, closePar ), null )
        {
        }

        static SqlNode[] Build( SqlTokenIdentifier castT, SqlTokenOpenPar openPar, SqlExpr e, SqlTokenIdentifier asT, SqlExprTypeDecl type, SqlTokenClosePar closePar )
        {
            if( castT == null ) throw new ArgumentNullException( "castTok" );
            if( openPar == null ) throw new ArgumentNullException( "openPar" );
            if( e == null ) throw new ArgumentNullException( "e" );
            if( asT == null ) throw new ArgumentNullException( "asTok" );
            if( type == null ) throw new ArgumentNullException( "type" );
            if( closePar == null ) throw new ArgumentNullException( "closePar" );
            return CreateArray<SqlNode>( SqlToken.EmptyOpenPar, castT, openPar, e, asT, type, closePar, SqlToken.EmptyClosePar );
        }

        protected SqlExprCast( ImmutableList<SqlTrivia> leading, SqlNode[] items, ImmutableList<SqlTrivia> trailing )
            : base( leading, items, trailing )
        {
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IReadOnlyList<SqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlExprCast( leading, EnsureArray( children ), trailing );
        }

        public SqlTokenIdentifier CastT { get { return (SqlTokenIdentifier)Slots[1]; } }

        public SqlTokenOpenPar OpenPar { get { return (SqlTokenOpenPar)Slots[2]; } }

        public SqlExpr Expression { get { return (SqlExpr)Slots[3]; } }

        public SqlTokenIdentifier AsT { get { return (SqlTokenIdentifier)Slots[4]; } }

        public SqlExprTypeDecl Type { get { return (SqlExprTypeDecl)Slots[5]; } }

        public SqlTokenClosePar ClosePar { get { return (SqlTokenClosePar)Slots[2]; } }

        [DebuggerStepThrough]
        internal protected override T Accept<T>( ISqlItemVisitor<T> visitor )
        {
            return visitor.Visit( this );
        }

    }
}
