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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public int MatchHere( SqlTCurlyPattern.AnalyzedPattern pattern )
        {
            return -1;
        }

        void IDisposable.Dispose()
        {
            _source.Dispose();
        }
    }
}
