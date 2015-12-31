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
    public class SqlParameterDefaultValue : SqlNode, ISqlServerParameterDefaultValue
    {
        readonly SNode<SqlTokenTerminal, SqlToken, SqlTokenBaseLiteral> _content;

        public SqlParameterDefaultValue( SqlTokenTerminal assignT, SqlToken minusSign, SqlTokenBaseLiteral value )
            : base( null, null )
        {
            _content = new SNode<SqlTokenTerminal, SqlToken, SqlTokenBaseLiteral>( assignT, minusSign, value );
            CheckContent();
        }

        public SqlParameterDefaultValue( SqlTokenTerminal assignT, SqlTokenIdentifier nullOrVariable )
            : base( null, null )
        {
            _content = new SNode<SqlTokenTerminal, SqlToken, SqlTokenBaseLiteral>( assignT, nullOrVariable, null );
            CheckContent();
        }

        void CheckContent()
        {
            SNode.CheckToken( AssignT, nameof( AssignT ), SqlTokenType.Assign );
            if( IsVariable || IsNull )
            {
                SNode.CheckNull( LiteralT, nameof( LiteralT ) );
            }
            else
            {
                SNode.CheckNullableToken( MinusSignOrNullOrVariableT, nameof( MinusSignOrNullOrVariableT ), SqlTokenType.Minus );
                SNode.CheckNotNull( LiteralT, nameof( LiteralT ) );
            }
        }

        SqlParameterDefaultValue( SqlParameterDefaultValue o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _content = o._content;
            else
            {
                _content = new SNode<SqlTokenTerminal, SqlToken, SqlTokenBaseLiteral>( items );
                CheckContent();
            }
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlParameterDefaultValue( this, leading, children, trailing );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public SqlTokenTerminal AssignT => _content.V1;

        public SqlToken MinusSignOrNullOrVariableT => _content.V2;

        public bool IsVariable => _content.V2 != null && _content.V2.IsToken( SqlTokenType.IdentifierVariable );

        public bool IsNull => _content.V2 != null && _content.V2.IsToken( SqlTokenType.Null );

        public SqlTokenBaseLiteral LiteralT => _content.V3;

        public bool IsLiteral => _content.V3 != null;

        public bool HasMinusSign => _content.Count == 3;

        /// <summary>
        /// Gets the default value (<see cref="IsVariable"/> must be false).
        /// It can be <see cref="DBNull.Value"/>, a <see cref="Int32"/>, <see cref="Decimal"/>, 
        /// a <see cref="Double"/> or a string for too big numerics (that exceed Decimal .Net capacity) 
        /// and money: .Net <see cref="Decimal"/> type has only 28 digits whereas Sql server numerics 
        /// has 38. And money is actually a Int64 for sql server.
        /// </summary>
        public object NullOrLitteralDotNetValue
        {
            get
            {
                if( IsVariable ) throw new InvalidOperationException();
                if( IsNull ) return DBNull.Value;
                Debug.Assert( IsLiteral );
                SqlTokenBaseLiteral t = LiteralT;
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
        internal protected override ISqlNode Accept( SqlItemVisitor visitor ) => visitor.Visit( this );

    }

}
