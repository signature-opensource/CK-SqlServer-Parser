#region Proprietary License
/*----------------------------------------------------------------------------
* This file (CK.SqlServer.Parser\Tokenizer\Token\SqlTokenError.cs) is part of CK-Database. 
* Copyright © 2007-2014, Invenietis <http://www.invenietis.com>. All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

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
            ErrorMessage = message ?? t.ToString();
        }

        public SqlTokenError( string message )
            : base( SqlTokenType.ErrorMask, null, null )
        {
            if( String.IsNullOrWhiteSpace( message ) ) throw new ArgumentNullException( "message" );
            ErrorMessage = message;
        }

        public string ErrorMessage { get; private set; }

        public new SqlTokenTypeError TokenType { get { return (SqlTokenTypeError)base.TokenType; } }

        public override SqlNode SetTrivias( ImmutableList<SqlTrivia> leading, ImmutableList<SqlTrivia> trailing )
        {
            return TriviasDiffer( ref leading, ref trailing )
                    ? new SqlTokenError( TokenType, leading, trailing, ErrorMessage )
                    : this;
        }

        public bool IsEndOfInput { get { return base.TokenType == SqlTokenType.EndOfInput; } }

        protected override void DoWrite( StringBuilder b )
        {
            b.AppendFormat( "[Error: {0}]", ErrorMessage ); 
        }


    }

}
