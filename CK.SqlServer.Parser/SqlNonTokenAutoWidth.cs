using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Parser
{
    /// <summary>
    /// Supports <see cref="Width"/> automatic computation.
    /// </summary>
    public abstract class SqlNonTokenAutoWidth : SqlNonToken
    {
        int _width;

        protected SqlNonTokenAutoWidth( ImmutableList<SqlTrivia> leading = null, ImmutableList<SqlTrivia> trailing = null )
            : base( leading, trailing )
        {
            _width = -1;
        }

        public override sealed int Width => _width == -1 ? (_width = ChildrenNodes.Select( c => c.Width ).Sum()) : _width;

    }
}
