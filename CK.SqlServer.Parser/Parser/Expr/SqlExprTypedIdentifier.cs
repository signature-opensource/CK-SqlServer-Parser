#region Proprietary License
/*----------------------------------------------------------------------------
* This file (CK.SqlServer.Parser\Parser\Expr\SqlExprTypedIdentifier.cs) is part of CK-Database. 
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
    /// An identifier (a <see cref="SqlTokenIdentifier"/>, typically a variable name) followed by a type declaration (<see cref="SqlExprTypeDecl"/>).
    /// </summary>
    public class SqlExprTypedIdentifier : SqlItem
    {
        public SqlExprTypedIdentifier( SqlTokenIdentifier identifier, SqlToken optAsToken,  SqlExprTypeDecl type )
            : this( null, Build( identifier, optAsToken, type ), null )
        {
        }

        private static SqlNode[] Build( SqlTokenIdentifier identifier, SqlToken optAsToken, SqlExprTypeDecl type )
        {
            if( identifier == null ) throw new ArgumentNullException( "identifier" );
            if( type == null ) throw new ArgumentNullException( "type" );
            return optAsToken != null 
                    ? CreateArray<SqlNode>( identifier, optAsToken, type ) 
                    : CreateArray<SqlNode>( identifier, type );
        }

        internal SqlExprTypedIdentifier( ImmutableList<SqlTrivia> leading, SqlNode[] items, ImmutableList<SqlTrivia> trailing )
            : base( leading, items, trailing )
        {
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IReadOnlyList<SqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlExprTypedIdentifier( leading, EnsureArray( children ), trailing );
        }


        public SqlTokenIdentifier Identifier { get { return (SqlTokenIdentifier)Slots[0]; } }

        /// <summary>
        /// Gets the optional AS token that may appear in function parameters between the parameter name
        /// and the type.
        /// </summary>
        public SqlToken AsToken { get { return Slots.Length == 2 ? null : (SqlToken)Slots[1]; } }

        public SqlExprTypeDecl TypeDecl { get { return (SqlExprTypeDecl)Slots[Slots.Length-1]; } }


        [DebuggerStepThrough]
        internal protected override T Accept<T>( ISqlItemVisitor<T> visitor )
        {
            return visitor.Visit( this );
        }
    }

}
