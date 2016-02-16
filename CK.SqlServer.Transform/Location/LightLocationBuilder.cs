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
    /// Creates current locations during traversal that do not have <see cref="SqlNodeLocation.Node"/>.
    /// </summary>
    class LightLocationBuilder : ISqlNodeLocationBuilder
    {
        LocationRoot _root;
        SqlNodeLocation _current;
        SqlNodeLocation _currentQ;
        int _curPos;
        int _depth;

        public void Reset( LocationRoot root )
        {
            Debug.Assert( root != null && root.Node != null );
            _root = root;
            _curPos = 0;
            _depth = 0;
        }

        public LocationRoot Root => _root;

        public int Depth => _depth;

        public int Position => _curPos;

        public void Enter( ISqlNode n ) => ++_depth;

        public void Leave( ISqlNode n )
        {
            --_depth;
            if( n is SqlToken )
            {
                ++_curPos;
                _currentQ = _current = null;
            }
        }

        public SqlNodeLocation GetCurrent( ISqlNode current, bool qualifiedLocation )
        {
            if( qualifiedLocation )
            {
                return _currentQ ?? (_currentQ = _current = _depth == 0 ? _root : _root.GetQualifiedLocation( _curPos, current ));
            }
            return _current ?? _currentQ ?? (_current = _depth == 0 || _curPos == 0 ? _root : new SqlNodeLocation( _root, null, _curPos ));
        }

    }
}
