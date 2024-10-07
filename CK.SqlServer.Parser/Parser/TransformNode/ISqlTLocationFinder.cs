using CK.Core;
using System;

namespace CK.SqlServer.Parser;

/// <summary>
/// Generalizes <see cref="SqlTOneLocationFinder"/> and <see cref="SqlTMultiLocationFinder"/>.
/// </summary>
public interface ISqlTLocationFinder : ISqlNode
{
    /// <summary>
    /// Gets a <see cref="ISqlHasStringValue"/> or a <see cref="SqlTNodeSimplePattern"/>.
    /// </summary>
    ISqlNode Pattern { get; }

    /// <summary>
    /// Gets the normalized cardinality.
    /// </summary>
    LocationCardinalityInfo GetCardinality();
}

/// <summary>
/// Extends <see cref="ISqlHasStringValue"/>.
/// </summary>
public static class SqlTLocationFinderExtension
{
    /// <summary>
    /// Create a comment trivia matcher from a <see cref="ISqlHasStringValue"/> that MUST be a comment: it must start
    /// with "--" or start with "/*" and end with "*/".
    /// </summary>
    /// <returns>A matcher function and a description of the match.</returns>
    public static (Func<SqlTrivia, bool> Matcher, string Description) CreateCommentTriviaMatcher( this ISqlHasStringValue @this )
    {
        var v = @this.Value;
        if( v.StartsWith( "--" ) )
        {
            string lineComment = v.Substring( 2 ).Trim();
            return (trivia => trivia.TokenType == SqlTokenType.LineComment && trivia.Text.TrimStart().StartsWith( lineComment ), $"line comment starting with '{lineComment}'");
        }
        else
        {
            Throw.CheckArgument( $@"'{v}' must be a comment: it must start with ""--"" or start with ""/*"" and end with ""*/""", v.StartsWith( "/*" ) && v.EndsWith( "*/" ) );
            string starComment = v.Substring( 2, v.Length - 4 ).Trim();
            return (trivia => trivia.TokenType == SqlTokenType.StarComment && trivia.Text.Contains( starComment ), $"comment containing '{starComment.Replace( '\r', '.' ).Replace( '\n', '.' )}'");
        }
    }
}
