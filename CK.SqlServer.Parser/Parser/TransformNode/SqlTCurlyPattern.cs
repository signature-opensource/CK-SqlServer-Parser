using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using CK.Core;
using System.Collections.Immutable;
using CK.Text;

namespace CK.SqlServer.Parser
{
    /// <summary>
    /// List of one or more <see cref="SqlToken"> enclosed in curly braces: {...}. 
    /// </summary>
    public sealed class SqlTCurlyPattern : ASqlNodeEnclosableList<SqlTokenTerminal,SqlToken,SqlTokenTerminal>, ISqlStructurallyEnclosed
    {
        AnalyzedPattern _analysedPattern;

        public SqlTCurlyPattern( SqlTokenTerminal opener, IEnumerable<SqlToken> items, SqlTokenTerminal closer )
            : base( 1, opener, items, closer )
        {
            if( opener.TokenType != SqlTokenType.OpenCurly ) throw new ArgumentException();
            if( closer.TokenType != SqlTokenType.CloseCurly ) throw new ArgumentException();
        }

        SqlTCurlyPattern( SqlTCurlyPattern o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( o, 1, leading, items, trailing )
        {
            if( items == null ) _analysedPattern = o._analysedPattern;
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlTCurlyPattern( this, leading, content, trailing );
        }

        /// <summary>
        /// Pattern token can be a simple <see cref="Token"/> that may be an <see cref="IsOptional"/>.
        /// When Token is null, it can be any one token (IsOptional is false) and any or no token at all when IsOptional is true.
        /// </summary>
        public struct PToken
        {
            public readonly SqlToken Token;
            public readonly bool IsOptional;
            public bool IsAnyToken => Token == null && !IsOptional;
            public bool IsFullyOptionalToken => Token == null && IsOptional;

            public PToken( SqlToken t, bool optional )
            {
                Token = t;
                IsOptional = optional;
            }
        }

        /// <summary>
        /// A <see cref="PToken"/> list starts with <see cref="SqlTokenType.TripleQuestionMark"/>, <see cref="SqlTokenType.QuadrupleQuestionMark"/>
        /// or <see cref="SqlTokenType.None"/>.
        /// </summary>
        public struct PTokenList
        {
            public readonly IReadOnlyList<PToken> PTokens;
            public readonly SqlTokenType Start;
            
            /// <summary>
            /// Gets the minimal number of tokens that this list can match: it is the total number of
            /// PToken less the number of <see cref="PToken.IsFullyOptionalToken"/>.
            /// </summary>
            public readonly int MinMatchLength;

            internal PTokenList( IReadOnlyList<PToken> p, SqlTokenType s, int minMatchLength )
            {
                Debug.Assert( s == SqlTokenType.None || s == SqlTokenType.TripleQuestionMark || s == SqlTokenType.QuadrupleQuestionMark );
                PTokens = p;
                Start = s;
                MinMatchLength = minMatchLength;
            }
        }

        /// <summary>
        /// Computed version of the content onf this list.
        /// </summary>
        public class AnalyzedPattern
        {
            /// <summary>
            /// Gets the list of <see cref="PToken"/>s sequence.
            /// </summary>
            public readonly IReadOnlyList<PTokenList> Patterns;

            /// <summary>
            /// Gets the index in <see cref="Patterns"/> that is the tail list (the first one 
            /// that starts with ????).
            /// </summary>
            public readonly int TailListIndex;

            /// <summary>
            /// Gets the number of trailing ??.
            /// </summary>
            public readonly int FullyOptionalTailCount;

            /// <summary>
            /// Gets the terminator pattern: either <see cref="SqlTokenType.None"/>, <see cref="SqlTokenType.TripleQuestionMark"/>
            /// or <see cref="SqlTokenType.QuadrupleQuestionMark"/>.
            /// </summary>
            public readonly SqlTokenType Terminator;

            internal AnalyzedPattern( IReadOnlyList<PTokenList> p, int tailListIndex, int fullyOptionalTailCount, SqlTokenType terminator )
            {
                Patterns = p;
                TailListIndex = tailListIndex;
                FullyOptionalTailCount = fullyOptionalTailCount;
                Terminator = terminator;
            }

        }

        /// <summary>
        /// Gets this pattern as a list of <see cref="PTokenList"/>.
        /// </summary>
        /// <returns></returns>
        public AnalyzedPattern Pattern
        {
            get
            {
                if( _analysedPattern == null )
                {
                    var result = new List<PTokenList>();
                    SqlTokenType start = SqlTokenType.None;
                    SqlTokenType terminator = SqlTokenType.None;
                    var e = this.GetEnumerator();
                    if( !e.MoveNext() ) return null;
                    SqlToken head, lookup;
                    lookup = e.Current;

                    int actualPTokenCount;
                    int tailListIndex = -1;
                    int minMatchLength = 0;
                    int fullyOptionalTailCount = 0;
                    var current = new List<PToken>();
                    for( ;;)
                    {
                        if( lookup == null ) break;
                        head = lookup;
                        lookup = e.MoveNext() ? e.Current : null;
                        if( current.Count == 0 )
                        {
                            Debug.Assert( start == SqlTokenType.None || start == SqlTokenType.TripleQuestionMark || start == SqlTokenType.QuadrupleQuestionMark );
                            if( head.TokenType == SqlTokenType.TripleQuestionMark )
                            {
                                if( start != SqlTokenType.QuadrupleQuestionMark ) start = SqlTokenType.TripleQuestionMark;
                                if( lookup == null ) terminator = start;
                                continue;
                            }
                            if( head.TokenType == SqlTokenType.QuadrupleQuestionMark )
                            {
                                start = SqlTokenType.QuadrupleQuestionMark;
                                if( lookup == null ) terminator = start;
                                continue;
                            }
                        }
                        else
                        {
                            if( head.TokenType == SqlTokenType.TripleQuestionMark || head.TokenType == SqlTokenType.QuadrupleQuestionMark )
                            {
                                Debug.Assert( current.Count > 0, "Duplicate ??? or ???? starting are removed above." );
                                actualPTokenCount = current.Count - fullyOptionalTailCount;
                                if( actualPTokenCount == 0 )
                                {
                                    Debug.Assert( minMatchLength == 0 );
                                    // Only ??
                                    // Avoids creating patterns full of ?? by transfering them to the start of the 
                                    // new pattern list.
                                    // If the current start was ???? it takes precedence.
                                    if( start != SqlTokenType.QuadrupleQuestionMark ) start = head.TokenType;
                                }
                                else
                                {
                                    if( start == SqlTokenType.QuadrupleQuestionMark )
                                    {
                                        if( tailListIndex >= 0 ) start = SqlTokenType.TripleQuestionMark;
                                        else tailListIndex = result.Count;
                                    }
                                    PToken[] t = new PToken[actualPTokenCount];
                                    current.CopyTo( t );
                                    result.Add( new PTokenList( t, start, minMatchLength ) );
                                    if( fullyOptionalTailCount == 0 ) current.Clear();
                                    else current.RemoveRange( 0, actualPTokenCount );
                                    start = head.TokenType;
                                    minMatchLength = 0;
                                }
                                if( lookup == null ) terminator = start;
                                continue;
                            }
                        }
                        if( head.TokenType == SqlTokenType.DoubleQuestionMark )
                        {
                            ++fullyOptionalTailCount;
                            current.Add( new PToken( null, true ) );
                            continue;
                        }
                        fullyOptionalTailCount = 0;
                        if( lookup != null
                            && lookup.TokenType == SqlTokenType.QuestionMark
                            && head.TrailingTrivias.IsEmpty
                            && lookup.LeadingTrivias.IsEmpty )
                        {
                            current.Add( new PToken( head, true ) );
                            lookup = e.MoveNext() ? e.Current : null;
                            continue;
                        }
                        ++minMatchLength;
                        current.Add( new PToken( head.TokenType != SqlTokenType.QuestionMark ? head : null, false ) );
                    }
                    actualPTokenCount = current.Count - fullyOptionalTailCount;
                    if( actualPTokenCount > 0 )
                    {
                        if( start == SqlTokenType.QuadrupleQuestionMark )
                        {
                            if( tailListIndex >= 0 ) start = SqlTokenType.TripleQuestionMark;
                            else tailListIndex = result.Count;
                        }
                        PToken[] t = new PToken[actualPTokenCount];
                        current.CopyTo( t );
                        result.Add( new PTokenList( t, start, minMatchLength ) );
                    }
                    else if( fullyOptionalTailCount > 0 )
                    {
                        Debug.Assert( terminator == SqlTokenType.None );
                        terminator = start;
                    }
                    _analysedPattern = new AnalyzedPattern( result, tailListIndex, fullyOptionalTailCount, terminator );
                }
                return _analysedPattern;
            }
        }

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

    }


}
