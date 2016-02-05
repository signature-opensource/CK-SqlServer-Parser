using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CK.SqlServer.Parser
{

    public interface ISelectSpecification : ISqlNode
    {
        /// <summary>
        /// Gets the operator type: it can be:
        /// <see cref="SqlTokenType.None"/> if this is a <see cref="SelectSpec"/>.
        ///  <see cref="SelectOperatorKind.UnionAll"/>, 
        ///  <see cref="SelectOperatorKind.UnionDistinct"/>, 
        ///  <see cref="SelectOperatorKind.Except"/>, 
        ///  <see cref="SelectOperatorKind.Intersect"/> if this is a <see cref="SelectCombine"/>, 
        /// <see cref="SelectOperatorKind.Decorator"/> for a <see cref="SelectDecorator"/>.
        /// </summary>
        SelectOperatorKind SelectOperator { get; }

        /// <summary>
        /// Gets the columns. This is the columns of the first or top <see cref="SelectSpec"/>.
        /// </summary>
        SelectColumnList Columns { get; }

    }
}
