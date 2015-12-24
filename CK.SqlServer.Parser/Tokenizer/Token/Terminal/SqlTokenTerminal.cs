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
        public static readonly SqlTokenTerminal Dot = new SqlTokenDot( null, null );
        public static readonly SqlTokenTerminal Comma = new SqlTokenComma( null, null );
        public static readonly SqlTokenTerminal SemiColon = new SqlTokenTerminal( SqlTokenType.SemiColon, null, null );
        public static readonly SqlTokenOpenPar OpenPar = new SqlTokenOpenPar( null, null );
        public static readonly SqlTokenClosePar ClosePar = new SqlTokenClosePar( null, null );

        protected SqlTokenTerminal( SqlTokenType t, ImmutableList<SqlTrivia> leadingTrivia = null, ImmutableList<SqlTrivia> trailingTrivia = null )
            : base( t, leadingTrivia, trailingTrivia )
        {
            Debug.Assert( t != SqlTokenType.Comma || GetType().Name == "SqlTokenComma" );
            Debug.Assert( t != SqlTokenType.Dot || GetType().Name == "SqlTokenDot" );
            Debug.Assert( t != SqlTokenType.OpenPar || GetType().Name == "SqlTokenOpenPar" );
            Debug.Assert( t != SqlTokenType.ClosePar || GetType().Name == "SqlTokenClosePar" );
            Debug.Assert( t != SqlTokenType.DoubleColons || GetType().Name == "SqlTokenDoubleColon" );
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IReadOnlyList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlTokenTerminal( TokenType, leading, trailing );
        }

        public static SqlTokenTerminal Create( SqlTokenType t, ImmutableList<SqlTrivia> lead, ImmutableList<SqlTrivia> tail )
        {
            if( (t & SqlTokenType.TerminalMask) == 0 ) throw new ArgumentException( "Must be a Terminal token.", "t" );
            switch( t )
            {
                case SqlTokenType.OpenPar: return new SqlTokenOpenPar( lead, tail );
                case SqlTokenType.ClosePar: return new SqlTokenClosePar( lead, tail );
                case SqlTokenType.Dot: return new SqlTokenDot( lead, tail );
                case SqlTokenType.Comma: return new SqlTokenComma( lead, tail );
                case SqlTokenType.DoubleColons: return new SqlTokenDoubleColon( lead, tail );
            }
            return new SqlTokenTerminal( t, lead, tail );
        }

        public override void WriteWithoutTrivias( ISqlTextWriter w )
        {
            bool? whiteSpaceBefore = null;
            bool? whiteSpaceAfter = null;
            if( TokenType == SqlTokenType.SemiColon
                                || TokenType == SqlTokenType.Colon )
            {
                whiteSpaceBefore = false;
            }
            if( TokenType == SqlTokenType.Colon )
            {
                whiteSpaceAfter = false;
            }
            w.Write( SqlTokenizer.Explain( TokenType ), whiteSpaceBefore, whiteSpaceAfter );
        }

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor )
        {
            return visitor.Visit( this );
        }
    }

}
