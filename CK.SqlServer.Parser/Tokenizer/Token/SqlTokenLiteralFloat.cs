using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

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

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlTokenLiteralFloat( TokenType, LiteralValue, Value, leading, trailing );
        }

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor )
        {
            return visitor.Visit( this );
        }
    }


}
