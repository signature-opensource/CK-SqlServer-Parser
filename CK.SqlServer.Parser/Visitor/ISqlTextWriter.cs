#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
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

        /// <summary>
        /// Writes a trivia.
        /// </summary>
        /// <param name="t">The trivia to write.</param>
        void Write( SqlTrivia t );

        /// <summary>
        /// Writes piece of text like a token or a terminal.
        /// </summary>
        /// <param name="type">
        /// Type of the token. This is used to secure the separators between tokens (for instance
        /// a separator must appear between two identifIers).
        /// </param>
        /// <param name="text">Text to write.</param>
        /// <param name="whiteSpaceBefore">
        /// True to force at least one whitespace before, false to remove it, null to let it be what it is.
        /// This does not apply to all kind of writer (<see cref="SqlTextWriter.CreateDefault"/> ignores it for instance).
        /// </param>
        /// <param name="whiteSpaceAfter">
        /// True to force at least one whitespace after, false to remove it, null to let it be what it is.
        /// This does not apply to all kind of writer (<see cref="SqlTextWriter.CreateDefault"/> ignores it for instance).
        /// </param>
        void Write( SqlTokenType type, string text, bool? whiteSpaceBefore = null, bool? whiteSpaceAfter = null );

        /// <summary>
        /// Special write method to write a <see cref="SqlTokenIdentifier"/>.
        /// </summary>
        /// <param name="id">The identifier to write.</param>
        /// <param name="whiteSpaceBefore">
        /// True to force at least one whitespace before, false to remove it, null to let it be what it is.
        /// This does not apply to all kind of writer (<see cref="SqlTextWriter.CreateDefault"/> ignores it for instance).
        /// </param>
        /// <param name="whiteSpaceAfter">
        /// True to force at least one whitespace after, false to remove it, null to let it be what it is.
        /// This does not apply to all kind of writer (<see cref="SqlTextWriter.CreateDefault"/> ignores it for instance).
        /// </param>
        void Write( SqlTokenIdentifier id, bool? whiteSpaceBefore = null, bool? whiteSpaceAfter = null );
    }
}
