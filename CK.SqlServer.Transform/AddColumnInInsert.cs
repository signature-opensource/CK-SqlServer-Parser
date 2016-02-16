using CK.Core;
using CK.SqlServer.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Transform
{
    public class AddColumnInInsert : SqlNodeLocationVisitor
    {
        readonly ISqlIdentifier _columnName;
        readonly ISqlNode _expression;

        public AddColumnInInsert( ISqlIdentifier columnName, ISqlNode expression = null )
        {
            if( columnName != null ) throw new ArgumentNullException( nameof( columnName ) );
            _columnName = columnName;
            _expression = expression;
        }

        protected override ISqlNode Visit( SqlInsertStatement e )
        {
            var newColumns = e.HasColumns
                                ? e.Columns.InsertAt( e.Columns.Count, _columnName )
                                : new SqlEnclosedIdentifierCommaList( _columnName );
            e = e.SetColumns( newColumns );
            if( _expression != null )
            {
                ISqlNode newValues;
                if( e.ValuesIsDefaultValues )
                {
                    newValues = new SqlTableValues( SqlKeyword.Values, 
                                                    new SqlMultiCommaList( new SqlEnclosedCommaList( _expression ) ),
                                                    e.Values.LeadingTrivias,
                                                    e.Values.TrailingTrivias );
                }
                else if( e.ValuesIsTableValues )
                {
                    SqlTableValues v = (SqlTableValues)e.Values;
                    newValues = v.AppendValue( _expression );
                }
                else throw new NotSupportedException( "Can not add column in 'insert into execute' or 'insert into select'." );
                e = e.SetValues( newValues );
            }
            return e;
        }

    }
}
