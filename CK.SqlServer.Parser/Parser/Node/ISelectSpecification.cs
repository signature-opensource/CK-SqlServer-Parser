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
        ///  <see cref="SelectOperatorKind.UnionDistinct"/>, <see cref="SqlTokenType.Except"/>, <see cref="SqlTokenType.Intersect"/>
        /// if this is a <see cref="SelectCombine"/>, 
        /// <see cref="SqlTokenType.Order"/> for a <see cref="SelectOrderBy"/>, 
        /// <see cref="SqlTokenType.For"/> 
        /// for <see cref="SelectFor"/> and <see cref="SqlTokenType.None"/> if this is a <see cref="SelectSpec"/>.
        /// </summary>
        SelectOperatorKind SelectOperator { get; }

        /// <summary>
        /// Gets the columns. This is the columns of the first or top <see cref="SelectSpec"/>.
        /// </summary>
        SelectColumnList Columns { get; }

    }
}
