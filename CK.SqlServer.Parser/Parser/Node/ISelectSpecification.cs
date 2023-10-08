
namespace CK.SqlServer.Parser
{
    /// <summary>
    /// Abstraction of a select specification: a <see cref="SelectOperator"/> and a list of <see cref="Columns"/>.
    /// </summary>
    public interface ISelectSpecification : ISqlNode
    {
        /// <summary>
        /// Gets the operator type. It can be:
        /// <list type="bullet">
        ///  <item>
        ///     <see cref="SqlTokenType.None"/> if this is a <see cref="SelectSpec"/>.
        ///  </item>
        ///  <item>
        ///     <see cref="SelectOperatorKind.UnionAll"/>, <see cref="SelectOperatorKind.UnionDistinct"/>, <see cref="SelectOperatorKind.Except"/>, 
        ///     <see cref="SelectOperatorKind.Intersect"/> if this is a <see cref="SelectCombine"/>, 
        ///  </item>
        ///  <item>
        ///     <see cref="SelectOperatorKind.Decorator"/> for a <see cref="SelectDecorator"/>.
        ///  </item>
        /// </list>
        /// </summary>
        SelectOperatorKind SelectOperator { get; }

        /// <summary>
        /// Gets the columns. This is the columns of the first or top <see cref="SelectSpec"/>.
        /// </summary>
        SelectColumnList Columns { get; }

    }
}
