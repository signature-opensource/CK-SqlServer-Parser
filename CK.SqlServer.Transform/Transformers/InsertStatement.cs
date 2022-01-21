using CK.Core;
using CK.SqlServer.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Transform.Transformers
{
    /// <summary>
    /// Transformer that inserts a <see cref="SqlStatement"/> in a <see cref="SqlStatementList"/>.
    /// </summary>
    public class InsertStatement : SqlNodeLocationVisitor
    {
        readonly SqlNodeLocation _loc;
        readonly ISqlStatement _statement;
        readonly SqlNodeLocation _fatherLoc;

        /// <summary>
        /// Initializes a new <see cref="InsertStatement"/>.
        /// </summary>
        /// <param name="monitor">The monitor to use.</param>
        /// <param name="loc">The location where the statement must be inserted. Must be in a <see cref="SqlStatementList"/>.</param>
        /// <param name="statement">The statement to insert.</param>
        public InsertStatement( IActivityMonitor monitor, SqlNodeLocation loc, ISqlStatement statement )
            : base( monitor )
        {
            Throw.CheckNotNullArgument( loc );
            Throw.CheckNotNullArgument( statement );
            _loc = loc.ToFullLocation();
            _fatherLoc = _loc.ReversePath.FirstOrDefault( l => l.Node is SqlStatementList );
            Throw.CheckNotNullArgument( "Location is not in a SqlStatementList.", _fatherLoc );
            _statement = statement;
        }

        /// <summary>
        /// Inserts the statement if it's in the location provided and stops the visit.
        /// </summary>
        /// <param name="e">The candidate statement list.</param>
        /// <returns>The resulting node.</returns>
        protected override ISqlNode Visit( SqlStatementList e )
        {
            if( e == _fatherLoc.Node )
            {
                int delta = _loc.Position - _fatherLoc.Position;
                int idx = e.LocateDirectChildIndex( ref delta );
                StopVisit();
                return e.StuffRawContent( idx, 0, _statement );
            }
            return base.Visit( e );
        }

    }
}
