#region Proprietary License
/*----------------------------------------------------------------------------
* This file (CK.SqlServer.Parser\Tokenizer\Token\SqlTokenLiteralDecimal.cs) is part of CK-Database. 
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
    public sealed class SqlTokenLiteralDecimal : SqlTokenBaseLiteral
    {
        public SqlTokenLiteralDecimal( SqlTokenType t, string value, ImmutableList<SqlTrivia> leadingTrivia = null, ImmutableList<SqlTrivia> trailingTrivia = null )
            : base( t, leadingTrivia, trailingTrivia )
        {
            if( t != SqlTokenType.Decimal ) throw new ArgumentException( "Invalid token type.", "t" );
            if( value == null ) throw new ArgumentNullException( "value" );
            ValueAsString = value;
            int precision, scale;

            int iDot = value.IndexOf( '.' );
            if( iDot >= 0 )
            {
                precision = value.Length - 1;
                if( iDot == 1 && value[0] == '0' ) --precision;
                scale = precision - iDot;
            }
            else
            {
                precision = value.Length;
                scale = 0;
            }
            Precision = (byte)precision;
            Scale = (byte)scale;
            Decimal d;
            IsValidDecimalValue = Decimal.TryParse( value, NumberStyles.Number, CultureInfo.InvariantCulture, out d );
            DecimalValue = d;
        }

        SqlTokenLiteralDecimal( SqlTokenLiteralDecimal x, ImmutableList<SqlTrivia> leadingTrivia, ImmutableList<SqlTrivia> trailingTrivia )
         : base( x.TokenType, leadingTrivia, trailingTrivia )
        {
            ValueAsString = x.ValueAsString;
            DecimalValue = x.DecimalValue;
            IsValidDecimalValue = x.IsValidDecimalValue;
            Precision = x.Precision;
            Scale = x.Scale;
        }

        /// <summary>
        /// Decimal is kept as a string, it is not converted to a numeric .Net type.
        /// Since .Net <see cref="Decimal"/> type has only 28 digits whereas Sql server numerics has 38.
        /// </summary>
        public string ValueAsString { get; private set; }

        /// <summary>
        /// Decimal value parsed if <see cref="IsValidDecimalValue"/> is true. 0 otherwise.
        /// </summary>
        public Decimal DecimalValue { get; private set; }

        /// <summary>
        /// Whether <see cref="DecimalVale"/> has been successfully parsed into a <see cref="Decimal"/> .Net type.
        /// </summary>
        public bool IsValidDecimalValue { get; private set; }

        public byte Precision { get; private set; }

        public byte Scale { get; private set; }

        public override string LiteralValue { get { return ValueAsString; } }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IReadOnlyList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlTokenLiteralDecimal( this, leading, trailing );
        }

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor )
        {
            return visitor.Visit( this );
        }
    }

}
