using CK.SqlServer.Parser;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Parser
{
    /// <summary>
    /// Captures a normalized cardinality of <see cref="SqlTMultiLocationFinder"/> (all, each)
    /// and <see cref="SqlTOneLocationFinder"/> (first, last, single).
    /// </summary>
    public readonly struct LocationCardinalityInfo
    {
        /// <summary>
        /// Total number of matches expected.
        /// Unapplicable when 0.
        /// </summary>
        public readonly int ExpectedMatchCount;

        /// <summary>
        /// The match number to consider among the multiple matches.
        /// Actual match depends on <see cref="FromFirst"/>: "first +Offset" or "last -Offset" (when FromFirst is false).
        /// </summary>
        public readonly int Offset;

        /// <summary>
        /// States whether <see cref="Offset"/> is from the first match or must be found in reverse order.
        /// </summary>
        public readonly bool FromFirst;

        /// <summary>
        /// Gets whether all the matches are concerned. Always true if <see cref="Each"/> is true.
        /// </summary>
        public readonly bool All;

        /// <summary>
        /// Gets whether all the matches are concerned but in sequence: each match is an independent range
        /// instead of bein considered as a multi-parts range.
        /// </summary>
        public readonly bool Each;

        /// <summary>
        /// Initializes this location for a mono (first/last/single match).
        /// </summary>
        /// <param name="loc">The location node.</param>
        public LocationCardinalityInfo( SqlTOneLocationFinder loc )
        {
            FromFirst = All = Each = false;
            if( loc.FirstOrLastOrSingleT.TokenType == SqlTokenType.Single )
            {
                // "single" is the same as "first out of 1".
                ExpectedMatchCount = 1;
                FromFirst = true;
            }
            else
            {
                ExpectedMatchCount = loc.ExpectedMatchCount?.Value ?? 0;
                if( loc.FirstOrLastOrSingleT.TokenType == SqlTokenType.First )
                {
                    FromFirst = true;
                }
                // else it is SqlTokenType.Last: FromFirst = All = false;
            }
            Offset = loc.Offset?.Value ?? 0;
        }

        /// <summary>
        /// Initializes this location for a multi (<see cref="All"/> match).
        /// </summary>
        /// <param name="loc">The location node.</param>
        public LocationCardinalityInfo( SqlTMultiLocationFinder loc )
        {
            FromFirst = All = true;
            ExpectedMatchCount = loc.ExpectedMatchCount?.Value ?? 0;
            if( loc.AllOrEachT.TokenType == SqlTokenType.All )
            {
                Each = false;
            }
            else
            {
                Debug.Assert( loc.AllOrEachT.TokenType == SqlTokenType.Each );
                Each = true;
            }
            Offset = 0;
        }

        /// <summary>
        /// Initializes a "single" cardinality.
        /// </summary>
        /// <param name="single">Must be true.</param>
        public LocationCardinalityInfo( bool single )
        {
            if( !single ) throw new ArgumentOutOfRangeException( nameof( single ) );
            All = Each = false;
            ExpectedMatchCount = 1;
            FromFirst = true;
            Offset = 0;
        }

        /// <summary>
        /// Gets the formatted string (as it can be parsed).
        /// </summary>
        /// <returns>The readable (and parsable) string.</returns>
        public override string ToString()
        {
            if( Each )
            {
                return ExpectedMatchCount == 0 ? "each" : "each " + ExpectedMatchCount;
            }
            if( All )
            {
                return ExpectedMatchCount == 0 ? "all" : "all " + ExpectedMatchCount;
            }
            string s;
            if( FromFirst )
            {
                if( Offset == 0 && ExpectedMatchCount == 1 ) return "single";
                s = Offset == 0 ? "first" : "first+" + Offset;
                if( ExpectedMatchCount > 0 ) s += " out of " + ExpectedMatchCount;
                return s;
            }
            s = Offset == 0 ? "last" : "last-" + Offset;
            if( ExpectedMatchCount > 0 ) s += " out of " + ExpectedMatchCount;
            return s;
        }

    }

}
