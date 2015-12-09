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
    public sealed class SqlTokenLiteralFloat : SqlTokenBaseLiteral
    {
        public SqlTokenLiteralFloat( SqlTokenType t, string literal, double value, ImmutableList<SqlTrivia> leadingTrivia = null, ImmutableList<SqlTrivia> trailingTrivia = null )
            : base( t, leadingTrivia, trailingTrivia )
        {
            if( t != SqlTokenType.Float ) throw new ArgumentException( "Invalid token type.", "t" );
            LiteralValue = literal;
            Value = value;
        }

        public double Value { get; }

        public override string LiteralValue { get; }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IReadOnlyList<SqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlTokenLiteralFloat( TokenType, LiteralValue, Value, leading, trailing );
        }
    }


}
