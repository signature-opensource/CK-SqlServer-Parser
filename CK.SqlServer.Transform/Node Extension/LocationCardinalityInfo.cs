using CK.SqlServer.Parser;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Transform
{
    public struct LocationCardinalityInfo
    {
        public readonly int ExpectedMatchCount;
        public readonly int Offset;
        public readonly bool FromFirst;
        public readonly bool All;

        public LocationCardinalityInfo( SqlTLocationFinder loc )
        {
            FromFirst = All = false;
            if( loc.FirstOrLastOrSingleOrAllT.TokenType == SqlTokenType.Single )
            {
                // "single" is the same as "first out of 1".
                ExpectedMatchCount = 1;
                FromFirst = true;
            }
            else
            {
                ExpectedMatchCount = loc.ExpectedMatchCount?.Value ?? 0;
                if( loc.FirstOrLastOrSingleOrAllT.TokenType == SqlTokenType.All )
                {
                    FromFirst = All = true;
                }
                else if( loc.FirstOrLastOrSingleOrAllT.TokenType == SqlTokenType.First )
                {
                    FromFirst = true;
                }
                // else it is SqlTokenType.Last: FromFirst = All = false;
            }
            Offset = loc.Offset?.Value ?? 0;
        }

        public LocationCardinalityInfo( bool single )
        {
            All = false;
            ExpectedMatchCount = 1;
            FromFirst = true;
            Offset = 0;
        }

        public override string ToString()
        {
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
