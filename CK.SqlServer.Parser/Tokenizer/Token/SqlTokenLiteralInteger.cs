using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

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

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlTokenLiteralInteger( TokenType, Value, leading, trailing );
        }

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

    }

}
