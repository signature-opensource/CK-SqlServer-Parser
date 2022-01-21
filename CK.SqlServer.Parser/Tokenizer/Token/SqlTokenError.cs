using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CK.SqlServer.Parser
{
    /// <summary>
    /// Error tokens are bound to a <see cref="TokenType"/> that is a <see cref="SqlTokenTypeError"/>.
    /// </summary>
    public class SqlTokenError : SqlToken
    {
        readonly string _errorMessage;

        public SqlTokenError( SqlTokenTypeError t, ImmutableList<SqlTrivia> leadingTrivia = null, ImmutableList<SqlTrivia> trailingTrivia = null, string message = null )
            : base( (SqlTokenType)t, leadingTrivia, trailingTrivia )
        {
            if( t >= 0 ) throw new ArgumentException( "Invalid error token type." );
            _errorMessage = message;
        }

        public SqlTokenError( string message )
            : base( SqlTokenType.ErrorMask, null, null )
        {
            if( string.IsNullOrWhiteSpace( message ) ) throw new ArgumentNullException( "message" );
            _errorMessage = message;
        }

        /// <summary>
        /// Gets the error message.
        /// </summary>
        public string ErrorMessage => SqlKeyword.ToString( base.TokenType ) 
                                        + (_errorMessage == null ? string.Empty : ": " + _errorMessage ); 

        public new SqlTokenTypeError TokenType => (SqlTokenTypeError)base.TokenType; 

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlTokenError( TokenType, leading, trailing, ErrorMessage );
        }

        public bool IsEndOfInput => base.TokenType == SqlTokenType.EndOfInput;

        public override void WriteWithoutTrivias( ISqlTextWriter w )
        {
            w.Write( base.TokenType, ErrorMessage ); 
        }

        public override bool TokenEquals( SqlToken t ) => ReferenceEquals( this, t );

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

    }

}
