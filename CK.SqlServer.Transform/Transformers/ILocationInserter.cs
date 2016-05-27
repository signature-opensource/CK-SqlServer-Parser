using CK.Core;
using CK.SqlServer.Parser;

namespace CK.SqlServer.Transform.Transformers
{
    interface ILocationInserter
    {
        bool CanStop { get; }
        bool Found { get; }
        bool RequiresConclude { get; }

        bool AddCandidate( IActivityMonitor monitor, int position, ISqlNode n );
        ISqlNode GetResult( ISqlNode e );
    }
}