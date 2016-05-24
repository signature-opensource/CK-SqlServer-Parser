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
    /// Specific <see cref="SqlTokenTerminal"/> for <see cref="SqlTokenType.QuestionMark"/>, <see cref="SqlTokenType.DoubleQuestionMark"/> 
    /// and <see cref="SqlTokenType.TripleQuestionMark"/>.
    /// </summary>
    public sealed class SqlTokenQuestionMarks : SqlTokenTerminal
    {
        public SqlTokenQuestionMarks( SqlTokenType tokenType, ImmutableList<SqlTrivia> leadingTrivia = null, ImmutableList<SqlTrivia> trailingTrivia = null )
            : base( tokenType, leadingTrivia, trailingTrivia )
        {
            if( tokenType != SqlTokenType.QuestionMark 
                && tokenType != SqlTokenType.DoubleQuestionMark 
                && tokenType != SqlTokenType.TripleQuestionMark ) throw new ArgumentException();
        }

        public override void WriteWithoutTrivias( ISqlTextWriter w )
        {
            Debug.Assert( SqlKeyword.ToString( SqlTokenType.QuestionMark ) == "?" );
            Debug.Assert( SqlKeyword.ToString( SqlTokenType.DoubleQuestionMark ) == "??" );
            Debug.Assert( SqlKeyword.ToString( SqlTokenType.TripleQuestionMark ) == "???" );
            w.Write( TokenType, ToString(), whiteSpaceBefore: false, whiteSpaceAfter: false );
        }

        public override string ToString() => TokenType == SqlTokenType.QuestionMark 
                                                ? "?" 
                                                : (TokenType == SqlTokenType.DoubleQuestionMark 
                                                    ? "??" 
                                                    : "???");

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlTokenMultiDots( TokenType, leading, trailing );
        }
    }

}
