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
        protected SqlTokenTerminal( SqlTokenType t, ImmutableList<SqlTrivia> leadingTrivia = null, ImmutableList<SqlTrivia> trailingTrivia = null )
            : base( t, leadingTrivia, trailingTrivia )
        {
            Debug.Assert( t != SqlTokenType.Comma || GetType().Name == "SqlTokenComma" );
            Debug.Assert( t != SqlTokenType.Dot || GetType().Name == "SqlTokenDot" );
            Debug.Assert( t != SqlTokenType.OpenPar || GetType().Name == "SqlTokenOpenPar" );
            Debug.Assert( t != SqlTokenType.ClosePar || GetType().Name == "SqlTokenClosePar" );
            Debug.Assert( t != SqlTokenType.DoubleColons || GetType().Name == "SqlTokenDoubleColon" );
            Debug.Assert( t != SqlTokenType.DoubleDots || GetType().Name == "SqlTokenMultiDots" );
            Debug.Assert( t != SqlTokenType.TripleDots || GetType().Name == "SqlTokenMultiDots" );
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
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
                case SqlTokenType.DoubleDots:
                case SqlTokenType.TripleDots: return new SqlTokenMultiDots( t, lead, tail );
                case SqlTokenType.QuestionMark:
                case SqlTokenType.DoubleQuestionMark:
                case SqlTokenType.TripleQuestionMark: return new SqlTokenQuestionMarks( t, lead, tail );
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
                if( TokenType == SqlTokenType.Colon )
                {
                    whiteSpaceAfter = false;
                }
            }
            else if( (TokenType&(SqlTokenType.IsAssignOperator|SqlTokenType.IsCompareOperator|SqlTokenType.IsBasicOperator)) != 0 )
            {
                whiteSpaceBefore = whiteSpaceAfter = false;
            }
            w.Write( TokenType, SqlKeyword.ToString( TokenType ), whiteSpaceBefore, whiteSpaceAfter );
        }

        public override bool TokenEquals( SqlToken t ) => TokenType == t.TokenType
                                                            || (TokenType == SqlTokenType.Equal && t.TokenType == SqlTokenType.Assign)
                                                            || (TokenType == SqlTokenType.Assign && t.TokenType == SqlTokenType.Equal)
                                                            || (TokenType == SqlTokenType.Mult && t.TokenType == SqlTokenType.IdentifierStar)
                                                            || (TokenType == SqlTokenType.IdentifierStar && t.TokenType == SqlTokenType.Mult)
                                                            || (TokenType == SqlTokenType.Different && t.TokenType == SqlTokenType.NotEqualTo)
                                                            || (TokenType == SqlTokenType.NotEqualTo && t.TokenType == SqlTokenType.Different)
                                                            || (TokenType == SqlTokenType.NotGreaterThan && t.TokenType == SqlTokenType.LessOrEqual)
                                                            || (TokenType == SqlTokenType.LessOrEqual && t.TokenType == SqlTokenType.NotGreaterThan)
                                                            || (TokenType == SqlTokenType.NotLessThan && t.TokenType == SqlTokenType.GreaterOrEqual)
                                                            || (TokenType == SqlTokenType.GreaterOrEqual && t.TokenType == SqlTokenType.NotLessThan);

        public override string ToString() => SqlKeyword.ToString( TokenType );

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

    }

}
