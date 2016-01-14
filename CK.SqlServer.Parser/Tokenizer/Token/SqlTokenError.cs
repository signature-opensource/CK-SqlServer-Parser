using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using CK.Core;
using System.Diagnostics;
using System.Globalization;
using System.Collections.Immutable;

namespace CK.SqlServer.Parser
{
    /// <summary>
    /// Error tokens are bound to a <see cref="TokenType"/> that is a <see cref="SqlTokenTypeError"/>.
    /// </summary>
    public class SqlTokenError : SqlToken
    {
        public static readonly SqlTokenError EndOfInput = new SqlTokenError( SqlTokenTypeError.EndOfInput, null, null, null );

        public SqlTokenError( SqlTokenTypeError t, ImmutableList<SqlTrivia> leadingTrivia = null, ImmutableList<SqlTrivia> trailingTrivia = null, string message = null )
            : base( (SqlTokenType)t, leadingTrivia, trailingTrivia )
        {
            if( t >= 0 ) throw new ArgumentException( "Invalid error token type." );
            ErrorMessage = message ?? SqlKeyword.ToString( (SqlTokenType)t );
        }

        public SqlTokenError( string message )
            : base( SqlTokenType.ErrorMask, null, null )
        {
            if( string.IsNullOrWhiteSpace( message ) ) throw new ArgumentNullException( "message" );
            ErrorMessage = message;
        }

        public string ErrorMessage { get; private set; }

        public new SqlTokenTypeError TokenType { get { return (SqlTokenTypeError)base.TokenType; } }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlTokenError( TokenType, leading, trailing, ErrorMessage );
        }

        public bool IsEndOfInput => base.TokenType == SqlTokenType.EndOfInput;

        public override void WriteWithoutTrivias( ISqlTextWriter w )
        {
            w.Write( string.Format( "[Error: {0}]", ErrorMessage ) ); 
        }


        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor ) => visitor.Visit( this );

    }

}
