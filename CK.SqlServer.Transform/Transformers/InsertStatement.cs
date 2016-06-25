using CK.Core;
using CK.SqlServer.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Transform.Transformers
{
    public class InsertStatement : SqlNodeLocationVisitor
    {
        readonly SqlNodeLocation _loc;
        readonly ISqlStatement _statement;
        readonly SqlNodeLocation _fatherLoc;

        public InsertStatement( SqlNodeLocation loc, ISqlStatement statement )
        {
            if( loc == null ) throw new ArgumentNullException( nameof(loc) );
            if( statement == null ) throw new ArgumentNullException( nameof(statement) );
            _loc = loc.ToFullLocation();
            _fatherLoc = _loc.ReversePath.FirstOrDefault( l => l.Node is SqlStatementList );
            if( _fatherLoc == null ) throw new ArgumentNullException( nameof(loc), "Location is not in a SqlStatementList." );
            _statement = statement;
        }

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
