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
    /// Transformer of "=" to "as" (or the opposite) in <see cref="SelectColumn"/> elements.
    /// </summary>
    public class SetSelectColumnAsOrAssign : SqlNodeLocationVisitor
    {
        readonly bool _equalSyntax;

        /// <summary>
        /// Initializes a new <see cref="SetSelectColumnAsOrAssign"/>.
        /// </summary>
        /// <param name="monitor">The monitor to use.</param>
        /// <param name="useEqualSyntax">True to use "=" instead of "as" syntax.</param>
        public SetSelectColumnAsOrAssign( IActivityMonitor monitor, bool useEqualSyntax )
            : base( monitor )
        {
            _equalSyntax = useEqualSyntax;
        }

        /// <summary>
        /// Calls <see cref="SelectColumn.ToAsSyntax()"/> or <see cref="SelectColumn.ToEqualSyntax()"/>.
        /// </summary>
        /// <param name="e">The column.</param>
        /// <returns>The resulting node.</returns>
        protected override ISqlNode Visit( SelectColumn e )
        {
            e = _equalSyntax ? e.ToEqualSyntax() : e.ToAsSyntax();
            return base.Visit( e );
        }
    }
}
