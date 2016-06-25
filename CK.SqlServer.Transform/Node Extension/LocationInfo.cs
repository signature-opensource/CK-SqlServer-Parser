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
        string _whatDescription;

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
                _whatDescription = null;
            }
            else
            {
                NodeMatcher = null;
                PatternRange = null;
                IsNodeMatchPart = IsNodeMatchStatement =  IsNodeMatchRange = false;
                if( t.Value.StartsWith( "--" ) )
                {
                    string lineComment = t.Value.Substring( 2 ).Trim();
                    _whatDescription = $"line comment starting with '{lineComment}'";
                    TriviaMatcher = trivia => trivia.TokenType == SqlTokenType.LineComment && trivia.Text.TrimStart().StartsWith( lineComment );
                }
                else
                {
                    Debug.Assert( t.Value.StartsWith( "/*" ) && t.Value.EndsWith( "*/" ) );
                    string starComment = t.Value.Substring( 2, t.Value.Length - 4 ).Trim();
                    _whatDescription = $"comment containing '{starComment.Replace( '\r', '.' ).Replace( '\n', '.' )}'";
                    TriviaMatcher = trivia => trivia.TokenType == SqlTokenType.StarComment && trivia.Text.Contains( starComment );
                }
            }
            Card = new LocationCardinalityInfo( loc );
        }


        public LocationInfo( TriviaExtensionMatcher m )
        {
            Card = new LocationCardinalityInfo( single:true );
            TriviaMatcher = m.Match;
            NodeMatcher = null;
            PatternRange = null;
            IsNodeMatchPart = IsNodeMatchRange = IsNodeMatchStatement = false;
            _whatDescription = null;
        }

        public string WhatDescription
        {
            get
            {
                if( _whatDescription == null )
                {
                    if( TriviaMatcher != null )
                    {
                        _whatDescription = $" extension named '{((TriviaExtensionMatcher)TriviaMatcher.Target).ExtensionName}'";
                    }
                    else
                    {
                        _whatDescription = "structural part(s) like {";
                        if( IsNodeMatchStatement ) _whatDescription = "statement(s) like {";
                        if( IsNodeMatchRange ) _whatDescription = "token(s) range like {";
                        _whatDescription += PatternRange.ToStringCompact() + '}';
                    }
                }
                return _whatDescription;
            }
        }

        public override string ToString() => Card.ToString() + " " + WhatDescription;
    }
}
