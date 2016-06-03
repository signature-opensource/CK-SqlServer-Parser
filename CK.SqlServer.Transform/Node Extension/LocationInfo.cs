using CK.SqlServer.Parser;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Transform
{
    internal struct LocationInfo
    {
        public readonly LocationCardinalityInfo Card;
        public readonly Func<SqlTrivia, bool> TriviaMatcher;
        public readonly Func<ISqlNode, bool> NodeMatcher;
        public readonly Func<ISqlNode, int> PatternRangeMatcher;
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
                PatternRangeMatcher = nodePattern.Pattern.Match;
                IsNodeMatchPart = nodePattern.IsMatchPart;
                IsNodeMatchStatement = nodePattern.IsMatchStatement;
                IsNodeMatchRange = nodePattern.IsMatchRange;
            }
            else
            {
                NodeMatcher = null;
                PatternRangeMatcher = null;
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
    }
}
