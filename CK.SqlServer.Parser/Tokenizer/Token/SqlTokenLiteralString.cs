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
    public sealed class SqlTokenLiteralString : SqlTokenBaseLiteral
    {
        public SqlTokenLiteralString( SqlTokenType t, string value, ImmutableList<SqlTrivia> leadingTrivia = null, ImmutableList<SqlTrivia> trailingTrivia = null )
            : base( t, leadingTrivia, trailingTrivia )
        {
            if( (t & SqlTokenType.IsString) == 0 ) throw new ArgumentException( "Invalid token type.", "t" );
            if( value == null ) throw new ArgumentNullException( "value" );
            Value = value;
        }

        public bool IsUnicode { get { return TokenType == SqlTokenType.UnicodeString; } }

        public string Value { get; private set; }

        public override string LiteralValue { get { return String.Format( IsUnicode ? "N'{0}'" : "'{0}'", Value.Replace( "'", "''" ) ); } }

        public override SqlNode SetTrivias( ImmutableList<SqlTrivia> leading, ImmutableList<SqlTrivia> trailing )
        {
            return TriviasDiffer( ref leading, ref trailing )
                    ? new SqlTokenLiteralString( TokenType, Value, leading, trailing )
                    : this;
        }
    }

}
