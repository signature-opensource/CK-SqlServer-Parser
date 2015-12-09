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
    public sealed class SqlTokenLiteralInteger : SqlTokenBaseLiteral
    {
        public SqlTokenLiteralInteger( SqlTokenType t, int value, ImmutableList<SqlTrivia> leadingTrivia = null, ImmutableList<SqlTrivia> trailingTrivia = null )
            : base( t, leadingTrivia, trailingTrivia )
        {
            if( t != SqlTokenType.Integer ) throw new ArgumentException( "Invalid token type.", "t" );
            Value = value;
        }

        public int Value { get; }

        public override string LiteralValue { get { return Value.ToString( CultureInfo.InvariantCulture ); } }

        protected override SqlNode Clone( ImmutableList<SqlTrivia> leading, IReadOnlyList<SqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return TriviasDiffer( ref leading, ref trailing )
                    ? new SqlTokenLiteralInteger( TokenType, Value, leading, trailing )
                    : this;
        }

    }

}
