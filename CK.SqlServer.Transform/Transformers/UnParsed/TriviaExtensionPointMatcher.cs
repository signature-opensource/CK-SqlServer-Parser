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
    /// Handles trivia extension point match in a way that enables reuse of <see cref="LocationInserter"/>
    /// algorithm and code.
    /// </summary>
    class TriviaExtensionPointMatcher
    {
        static readonly Regex _rExt = new Regex( @"^</?(?<1>(\w|\.|-)+)(?<2>\s+rever(t|se))?\s*/?>", RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant );

        readonly IActivityMonitor _monitor;
        readonly string _injectedCode;
        bool _foundInsertPoint;
        bool _foundOpening;
        bool _foundOpeningIsAutoClosing;
        bool _foundOpeningIsRevert;

        public TriviaExtensionPointMatcher( IActivityMonitor monitor, string extensionName, string injectedCode )
        {
            _monitor = monitor;
            ExtensionName = extensionName;
            _injectedCode = injectedCode.TrimEnd() + Environment.NewLine;
        }

        /// <summary>
        /// Gets the name of the extension point (for instance "PreDestroy", "OnNameChanged", etc.).
        /// </summary>
        public string ExtensionName { get; }

        /// <summary>
        /// Gets the text to insert before the match.
        /// Can be null.
        /// </summary>
        public string TextBefore { get; private set; }

        /// <summary>
        /// Gets the text to insert after the match.
        /// Can be null.
        /// </summary>
        public string TextAfter { get; private set; }

        /// <summary>
        /// Gets the 3 trivias that must replace the matched one.
        /// Can be null.
        /// </summary>
        public SqlTrivia[] TextReplace { get; private set; }

        /// <summary>
        /// Tests whether the given trivia matches. This is called multiple times and the internal state is updated.
        /// On final call and success, true is returned and <see cref="TextBefore"/>, <see cref="TextAfter"/>
        /// and <see cref="TextReplace"/> are updated to reflect the action that should be applied.
        /// </summary>
        /// <param name="t">The trivia to process.</param>
        /// <returns>True on eventual success, false otherwise.</returns>
        public bool Match( SqlTrivia t )
        {
            if( t.TokenType != SqlTokenType.LineComment ) return false;
            Match m = _rExt.Match( t.Text );
            if( !m.Success ) return false;
            string name = m.Groups[1].Value;
            if( name != ExtensionName ) return false;
            bool isClosing = t.Text[1] == '/';
            bool isAutoClosing = m.Value[m.Value.Length - 2] == '/';
            bool isRevert = m.Groups[2].Value.Length > 0;
            if( isClosing && isAutoClosing ) _monitor.Error( $"Invalid extension tag: '{t.Text}': can not start with '</' and end with '/>'." );
            else if( isClosing && isRevert ) _monitor.Error( $"Invalid extension tag: '{t.Text}': revert must be on the opening tag." );
            else
            {
                if( isClosing )
                {
                    if( !_foundOpening ) _monitor.Error( $"Closing '{t.Text}' has no opening." );
                    else if( _foundInsertPoint )
                    {
                        if( _foundOpeningIsAutoClosing )
                        {
                            _monitor.Error( $"Unexpected closing of extension tag: '{t.Text}': opening tag is auto closed (ends with '/>')." );
                        }
                        else if( !_foundOpeningIsRevert )
                        {
                            _monitor.Error( $"Unexpected closing of extension tag: '{t.Text}': it is already closed." );
                        }
                        // => This is the closing tag of a reverted extension.
                    }
                    else
                    {
                        TextBefore = _injectedCode;
                        return _foundInsertPoint = true;
                    }
                }
                else // Opening tag.
                {
                    if( _foundOpening ) _monitor.Error( $"Duplicate opening of extension tag: '{name}'." );
                    else
                    {
                        _foundOpening = true;
                        _foundOpeningIsAutoClosing = isAutoClosing;
                        _foundOpeningIsRevert = isRevert;
                        if( isAutoClosing )
                        {
                            TextReplace = new[]
                            {
                            new SqlTrivia(SqlTokenType.LineComment, m.Value.Remove(m.Value.Length - 2, 1 )),
                            new SqlTrivia(SqlTokenType.None, _injectedCode ),
                            new SqlTrivia( SqlTokenType.LineComment, "</" + ExtensionName + ">" )
                        };
                            return _foundInsertPoint = true;
                        }
                        else
                        {
                            if( isRevert )
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
