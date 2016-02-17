using CK.Core;
using CK.SqlServer.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Transform.Transformers
{
    public class AddParameter : SqlNodeLocationVisitor
    {
        readonly SqlParameter _param;
        readonly string _paramNameBefore;
        readonly string _paramNameAfter;

        public AddParameter( SqlParameter param, string paramNameBefore = null, string paramNameAfter = null )
        {
            _param = param;
            _paramNameBefore = paramNameBefore;
            _paramNameAfter = paramNameAfter;
        }

        protected override ISqlNode Visit( SqlParameterList e )
        {
            int idx = _paramNameBefore != null 
                        ? e.IndexOf( p => p.Name == _paramNameBefore ) + 1 
                        : (_paramNameAfter != null 
                            ? e.IndexOf( p => p.Name == _paramNameAfter ) 
                            : e.Count);
            StopVisit();
            return e.InsertAt( idx, _param );
        }
    }
}
