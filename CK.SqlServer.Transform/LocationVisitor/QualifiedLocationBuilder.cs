using CK.SqlServer.Parser;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Transform
{
    /// <summary>
    /// Creates current locations during traversal that have associated <see cref="SqlNodeLocation.Node"/>.
    /// </summary>
    class QualifiedLocationBuilder
    {
        struct PLoc
        {
            public readonly SqlNodeLocation Loc;
            public readonly ISqlNode Node;
            public readonly int Pos;
            public readonly int CurChildPos;

            public PLoc( SqlNodeLocation loc, ISqlNode n, int idx, int curChildPos )
            {
                Loc = loc;
                Node = n;
                Pos = idx;
                CurChildPos = curChildPos;
            }
        }
        LocationRoot _root;
        readonly List<PLoc> _path;

        public QualifiedLocationBuilder()
        {
            _path = new List<PLoc>();
        }

        /// <summary>
        /// Gets the current location root.
        /// </summary>
        public LocationRoot Root => _root;

        /// <summary>
        /// Gets the current visit depth.
        /// </summary>
        public int Depth => _path.Count - 1;

        /// <summary>
        /// Gets the current visit position.
        /// </summary>
        public int Position => _path[_path.Count - 1].CurChildPos;

        /// <summary>
        /// Resets the builder on a <see cref="Root"/>.
        /// </summary>
        /// <param name="root">The new root.</param>
        public void Reset( LocationRoot root )
        {
            Debug.Assert( root != null && root.Node != null );
            _root = root;
            _path.Clear();
        }

        /// <summary>
        /// Called before visiting a node.
        /// </summary>
        /// <param name="n">The node to be visited.</param>
        public void Enter( ISqlNode n )
        {
            if( n == _root.Node ) _path.Add( new PLoc( _root, n, 0, 0 ) );
            else
            {
                PLoc c = _path[_path.Count - 1];
                _path.Add( new PLoc( null, n, c.CurChildPos, c.CurChildPos ) );
            }
        }

        /// <summary>
        /// Called after having visited a node.
        /// </summary>
        /// <param name="n">The visited node.</param>
        public void Leave( ISqlNode n )
        {
            Debug.Assert( _path[_path.Count - 1].Node == n );
            int top = _path.Count;
            _path.RemoveAt( --top );
            if( n != _root.Node )
            {
                PLoc c = _path[--top];
                _path[top] = new PLoc( c.Loc, c.Node, c.Pos, c.CurChildPos + n.Width );
            }
        }

        /// <summary>
        /// Obtains the location of the currently visited node.
        /// When no nodes are beeing visited, <see cref="Root"/> is returned.
        /// </summary>
        /// <returns>A qualified location.</returns>
        public SqlNodeLocation GetCurrent()
        {
            SqlNodeLocation prev = _root;
            for( int i = 1; i < _path.Count; i++ )
            {
                PLoc c = _path[i];
                SqlNodeLocation loc = c.Loc;
                if( loc == null )
                {
                    loc = _root.EnsureLocation( prev, c.Node, c.Pos );
                    _path[i] = new PLoc( loc, c.Node, c.Pos, c.CurChildPos );
                }
                prev = loc;
            }
            return prev;
        }

    }

}
