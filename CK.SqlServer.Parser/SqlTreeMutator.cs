using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Parser
{
    public class SqlTreeMutator
    {
        readonly List<ISqlNode> _path;

        public SqlTreeMutator()
        {
            _path = new List<ISqlNode>();
        }

        public ISqlNode Mutate( ISqlNode n )
        {
            return Mutate( _path, n, 0 );
        }

        protected virtual IReadOnlyList<ISqlNode> MutateChildren( IReadOnlyList<ISqlNode> parents, ISqlNode c )
        {
            _path.Add( c );
            List<ISqlNode> result = null;
            int j = 0;
            for( int i = 0; i < c.ChildrenNodes.Count; ++i )
            {
                var n = c.ChildrenNodes[i];
                var newN = Mutate( _path, n, i );
                if( n != newN )
                {
                    if( result == null ) result = new List<ISqlNode>( c.ChildrenNodes );
                    if( newN == null ) result.RemoveAt( j-- );
                    else result[j] = newN;
                }
                ++j;
            }
            _path.RemoveAt( _path.Count - 1 );
            return result ?? c.ChildrenNodes;
       }

        protected virtual ISqlNode Mutate( IReadOnlyList<ISqlNode> parents, ISqlNode n, int idx )
        {
            return n.SetChildrenNodes( MutateChildren( parents, n ) );
        }

    }

}

