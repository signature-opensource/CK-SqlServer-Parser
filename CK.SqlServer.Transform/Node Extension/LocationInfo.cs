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
        /// <summary>
        /// The cardinality specification.
        /// </summary>
        public readonly LocationCardinalityInfo Card;

        /// <summary>
        /// The trivia matcher is not null if and only if <see cref="NodeMatcher"/> is null.
        /// </summary>
        public readonly Func<SqlTrivia, bool> TriviaMatcher;

        /// <summary>
        /// Node matcher is not null if and only if <see cref="TriviaMatcher"/> is null.
        /// </summary>
        public readonly Func<ISqlNode, bool> NodeMatcher;

        /// <summary>
        /// Gets the <see cref="SqlTCurlyPattern"/>. Null when <see cref="TriviaMatcher"/> is not null.
        /// </summary>
        public readonly IReadOnlyList<SqlToken> PatternRange;

        /// <summary>
        /// Whether basic <see cref="PatternRange"/> must be matched. See <see cref="SqlTNodeSimplePattern.IsMatchRange"/>.
        /// </summary>
        public readonly bool IsNodeMatchRange;

        /// <summary>
        /// Whether <see cref="NodeMatcher"/> must match a structural part. See <see cref="SqlTNodeSimplePattern.IsMatchPart"/>.
        /// </summary>
        public readonly bool IsNodeMatchPart;

        /// <summary>
        /// Whether <see cref="NodeMatcher"/> must match a statement. See <see cref="SqlTNodeSimplePattern.IsMatchStatement"/>.
        /// </summary>
        public readonly bool IsNodeMatchStatement;

        readonly string _commentDesc;

        public LocationInfo( ISqlTLocationFinder loc )
        {
            if( loc.CreateTriviaMatcher( out TriviaMatcher, out _commentDesc ) )
            {
                NodeMatcher = null;
                PatternRange = null;
                IsNodeMatchPart = IsNodeMatchStatement = IsNodeMatchRange = false;
            }
            else
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
            Card = loc.GetCardinality();
        }

        public LocationInfo( TriviaExtensionPointMatcher m )
        {
            Card = new LocationCardinalityInfo( single:true );
            TriviaMatcher = m.Match;
            _commentDesc = null;
            NodeMatcher = null;
            PatternRange = null;
            IsNodeMatchPart = IsNodeMatchRange = IsNodeMatchStatement = false;
        }

        /// <summary>
        /// Creates a <see cref="SqlNodeScopePatternRange"/> (if <see cref="IsNodeMatchRange"/> is true) or
        /// a <see cref="SqlNodeScopeDepthPredicate"/> bound to the <see cref="NodeMatcher"/>.
        /// This MUST not be called when <see cref="TriviaMatcher"/> is not null: matching a trivia requires
        /// more information since a Trivia is between nodes (is not a node). Typically one need to know
        /// if the node "before" or "after" must belong to the scope.
        /// </summary>
        /// <returns>A scope builder.</returns>
        public SqlNodeScopeBuilder CreateScopeBuilder() => IsNodeMatchRange
                                                            ? (SqlNodeScopeBuilder)new SqlNodeScopePatternRange( PatternRange )
                                                            : (NodeMatcher != null
                                                                ? new SqlNodeScopeDepthPredicate( NodeMatcher, IsNodeMatchPart )
                                                                : throw new InvalidOperationException() );


        public string GetDescription()
        {
            if( _commentDesc != null ) return _commentDesc;
            // This is no more cached (readonly struct) since this is used only
            // for error details (and in debug by this ToString).
            if( TriviaMatcher != null )
            {
                return $" extension point '{((TriviaExtensionPointMatcher)TriviaMatcher.Target).ExtensionName}'";
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
