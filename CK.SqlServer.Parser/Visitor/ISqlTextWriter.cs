using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Parser
{
    public interface ISqlTextWriter
    {
        /// <summary>
        /// Gets whether star comments must be skipped.
        /// </summary>
        bool SkipStarComment { get; }

        /// <summary>
        /// Gets whether line comments must be skipped.
        /// </summary>
        bool SkipLineComment { get; }

        ///// <summary>
        ///// Gets the white space options.
        ///// </summary>
        //SqlTextWriter.WhiteSpaceOption WhiteSpace { get; }

        /// <summary>
        /// Writes a trivia.
        /// </summary>
        /// <param name="t">The trivia to write.</param>
        void Write( SqlTrivia t );

        /// <summary>
        /// Writes a sql piece of text like a token or a terminal.
        /// </summary>
        /// <param name="text">Text to write.</param>
        /// <param name="whiteSpaceBefore">
        /// True to force at least one whitespace before, false to remove it, null to let it be what it is.
        /// This does not apply to all kind of writer (<see cref="SqlTextWriter.CreateDefault"/> ignores it for instance).
        /// </param>
        /// <param name="canOmitWhiteSpaceAfter">
        /// True to force at least one whitespace after, false to remove it, null to let it be what it is.
        /// This does not apply to all kind of writer (<see cref="SqlTextWriter.CreateDefault"/> ignores it for instance).
        /// </param>
        void Write( string text, bool? whiteSpaceBefore = null, bool? whiteSpaceAfter = null );
    }
}
