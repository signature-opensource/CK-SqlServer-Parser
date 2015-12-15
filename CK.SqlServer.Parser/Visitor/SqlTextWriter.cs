using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Parser
{
    public class SqlTextWriter
    {
        /// <summary>
        /// Creates a default writer that writes everything.
        /// </summary>
        /// <param name="b">An optional existing String builder.</param>
        /// <returns>The text writer.</returns>
        public static ISqlTextWriter CreateDefault( StringBuilder b = null )
        {
            return new Full( b ?? new StringBuilder() );
        }

        /// <summary>
        /// Creates a writer on one line without any comments.
        /// </summary>
        /// <param name="b">An optional existing String builder.</param>
        /// <returns>The text writer.</returns>
        public static ISqlTextWriter CreateOneLineCompact( StringBuilder b = null )
        {
            return new OneLineCompact( b ?? new StringBuilder() );
        }

        class Full : ISqlTextWriter
        {
            readonly StringBuilder _b;

            public Full( StringBuilder b ) { _b = b; }

            public bool SkipLineComment => false;

            public bool SkipStarComment => false;

            public void Write( SqlTrivia t )
            {
                switch( t.TokenType )
                {
                    case SqlTokenType.LineComment: _b.Append( "--" ).Append( t.Text ).AppendLine(); break;
                    case SqlTokenType.StarComment: _b.Append( "/*" ).Append( t.Text ).Append( "*/" ); break;
                    default: _b.Append( t.Text ); break;
                }
            }

            public void Write( string text, bool? whiteSpaceBefore = null, bool? whiteSpaceAfter = null )
            {
                _b.Append( text );
            }

            public override string ToString()
            {
                return _b.ToString();
            }
        }

        class OneLineCompact : ISqlTextWriter
        {
            readonly StringBuilder _b;
            bool _ensureWhiteSpace;
            bool _allowWhiteSpaceAfter;

            public OneLineCompact( StringBuilder b ) { _b = b; }

            public bool SkipLineComment => true;

            public bool SkipStarComment => true;

            public void Write( SqlTrivia t )
            {
                _ensureWhiteSpace = _allowWhiteSpaceAfter;
            }

            public void Write( string text, bool? whiteSpaceBefore = null, bool? whiteSpaceAfter = null )
            {
                if( text.Length > 0 )
                {
                    if( (!whiteSpaceBefore.HasValue && _ensureWhiteSpace)
                        || (whiteSpaceBefore.HasValue && whiteSpaceBefore.Value) )
                    {
                        _b.Append( ' ' );
                    }
                    _b.Append( text );
                    if( whiteSpaceAfter.HasValue )
                    {
                        _allowWhiteSpaceAfter = _ensureWhiteSpace = whiteSpaceAfter.Value;
                    }
                    else
                    {
                        _allowWhiteSpaceAfter = true;
                        _ensureWhiteSpace = false;
                    }
                }
            }

            public override string ToString()
            {
                return _b.ToString();
            }
        }
    }
}