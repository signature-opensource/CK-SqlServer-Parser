using CK.Core;
using CK.SqlServer.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Transform.Transformers
{
    public class AddColumnInInsert : SqlNodeLocationVisitor
    {
        readonly SqlTokenIdentifier _columnName;
        readonly ISqlNode _expression;

        public AddColumnInInsert( SqlTokenIdentifier columnName, ISqlNode expression = null )
        {
            if( columnName == null ) throw new ArgumentNullException( nameof( columnName ) );
            _columnName = columnName;
            _expression = expression;
        }

        protected override ISqlNode Visit( SqlInsertStatement e )
        {
            return e.AddSimpleColumn( _columnName, _expression );
        }

    }
}
