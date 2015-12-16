#region Proprietary License
/*----------------------------------------------------------------------------
* This file (CK.SqlServer.Parser\Parser\Expr\SqlExprTypeDeclDecimal.cs) is part of CK-Database. 
* Copyright © 2007-2014, Invenietis <http://www.invenietis.com>. All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using CK.Core;
using System.Collections.Immutable;

namespace CK.SqlServer.Parser
{
    public class SqlExprTypeDeclDecimal : SqlItem, ISqlExprUnifiedTypeDecl
    {
        public SqlExprTypeDeclDecimal( SqlTokenIdentifier id )
            : base( null, CreateArray( id ), null )
        {
            if( id == null ) throw new ArgumentNullException( "id" );
            if( id.TokenType != SqlTokenType.IdentifierTypeDecimal )
            {
                throw new ArgumentException( "Invalid decimal token.", "id" );
            }
            SyntaxPrecision = 0;
            SyntaxScale = 0;
        }

        public SqlExprTypeDeclDecimal( SqlTokenIdentifier id, SqlTokenTerminal openPar, SqlTokenLiteralInteger precision, SqlTokenTerminal closePar )
            : base( null, CreateArray<SqlNode>( id, openPar, precision, closePar ), null )
        {
            if( id == null ) throw new ArgumentNullException( "id" );
            if( openPar == null ) throw new ArgumentNullException( "openPar" );
            if( openPar.TokenType != SqlTokenType.OpenPar ) throw new ArgumentException( "Must be '('.", "openPar" );
            if( precision == null ) throw new ArgumentNullException( "secondScale" );
            if( closePar == null ) throw new ArgumentNullException( "closePar" );
            if( closePar.TokenType != SqlTokenType.ClosePar ) throw new ArgumentException( "Must be ')'.", "closePar" );
            if( id.TokenType != SqlTokenType.IdentifierTypeDecimal )
            {
                throw new ArgumentException( "Invalid decimal token.", "id" );
            }
            if( precision.Value <= 0 || precision.Value > 38 )
            {
                throw new ArgumentException( "Invalid precision.", "precision" );
            }
            SyntaxPrecision = (byte)precision.Value;
            SyntaxScale = 0;
        }

        public SqlExprTypeDeclDecimal( SqlTokenIdentifier id, SqlTokenTerminal openPar, SqlTokenLiteralInteger precision, SqlTokenTerminal comma, SqlTokenLiteralInteger scale, SqlTokenTerminal closePar )
             : base( null, CreateArray<SqlNode>( id, openPar, precision, comma, scale, closePar ), null )
        {
            if( id == null ) throw new ArgumentNullException( "id" );
            if( openPar == null ) throw new ArgumentNullException( "openPar" );
            if( openPar.TokenType != SqlTokenType.OpenPar ) throw new ArgumentException( "Must be '('.", "openPar" );
            if( precision == null ) throw new ArgumentNullException( "secondScale" );
            if( comma == null ) throw new ArgumentNullException( "comma" );
            if( comma.TokenType != SqlTokenType.Comma ) throw new ArgumentException( "Must be ','.", "comma" );
            if( scale == null ) throw new ArgumentNullException( "scale" );
            if( closePar == null ) throw new ArgumentNullException( "closePar" );
            if( closePar.TokenType != SqlTokenType.ClosePar ) throw new ArgumentException( "Must be ')'.", "closePar" );
            if( id.TokenType != SqlTokenType.IdentifierTypeDecimal )
            {
                throw new ArgumentException( "Invalid decimal token.", "id" );
            }
            if( precision.Value <= 0 || precision.Value > 38 )
            {
                throw new ArgumentException( "Invalid precision.", "precision" );
            }
            if( scale.Value < 0 || scale.Value > precision.Value )
            {
                throw new ArgumentException( "Invalid scale (must be less or equal to precision).", "scale" );
            }
            SyntaxPrecision = (byte)precision.Value;
            SyntaxScale = (byte)scale.Value;
        }


        SqlExprTypeDeclDecimal( ImmutableList<SqlTrivia> leading, SqlNode[] items, ImmutableList<SqlTrivia> trailing )
            : base( leading, items, trailing )
        {
            if( Slots.Length >= 4 )
            {
                SyntaxPrecision = (byte)((SqlTokenLiteralInteger)Slots[2]).Value;
                if( Slots.Length == 6 )
                {
                    SyntaxScale = (byte)((SqlTokenLiteralInteger)Slots[5]).Value;
                }
                else throw new ArgumentException( "invalid Decimal." );
            }
            else if( Slots.Length != 1 ) throw new ArgumentException( "invalid Decimal." );
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IReadOnlyList<SqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlExprTypeDeclDecimal( leading, EnsureArray( children ), trailing );
        }

        public SqlTokenIdentifier TypeIdentifierT  { get { return (SqlTokenIdentifier)Slots[0]; } }

        public SqlDbType DbType { get { return SqlDbType.Decimal; } }

        public byte SyntaxPrecision { get; private set; }

        public byte SyntaxScale { get; private set; }

        string ISqlServerUnifiedTypeDecl.ToStringClean() => ChildrenNodes.ToStringCompact();

        [DebuggerStepThrough]
        internal protected override SqlNode Accept( SqlItemVisitor visitor )
        {
            return visitor.Visit( this );
        }

        int ISqlServerUnifiedTypeDecl.SyntaxSize
        {
            get { return -2; }
        }

        int ISqlServerUnifiedTypeDecl.SyntaxSecondScale
        {
            get { return -1; }
        }
    }

}
