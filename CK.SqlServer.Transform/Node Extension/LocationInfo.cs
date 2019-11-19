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
    /// Captures operational information from a <see cref="ISqlTLocationFinder"/>.
    /// Cardinality check is handled thanks to <see cref="LocationCardinalityInfo"/>.
    /// This handles the 5 kind of matches: part and statement, range match, trivia match 
    /// and fragment match.
    /// </summary>
    internal readonly struct LocationInfo
    {
        public readonly LocationCardinalityInfo Card;
        public readonly Func<SqlTrivia, bool> TriviaMatcher;
        public readonly Func<ISqlNode, bool> NodeMatcher;
        public readonly IReadOnlyList<SqlToken> PatternRange;
        public readonly bool IsNodeMatchPart;
        public readonly bool IsNodeMatchStatement;
        public readonly bool IsNodeMatchRange;
        readonly string _commentDesc;

        public LocationInfo( ISqlTLocationFinder loc )
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
                _commentDesc = null;
            }
            else
            {
                NodeMatcher = null;
                PatternRange = null;
                IsNodeMatchPart = IsNodeMatchStatement =  IsNodeMatchRange = false;
                if( t.Value.StartsWith( "--" ) )
                {
                    string lineComment = t.Value.Substring( 2 ).Trim();
                    _commentDesc = $"line comment starting with '{lineComment}'";
                    TriviaMatcher = trivia => trivia.TokenType == SqlTokenType.LineComment && trivia.Text.TrimStart().StartsWith( lineComment );
                }
                else
                {
                    Debug.Assert( t.Value.StartsWith( "/*" ) && t.Value.EndsWith( "*/" ) );
                    string starComment = t.Value.Substring( 2, t.Value.Length - 4 ).Trim();
                    _commentDesc = $"comment containing '{starComment.Replace( '\r', '.' ).Replace( '\n', '.' )}'";
                    TriviaMatcher = trivia => trivia.TokenType == SqlTokenType.StarComment && trivia.Text.Contains( starComment );
                }
            }
            Card = loc.GetCardinality();
        }


        public LocationInfo( TriviaExtensionMatcher m )
        {
            Card = new LocationCardinalityInfo( single:true );
            TriviaMatcher = m.Match;
            _commentDesc = null;
            NodeMatcher = null;
            PatternRange = null;
            IsNodeMatchPart = IsNodeMatchRange = IsNodeMatchStatement = false;
        }

        public string GetDescription()
        {
            if( _commentDesc != null ) return _commentDesc;
            // This is no more cached (readonly struct) since this is used only
            // for error details (and in debug by this ToString).
            if( TriviaMatcher != null )
            {
                return $" extension named '{((TriviaExtensionMatcher)TriviaMatcher.Target).ExtensionName}'";
            }
            var desc = "structural part(s) like {";
            if( IsNodeMatchStatement ) desc = "statement(s) like {";
            if( IsNodeMatchRange ) desc = "token(s) range like {";
            desc += PatternRange.ToStringCompact() + '}';
            return desc;
        }

        public override string ToString() => Card.ToString() + " " + GetDescription();
    }
}
