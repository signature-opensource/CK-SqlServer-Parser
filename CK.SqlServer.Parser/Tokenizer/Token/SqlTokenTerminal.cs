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

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IReadOnlyList<SqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlTokenTerminal( TokenType, leading, trailing );
        }

        public override void WriteWithoutTrivias( ISqlTextWriter w )
        {
            bool? whiteSpaceBefore = null;
            bool? whiteSpaceAfter = null;
            if( TokenType == SqlTokenType.Dot
                                || TokenType == SqlTokenType.Comma
                                || TokenType == SqlTokenType.SemiColon
                                || TokenType == SqlTokenType.Colon
                                || TokenType == SqlTokenType.DoubleColons )
            {
                whiteSpaceBefore = false;
            }
            if( TokenType == SqlTokenType.Dot
                                || TokenType == SqlTokenType.Colon
                                || TokenType == SqlTokenType.DoubleColons )
            {
                whiteSpaceAfter = false;
            }
            w.Write( SqlTokenizer.Explain( TokenType ), whiteSpaceBefore, whiteSpaceAfter );
        }
    }

}
