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
    /// Covers operators, punctuations and brackets: the token is fully defined by its <see cref="SqlToken.TokenType"/> itself (no associated value is necessary).
    /// </summary>
    public class SqlTokenTerminal : SqlToken
    {
        public static readonly SqlTokenTerminal Dot = new SqlTokenTerminal( SqlTokenType.Dot, null, null );
        public static readonly SqlTokenTerminal Comma = new SqlTokenTerminal( SqlTokenType.Comma, null, null );
        public static readonly SqlTokenTerminal SemiColon = new SqlTokenTerminal( SqlTokenType.SemiColon, null, null );
        public static readonly SqlTokenOpenPar OpenPar = new SqlTokenOpenPar( null, null );
        public static readonly SqlTokenClosePar ClosePar = new SqlTokenClosePar( null, null );

        public SqlTokenTerminal( SqlTokenType t, ImmutableList<SqlTrivia> leadingTrivia = null, ImmutableList<SqlTrivia> trailingTrivia = null )
            : base( t, leadingTrivia, trailingTrivia )
        {
            if( (t & SqlTokenType.TerminalMask) == 0 ) throw new ArgumentException( "Invalid token type.", "t" );
        }

        public override SqlNode SetTrivias( ImmutableList<SqlTrivia> leading, ImmutableList<SqlTrivia> trailing )
        {
            return TriviasDiffer( ref leading, ref trailing )
                    ? new SqlTokenTerminal( TokenType, leading, trailing )
                    : this;
        }

        protected override void DoWrite( StringBuilder b )
        {
            b.Append( SqlTokenizer.Explain( TokenType ) );
        }
    }

}
