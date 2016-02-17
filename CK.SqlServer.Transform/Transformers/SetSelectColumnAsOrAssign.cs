using CK.Core;
using CK.SqlServer.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Transform.Transformers
{
    public class SetSelectColumnAsOrAssign : SqlNodeLocationVisitor
    {
        readonly bool _equalSyntax;
        
        public SetSelectColumnAsOrAssign( bool useEqualSyntax )
        {
            _equalSyntax = useEqualSyntax;
        }

        protected override ISqlNode Visit( SelectColumn e )
        {
            e = _equalSyntax ? e.ToEqualSyntax() : e.ToAsSyntax();
            return base.Visit( e );
        }
    }
}
