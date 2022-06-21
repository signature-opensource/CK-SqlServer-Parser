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
    /// Transformer that can add <see cref="SelectColumn"/> to <see cref="SqlUpdateStatement"/>, <see cref="SqlInsertStatement"/>
    /// or <see cref="SelectSpec"/>.
    /// </summary>
    public class AddColumn : SqlNodeLocationVisitor
    {
        readonly IEnumerable<SelectColumn> _columns;

        /// <summary>
        /// Initializes a new <see cref="AddColumn"/> transformer.
        /// </summary>
        /// <param name="monitor">The monitor to use.</param>
        /// <param name="columns">The columns to add.</param>
        public AddColumn( IActivityMonitor monitor, IEnumerable<SelectColumn> columns )
            : base( monitor )
        {
            Throw.CheckNotNullArgument( columns );
            _columns = columns;
        }


        /// <summary>
        /// Adds the columns to the <see cref="SqlInsertStatement.Columns"/> if it's included the selected range.
        /// Returns <paramref name="e"/> if outside the selected range.
        /// </summary>
        /// <param name="e">The select specification.</param>
        /// <returns>The resulting node.</returns>
        protected override ISqlNode Visit( SqlInsertStatement e )
        {
            return VisitContext.RangeFilterStatus.IsIncludedInFilteredRange()
                    ? e.AddColumns( _columns )
                    : base.Visit( e );
        }

        /// <summary>
        /// Adds the columns to the <see cref="SqlUpdateStatement.Assigns"/> if it's included the selected range.
        /// Returns <paramref name="e"/> if outside the selected range.
        /// </summary>
        /// <param name="e">The select specification.</param>
        /// <returns>The resulting node.</returns>
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

        /// <summary>
        /// Adds the columns to the <see cref="SelectSpec.Columns"/> if it's included the selected range.
        /// Returns <paramref name="e"/> if outside the selected range.
        /// </summary>
        /// <param name="e">The select specification.</param>
        /// <returns>The resulting node.</returns>
        protected override ISqlNode Visit( SelectSpec e )
        {
            return VisitContext.RangeFilterStatus.IsIncludedInFilteredRange()
                    ? e.InsertColumns( e.Columns.Count, _columns )
                    : base.Visit( e );
        }

    }
}
