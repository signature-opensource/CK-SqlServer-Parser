using CK.Core;
using CK.SqlServer.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Transform.Transformers
{
    public class AddColumn : SqlNodeLocationVisitor
    {
        readonly IEnumerable<SelectColumn> _columns;

        public AddColumn( IEnumerable<SelectColumn> columns )
        {
            if( columns == null ) throw new ArgumentNullException( nameof( columns ) );
            _columns = columns;
        }


        protected override ISqlNode Visit( SqlInsertStatement e )
        {
            return VisitContext.RangeFilterStatus.IsIncludedInFilteredRange()
                    ? e.AddColumns( _columns )
                    : base.Visit( e );
        }

        protected override ISqlNode Visit( SqlUpdateStatement e )
        {
            if( VisitContext.RangeFilterStatus.IsIncludedInFilteredRange() )
            {
                var missingName = _columns.FirstOrDefault( c => c.ColumnName == null );
                if( missingName == null ) return e.AddColumns( _columns );
                Monitor.Error( $"'add column' in update expects column name to be specified: column '{missingName.Definition}'." );
            }
            return base.Visit( e ); 
        }

        protected override ISqlNode Visit( SelectSpec e )
        {
            return VisitContext.RangeFilterStatus.IsIncludedInFilteredRange()
                    ? e.InsertColumns( e.Columns.Count, _columns )
                    : base.Visit( e );
        }

    }
}
