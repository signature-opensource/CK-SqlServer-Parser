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
    public sealed class SqlTokenLiteralBinary : SqlTokenBaseLiteral
    {
        public SqlTokenLiteralBinary( SqlTokenType t, string value, ImmutableList<SqlTrivia> leadingTrivia = null, ImmutableList<SqlTrivia> trailingTrivia = null )
            : base( t, leadingTrivia, trailingTrivia )
        {
            if( t != SqlTokenType.Binary ) throw new ArgumentException( "Invalid token type.", "t" );
            if( value == null ) throw new ArgumentNullException( "value" );
            Value = value;
        }

        public string Value { get; }

        public override string LiteralValue => Value; 

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlTokenLiteralBinary( TokenType, Value, leading, trailing );
        }

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor ) => visitor.Visit( this );

    }

}
