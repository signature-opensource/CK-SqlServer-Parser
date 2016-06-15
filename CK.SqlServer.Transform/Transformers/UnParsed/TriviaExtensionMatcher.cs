using CK.Core;
using CK.SqlServer.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CK.SqlServer.Transform.Transformers
{
    /// <summary>
    /// Handles trivia extension match in a way that enables reuse of <see cref="LocationInserter"/>
    /// algorithm and code.
    /// </summary>
    class TriviaExtensionMatcher
    {
        static readonly Regex _rExt = new Regex( @"^</?(?<1>(\w|\.|-)+)(?<2>\s+reverse(\s*=\s*(""|')?(true|1)(""|')?)?)?\s*/?>", RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant );

        readonly IActivityMonitor _monitor;
        readonly string _extensionName;
        readonly string _injectedCode;
        bool _foundInsertPoint;
        bool _foundOpening;
        bool _foundOpeningIsAutoClosing;
        bool _foundOpeningIsReverse;

        public TriviaExtensionMatcher( IActivityMonitor monitor, string extensionName, string injectedCode )
        {
            _monitor = monitor;
            _extensionName = extensionName;
            _injectedCode = injectedCode.TrimEnd() + Environment.NewLine;
        }

        /// <summary>
        /// Gets the text to insert before the match. Can be null.
        /// </summary>
        public string TextBefore { get; private set; }

        /// <summary>
        /// Gets the text to insert after the match. Can be null.
        /// </summary>
        public string TextAfter { get; private set; }

        /// <summary>
        /// Gets the 3 trivias that must replace the matched one.
        /// </summary>
        public SqlTrivia[] TextReplace { get; private set; }

        public bool Match( SqlTrivia t )
        {
            if( t.TokenType != SqlTokenType.LineComment ) return false;
            Match m = _rExt.Match( t.Text );
            if( !m.Success ) return false;
            string name = m.Groups[1].Value;
            if( name != _extensionName ) return false;
            bool isClosing = t.Text[1] == '/';
            bool isAutoClosing = m.Value[m.Value.Length - 2] == '/';
            bool isReverse = m.Groups[2].Value.Length > 0;
            if( isClosing && isAutoClosing ) _monitor.Error().Send( $"Invalid extension tag: '{t.Text}': can not start with '</' and end with '/>'." );
            else if( isClosing && isReverse ) _monitor.Error().Send( $"Invalid extension tag: '{t.Text}': reverse must be on the opening tag." );
            else
            {
                if( isClosing )
                {
                    if( !_foundOpening ) _monitor.Error().Send( $"Closing '{t.Text}' has no opening." );
                    else if( _foundInsertPoint )
                    {
                        if( _foundOpeningIsAutoClosing )
                        {
                            _monitor.Error().Send( $"Unexpected closing of extension tag: '{t.Text}': opening tag is auto closed (ends with '/>')." );
                        }
                        else if( !_foundOpeningIsReverse )
                        {
                            _monitor.Error().Send( $"Unexpected closing of extension tag: '{t.Text}': it is already closed." );
                        }
                        // => This is the closing tag of a reverse extension.
                    }
                    else
                    {
                        TextBefore = _injectedCode;
                        return _foundInsertPoint = true;
                    }
                }
                else // Opening tag.
                {
                    if( _foundOpening ) _monitor.Error().Send( $"Duplicate opening of extension tag: '{name}'." );
                    else
                    {
                        _foundOpening = true;
                        _foundOpeningIsAutoClosing = isAutoClosing;
                        _foundOpeningIsReverse = isReverse;
                        if( isAutoClosing )
                        {
                            TextReplace = new[]
                            {
                            new SqlTrivia(SqlTokenType.LineComment, m.Value.Remove(m.Value.Length - 2, 1 )),
                            new SqlTrivia(SqlTokenType.None, _injectedCode ),
                            new SqlTrivia( SqlTokenType.LineComment, "</" + _extensionName + ">" )
                        };
                            return _foundInsertPoint = true;
                        }
                        else
                        {
                            if( isReverse )
                            {
                                TextAfter = _injectedCode;
                                return _foundInsertPoint = true;
                            }
                        }
                    }
                }
            }
            return false;
        }
    }
}
