using CK.Core;
using CK.SqlServer.Parser;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Transform
{
    public static class SqlTNodeExtension
    {
        internal static LocationInfo GetFinderInfo( this SqlTLocationFinder @this ) => new LocationInfo( @this );

        public static bool MatchPartOrStatement( this SqlTNodeSimplePattern @this, ISqlNode n )
        {
            if( @this.IsMatchPart && !(n is ISqlStatementPart)
                || @this.IsMatchStatement && !(n is ISqlStatement) ) return false;
            return @this.Pattern.Match( n ) > 0;
        }

        public static int Match( this SqlTCurlyPattern @this, ISqlNode n )
        {
            var tokens = n.AllTokens.GetEnumerator();
            var patterns = @this.GetEnumerator();
            try
            {
                if( !tokens.MoveNext() || !patterns.MoveNext() ) return 0;

                int width = 0;
                for( ;;)
                {
                    if( patterns.Current.TokenType == SqlTokenType.QuestionMark
                        || tokens.Current.TokenEquals( patterns.Current ) )
                    {
                        ++width;
                        if( !patterns.MoveNext() ) return width;
                        if( !tokens.MoveNext() ) return -width;
                    }
                    else return -width;
                }
            }
            finally
            {
                tokens.Dispose();
                patterns.Dispose();
            }
        }

        struct PToken
        {
            public readonly SqlToken Token;
            public readonly bool IsOptional;

            public PToken( SqlToken t, bool optional )
            {
                Token = t;
                IsOptional = optional;
            }
        }

        struct PTokenList
        {
            public readonly IReadOnlyList<PToken> PTokens;
            public readonly SqlTokenType Start;

            public PTokenList( IReadOnlyList<PToken> p, SqlTokenType s )
            {
                Debug.Assert( s == SqlTokenType.None || s == SqlTokenType.TripleQuestionMark || s == SqlTokenType.QuadrupleQuestionMark );
                PTokens = p;
                Start = s;
            }
        }

        static List<PTokenList> AnalyzePattern( SqlTCurlyPattern pattern )
        {
            var result = new List<PTokenList>();
            SqlTokenType start = SqlTokenType.None;
            var e = pattern.GetEnumerator();
            if( !e.MoveNext() ) return null;
            SqlToken head, lookup;
            lookup = e.Current;

            var current = new List<PToken>();
            for(;;)
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
                        continue;
                    }
                    else if( head.TokenType == SqlTokenType.QuadrupleQuestionMark )
                    {
                        start = SqlTokenType.QuadrupleQuestionMark;
                        continue;
                    }
                    if( head.TokenType == SqlTokenType.DoubleQuestionMark )
                    {
                        if( start != SqlTokenType.None ) continue;
                        current.Add( new PToken( null, true ) );
                        continue;
                    }
                    if( lookup != null && (lookup.TokenType == SqlTokenType.QuestionMark) )
                    {
                        current.Add( new PToken( head, true ) );
                        lookup = e.MoveNext() ? e.Current : null;
                    }
                    else current.Add( new PToken( head, false ) );
                }
            }
            return result;
        }

        public static IEnumerable<SqlNodeLocationRange> ToRanges( this IEnumerable<SqlToken> @this, SqlTCurlyPattern pattern )
        {
            if( pattern == null ) throw new ArgumentNullException( nameof(pattern) );
            return null;
        }

        class WindowToken : IDisposable
        {
            readonly FIFOBuffer<SqlToken> _tokens;
            readonly IEnumerator<SqlToken> _source;

            public WindowToken( int length, IEnumerable<SqlToken> tokens )
            {
                _tokens = new FIFOBuffer<SqlToken>( length );
                _source = tokens.GetEnumerator();
            }

            public int Count => _tokens.Count;

            public SqlToken this[int i] => _tokens[i];

            public int Shift( int n )
            {
                Debug.Assert( n > 0 );
                while( _source.MoveNext() && --n >= 0 )
                {
                    _tokens.Push( _source.Current );
                }
                while( --n > 0 && _tokens.Count > 0 ) _tokens.PopLast();
                return _tokens.Count;
            }

            void IDisposable.Dispose()
            {
                _source.Dispose();
            }
        }
    }
}
