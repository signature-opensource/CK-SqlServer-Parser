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
    /// An identifier (a <see cref="SqlTokenIdentifier"/>, typically a variable name) followed by an
    /// optional 'as' and a type declaration (<see cref="ISqlUnifiedTypeDecl"/>).
    /// </summary>
    public sealed class SqlTypedIdentifier : SqlNode
    {
        readonly SNode<SqlTokenIdentifier, SqlTokenIdentifier, ISqlUnifiedTypeDecl> _content;

        public SqlTypedIdentifier( SqlTokenIdentifier identifier, SqlTokenIdentifier optAsToken, ISqlUnifiedTypeDecl type )
            : base( null, null )
        {
            _content = new SNode<SqlTokenIdentifier, SqlTokenIdentifier, ISqlUnifiedTypeDecl>( identifier, optAsToken, type );
            CheckContent();
        }

        void CheckContent()
        {
            SNode.CheckNotNull( Identifier, nameof( Identifier ) );
            SNode.CheckNullableToken( AsT, nameof( AsT ), SqlTokenType.As );
            SNode.CheckNotNull( TypeDecl, nameof( TypeDecl ) );
        }

        SqlTypedIdentifier( SqlTypedIdentifier o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _content = o._content;
            else
            {
                _content = new SNode<SqlTokenIdentifier, SqlTokenIdentifier, ISqlUnifiedTypeDecl>( items );
                CheckContent();
            }
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IReadOnlyList<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlTypedIdentifier( this, leading, children, trailing );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public SqlTokenIdentifier Identifier => _content.V1;

        /// <summary>
        /// Gets the optional AS token that may appear in function parameters between the parameter name
        /// and the type.
        /// </summary>
        public SqlTokenIdentifier AsT => _content.V2;

        public ISqlUnifiedTypeDecl TypeDecl => _content.V3;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor ) => visitor.Visit( this );
    }

}
