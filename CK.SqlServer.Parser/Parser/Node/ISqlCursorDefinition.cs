namespace CK.SqlServer.Parser;

/// <summary>
/// Abstraction of a cursor definition.
/// </summary>
public interface ISqlCursorDefinition : ISqlNode
{
    /// <summary>
    /// Gets whether the cursor follows Sql92 syntax.
    /// </summary>
    bool IsSql92Syntax { get; }

    /// <summary>
    /// Gets the 'cursor' token.
    /// </summary>
    SqlTokenIdentifier CursorT { get; }

    /// <summary>
    /// Gets the select specification.
    /// </summary>
    ISelectSpecification Select { get; }

}
