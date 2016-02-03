using CK.Core;
using CK.SqlServer.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Transform
{
    public class AddParameter : SqlNodeTransformer
    {
        readonly SqlParameter _param;
        readonly string _paramNameBefore;
        readonly string _paramNameAfter;

        public AddParameter( IActivityMonitor monitor, SqlParameter param, string paramNameBefore = null, string paramNameAfter = null )
            : base( monitor )
        {
            _param = param;
            _paramNameBefore = paramNameBefore;
            _paramNameAfter = paramNameAfter;
        }

        public override ISqlNode Visit( SqlParameterList e )
        {
            int idx = _paramNameBefore != null 
                        ? e.IndexOf( p => p.Name == _paramNameBefore ) + 1 
                        : (_paramNameAfter != null 
                            ? e.IndexOf( p => p.Name == _paramNameAfter ) 
                            : e.Count);
            Monitor.Trace().Send( "Inserting '{0}' at index {1}.", _param, idx );
            StopVisit();
            return e.InsertAt( idx, _param );
        }
    }
}
