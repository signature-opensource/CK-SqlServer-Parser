using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Parser;

/// <summary>
/// Defines actual statements. 
/// The <see cref="StatementTerminator"/> is optional.
/// </summary>
public interface ISqlStatement : ISqlStatementPart
{
    /// <summary>
    /// Gets the optional statement terminator.
    /// </summary>
    SqlTokenTerminal StatementTerminator { get; }
}

/// <summary>
/// Extends <see cref="ISqlStatement"/>.
/// </summary>
public static class SqlStatementExtension
{
    /// <summary>
    /// Checks whether this is the GO separator.
    /// </summary>
    /// <param name="this">This statement.</param>
    /// <returns>True for GO separator otherwise false.</returns>
    public static bool IsGOSeparator( this ISqlStatement @this )
    {
        SqlStatement s = @this as SqlStatement;
        return s != null && s.Name.IsToken( SqlTokenType.Go );
    }

}
