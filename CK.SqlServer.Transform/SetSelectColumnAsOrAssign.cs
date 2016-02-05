using CK.Core;
using CK.SqlServer.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Transform
{
    public class SetSelectColumnAsOrAssign : SqlNodeTransformer
    {
        readonly bool _equalSyntax;
        
        public SetSelectColumnAsOrAssign( IActivityMonitor monitor, bool useEqualSyntax )
            : base( monitor )
        {
            _equalSyntax = useEqualSyntax;
        }

        public override ISqlNode Visit( SelectColumn e )
        {
            e = _equalSyntax ? e.ToEqualSyntax() : e.ToAsSyntax();
            return base.Visit( e );
        }
    }
}
