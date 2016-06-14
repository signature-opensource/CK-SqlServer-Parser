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

    }

}
