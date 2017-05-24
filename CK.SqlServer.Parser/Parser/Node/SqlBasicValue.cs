using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using CK.Core;
using CK.Text;
using System.Collections.Immutable;

namespace CK.SqlServer.Parser
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class SqlBasicValue : SqlNonToken, ISqlServerParameterDefaultValue
    {
        readonly SNode<SqlTokenTerminal, SqlToken> _content;

        public SqlBasicValue( SqlTokenTerminal minusT, SqlToken value )
            : base( null, null )
        {
            _content = new SNode<SqlTokenTerminal, SqlToken>( minusT, value );
            CheckContent();
        }

        SqlBasicValue( SqlBasicValue o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _content = o._content;
            else
            {
                _content = new SNode<SqlTokenTerminal, SqlToken>( items );
                CheckContent();
            }
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlBasicValue( this, leading, content, trailing );
        }

        void CheckContent()
        {
            Helper.CheckNullableToken( MinusT, nameof( MinusT ), SqlTokenType.Minus );
            Helper.CheckNotNull( Value, nameof( Value ) );
        }

        public StatementKnownName StatementKnownName => StatementKnownName.Return;

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

        public SqlTokenTerminal MinusT => _content.V1;

        public SqlToken Value => _content.V2;

        public bool IsVariable => Value.TokenType == SqlTokenType.IdentifierVariable;

        public bool IsNull => Value.TokenType == SqlTokenType.Null;

        public bool IsLiteral => Value is SqlTokenBaseLiteral;

        public bool HasMinusSign => MinusT != null;

        public object NullOrLitteralDotNetValue
        {
            get
            {
                if( IsVariable ) throw new InvalidOperationException();
                if( IsNull ) return DBNull.Value;
                if( (Value.TokenType & SqlTokenType.IsString) != 0 )
                {
                    return ((SqlTokenLiteralString)Value).Value;
                }
                Debug.Assert( (Value.TokenType & SqlTokenType.IsNumber) != 0 );
                if( Value.TokenType == SqlTokenType.Integer )
                {
                    int v = ((SqlTokenLiteralInteger)Value).Value;
                    return HasMinusSign ? -v : v;
                }
                if( Value.TokenType == SqlTokenType.Decimal )
                {
                    SqlTokenLiteralDecimal dec = (SqlTokenLiteralDecimal)Value;
                    if( dec.IsValidDecimalValue )
                    {
                        Decimal d = dec.DecimalValue;
                        return HasMinusSign ? -d : d;
                    }
                    string s = dec.ValueAsString;
                    return HasMinusSign ? '-' + s : s;
                }
                if( Value.TokenType == SqlTokenType.Float )
                {
                    double d = ((SqlTokenLiteralFloat)Value).Value;
                    return HasMinusSign ? -d : d;
                }
                if( Value.TokenType == SqlTokenType.Money )
                {
                    string s = ((SqlTokenLiteralMoney)Value).Value;
                    return HasMinusSign ? '-' + s : s;
                }
                if( Value.TokenType == SqlTokenType.Binary )
                {
                    string s = ((SqlTokenLiteralBinary)Value).Value;
                    Debug.Assert( s.StartsWith( "0x" ) );
                    if( s.Length == 2 ) return Util.Array.Empty<byte>();
                    byte[] val = new byte[(s.Length - 3) / 2 + 1];
                    int iB = val.Length-1;
                    for( int i = s.Length-1; i > 1;)
                    {
                        int low = s[i--].HexDigitValue();
                        char cHigh = s[i--];
                        if( cHigh != 'x')
                        {
                            val[iB--] = (byte)(cHigh.HexDigitValue() << 4 | low);
                        }
                        else val[iB--] = (byte)low;
                    }
                    return val;
                }
                throw new NotSupportedException( $"Token type: '{Value.TokenType}'." );
            }
        }


        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

    }


}
