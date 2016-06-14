using CK.SqlServer.Parser;
using CK.SqlServer.Transform.Transformers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Transform
{
    /// <summary>
    /// Captures operational information from a <see cref="SqlTLocationFinder"/>.
    /// Cardinality check is handled thanks to <see cref="LocationCardinalityInfo"/>.
    /// This handles the 5 kind of matches: part and statement, range match, trivia match 
    /// and fragment match.
    /// </summary>
    internal struct LocationInfo
    {
        public readonly LocationCardinalityInfo Card;
        public readonly Func<SqlTrivia, bool> TriviaMatcher;
        public readonly Func<ISqlNode, bool> NodeMatcher;
        public readonly IReadOnlyList<SqlToken> PatternRange;
        public readonly bool IsNodeMatchPart;
        public readonly bool IsNodeMatchStatement;
        public readonly bool IsNodeMatchRange;

        public LocationInfo( SqlTLocationFinder loc )
        {
            var t = loc.Pattern as ISqlHasStringValue;
            if( t == null )
            {
                TriviaMatcher = null;
                var nodePattern = (SqlTNodeSimplePattern)loc.Pattern;
                NodeMatcher = nodePattern.MatchPartOrStatement;
                PatternRange = nodePattern.Pattern;
                IsNodeMatchPart = nodePattern.IsMatchPart;
                IsNodeMatchStatement = nodePattern.IsMatchStatement;
                IsNodeMatchRange = nodePattern.IsMatchRange;
            }
            else
            {
                NodeMatcher = null;
                PatternRange = null;
                IsNodeMatchPart = IsNodeMatchStatement =  IsNodeMatchRange = false;
                if( t.Value.StartsWith( "--" ) )
                {
                    string lineComment = t.Value.Substring( 2 ).Trim();
                    TriviaMatcher = trivia => trivia.TokenType == SqlTokenType.LineComment && trivia.Text.TrimStart().StartsWith( lineComment );
                }
                else
                {
                    Debug.Assert( t.Value.StartsWith( "/*" ) && t.Value.EndsWith( "*/" ) );
                    string starComment = t.Value.Substring( 2, t.Value.Length - 4 ).Trim();
                    TriviaMatcher = trivia => trivia.TokenType == SqlTokenType.StarComment && trivia.Text.Contains( starComment );
                }
            }
            Card = new LocationCardinalityInfo( loc );
        }

        public LocationInfo( TriviaExtensionMatcher m )
        {
            Card = new LocationCardinalityInfo( true );
            TriviaMatcher = m.Match;
            NodeMatcher = null;
            PatternRange = null;
            IsNodeMatchPart = IsNodeMatchRange = IsNodeMatchStatement = false;
        }
    }
}
