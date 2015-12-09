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
        readonly List<SqlNode> _path;

        public SqlTreeMutator()
        {
            _path = new List<SqlNode>();
        }

        public SqlNode Mutate( SqlNode n )
        {
            return Mutate( _path, n, 0 );
        }

        protected virtual IReadOnlyList<SqlNode> MutateChildren( IReadOnlyList<SqlNode> parents, SqlNode c )
        {
            _path.Add( c );
            List<SqlNode> result = null;
            int j = 0;
            for( int i = 0; i < c.ChildrenNodes.Count; ++i )
            {
                var n = c.ChildrenNodes[i];
                var newN = Mutate( _path, n, i );
                if( n != newN )
                {
                    if( result == null ) result = new List<SqlNode>( c.ChildrenNodes );
                    if( newN == null ) result.RemoveAt( j-- );
                    else result[j] = newN;
                }
                ++j;
            }
            _path.RemoveAt( _path.Count - 1 );
            return result ?? c.ChildrenNodes;
       }

        protected virtual SqlNode Mutate( IReadOnlyList<SqlNode> parents, SqlNode n, int idx )
        {
            return n.SetChildrenNodes( MutateChildren( parents, n ) );
        }

    }

}

