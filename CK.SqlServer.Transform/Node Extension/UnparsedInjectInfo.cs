using CK.SqlServer.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Transform
{
    internal class UnparsedInjectInfo
    {
        public readonly string TextBefore;
        public readonly string TextAfter;
        public readonly ISqlNode Node;
        public readonly LocationInfo Location;
        public readonly bool ClearStarComments;

        public UnparsedInjectInfo( SqlTInject ins )
        {
            Node = ins;
            TextBefore = ins.TextBefore;
            TextAfter = ins.TextAfter;
            Location = ins.Location.GetFinderInfo( false );
        }

        public UnparsedInjectInfo( SqlTReplace rep )
        {
            Node = rep;
            TextBefore = rep.Content.Value + "/*" + SqlTokenizer.CommentPrefixToSkip;
            TextAfter = "*/";
            Location = rep.Location.GetFinderInfo( false );
            ClearStarComments = true;
        }
    }
}
