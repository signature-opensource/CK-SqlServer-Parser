#region Proprietary License
/*----------------------------------------------------------------------------
* This file (CK.SqlServer.Parser\Parser\Expr\SqlExprParameterDefaultValue.cs) is part of CK-Database. 
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
    public class SqlExprParameterDefaultValue : SqlItem, ISqlServerParameterDefaultValue
    {
        public SqlExprParameterDefaultValue( SqlTokenTerminal assignToken, SqlTokenTerminal minusSign, SqlTokenBaseLiteral value )
            : this( null, Build( assignToken, minusSign, value ), null )
        {
        }

        public SqlExprParameterDefaultValue( SqlTokenTerminal assignToken, SqlTokenIdentifier variable )
            : this( null, CreateArray<SqlNode>( assignToken, variable ), null )
        {
            if( assignToken == null ) throw new ArgumentNullException( "assignToken" );
            if( variable == null ) throw new ArgumentNullException( "variable" );
        }

        static ISqlNode[] Build( SqlTokenTerminal assignToken, SqlTokenTerminal minusSign, SqlTokenBaseLiteral value )
        {
            if( assignToken == null ) throw new ArgumentNullException( "assignToken" );
            if( minusSign != null && minusSign.TokenType != SqlTokenType.Minus ) throw new ArgumentException( "Must be null or minus." );
            if( value == null ) throw new ArgumentNullException( "value" );

            return minusSign == null ? CreateArray<SqlNode>( assignToken, value ) : CreateArray<SqlNode>( assignToken, minusSign, value );
        }

        internal SqlExprParameterDefaultValue( ImmutableList<SqlTrivia> leading, ISqlNode[] items, ImmutableList<SqlTrivia> trailing )
            : base( leading, items, trailing )
        {
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IReadOnlyList<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlExprParameterDefaultValue( leading, EnsureArray( children ), trailing );
        }

        public bool IsVariable { get { return Slots.Length == 2 && Slots[1].IsToken( SqlTokenType.IdentifierVariable ); } }

        public bool IsNull { get { return Slots.Length == 2 && Slots[1].IsToken( SqlTokenType.Null ); } }
        
        public bool IsLiteral { get { return Slots.Length == 3 || Slots[1].IsLiteralToken(); } }

        public bool HasMinusSign { get { return Slots.Length == 3; } }

        /// <summary>
        /// Gets the default value (<see cref="IsVariable"/> must be false).
        /// It can be <see cref="DBNull.Value"/>, a <see cref="Int32"/>, <see cref="Decimal"/>, a <see cref="Double"/> or a string for 
        /// too big numerics (that exceed Decimal .Net capacity) and money:
        /// .Net <see cref="Decimal"/> type has only 28 digits whereas Sql server numerics has 38. And money is actually a Int64 for
        /// sql server.
        /// </summary>
        public object NullOrLitteralDotNetValue
        {
            get
            {
                if( IsVariable ) throw new InvalidOperationException();
                if( IsNull ) return DBNull.Value;
                Debug.Assert( IsLiteral );
                SqlTokenBaseLiteral t = (SqlTokenBaseLiteral)Slots[Slots.Length == 3 ? 2 : 1];
                if( (t.TokenType & SqlTokenType.IsString) != 0 )
                {
                    return ((SqlTokenLiteralString)t).Value;
                }
                Debug.Assert( (t.TokenType & SqlTokenType.IsNumber) != 0 );
                if( t.TokenType == SqlTokenType.Integer )
                {
                    int v = ((SqlTokenLiteralInteger)t).Value;
                    return HasMinusSign ? -v : v;
                }
                if( t.TokenType == SqlTokenType.Decimal )
                {
                    SqlTokenLiteralDecimal dec = (SqlTokenLiteralDecimal)t; 
                    if( dec.IsValidDecimalValue )
                    {
                        Decimal d = dec.DecimalValue;
                        return HasMinusSign ? -d : d;
                    }
                    string s = dec.ValueAsString;
                    return HasMinusSign ? '-' + s : s;
                }
                if( t.TokenType == SqlTokenType.Float )
                {
                    double d = ((SqlTokenLiteralFloat)t).Value;
                    return HasMinusSign ? -d : d;
                }
                if( t.TokenType == SqlTokenType.Money )
                {
                    string s = ((SqlTokenLiteralMoney)t).Value;
                    return HasMinusSign ? '-' + s : s;
                }
                throw new NotSupportedException();
            }
        }

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor )
        {
            return visitor.Visit( this );
        }

    }

}
