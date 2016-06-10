using CK.Core;
using CK.SqlServer.Parser;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Transform.Matcher
{
    class WindowToken : IDisposable
    {
        readonly FIFOBuffer<SqlToken> _tokens;
        readonly IEnumerator<SqlToken> _source;

        public WindowToken( int length, IEnumerator<SqlToken> tokens )
        {
            _tokens = new FIFOBuffer<SqlToken>( length );
            _source = tokens;
            int fill = length;
            while( --fill >= 0 && _source.MoveNext() ) _tokens.Push( _source.Current );
        }

        public int Count => _tokens.Count;

        public SqlToken this[int i] => _tokens[i];

        public int Shift( int n )
        {
            Debug.Assert( n > 0 );
            while( --n >= 0 && _source.MoveNext() )
            {
                _tokens.Push( _source.Current );
            }
            return _tokens.Count;
        }

        public int HeadMatch( IReadOnlyList<SqlToken> pattern )
        {
            var e = _tokens.GetEnumerator();
            return e.MoveNext() ? RawHeadMatch( e, _tokens.Count, pattern ) : 0;
        }

        /// <summary>
        /// Matches the pattern on the current token of the emnumerator. 
        /// On success, returns the positive number of tokens that have matched.
        /// A zero or negative returned value indicates a failed matched.
        /// </summary>
        /// <param name="tokens">An enumerator withe a ready Current.</param>
        /// <param name="maxTokenCount">The maximum number of tokens to consider in the enumerator.</param>
        /// <param name="pattern">The pattern to match.</param>
        /// <returns>Positive or negative number of tokens that have been forwarded.</returns>
        public static int RawHeadMatch( IEnumerator<SqlToken> tokens, int maxTokenCount, IReadOnlyList<SqlToken> pattern )
        {
            int maxPatternIndex = pattern.Count;
            if( maxPatternIndex == 0 ) return 0;
            int width = 0;
            for( ;;)
            {
                var pat = pattern[width];
                if( pat.TokenType == SqlTokenType.QuestionMark
                    || tokens.Current.TokenEquals( pat ) )
                {
                    ++width;
                    if( --maxPatternIndex == 0 ) return width;
                    if( width == maxTokenCount || !tokens.MoveNext() ) return -width;
                }
                else return -width;
            }
        }

        void IDisposable.Dispose()
        {
            _source.Dispose();
        }
    }
}
