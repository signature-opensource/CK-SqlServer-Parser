using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace CK.SqlServer.Parser
{
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

    public static class SqlTLocationFinderExtension
    {
        /// <summary>
        /// Create a trivia matcher from the <see cref="Pattern"/> if it is a <see cref="ISqlHasStringValue"/>.
        /// </summary>
        /// <param name="matcher">The predicate or null if Pattern is not a string value.</param>
        /// <param name="description">The description of the predicate or null if Pattern is not a string value.</param>
        /// <returns>True is Pattern is a string value, false if it is a <see cref="SqlTNodeSimplePattern"/>.</returns>
        public static bool CreateTriviaMatcher( this ISqlTLocationFinder @this, out Func<SqlTrivia, bool> matcher, out string description )
        {
            if( @this.Pattern is ISqlHasStringValue textInsideComment )
            {
                if( textInsideComment.Value.StartsWith( "--" ) )
                {
                    string lineComment = textInsideComment.Value.Substring( 2 ).Trim();
                    description = $"line comment starting with '{lineComment}'";
                    matcher = trivia => trivia.TokenType == SqlTokenType.LineComment && trivia.Text.TrimStart().StartsWith( lineComment );
                }
                else
                {
                    Debug.Assert( textInsideComment.Value.StartsWith( "/*" ) && textInsideComment.Value.EndsWith( "*/" ) );
                    string starComment = textInsideComment.Value.Substring( 2, textInsideComment.Value.Length - 4 ).Trim();
                    description = $"comment containing '{starComment.Replace( '\r', '.' ).Replace( '\n', '.' )}'";
                    matcher = trivia => trivia.TokenType == SqlTokenType.StarComment && trivia.Text.Contains( starComment );
                }
                return true;
            }
            matcher = null;
            description = null;
            return false;
        }
    }
}
