using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Parser
{
    public interface ISqlNamedStatement : ISqlStatement
    {
        StatementKnownName StatementKnownName { get; }

    }

    static public class SqlNamedStatementExtension
    {

        /// <summary>
        /// Gets either the <see cref="ISqlNamedStatement.StatementKnownName"/> or,
        /// when it is <see cref="StatementKnownName.Unknown"/>, the name of the 
        /// first <see cref="ISqlIdentifier"/> token of the sttement.
        /// </summary>
        /// <param name="@this">This named statement.</param>
        /// <returns>The statement name.</returns>
        static public string GetStatementName( this ISqlNamedStatement @this )
        {
            StatementKnownName n = @this.StatementKnownName;
            if( n != StatementKnownName.Unknown ) return n.ToString();
            return @this.AllTokens.OfType<ISqlIdentifier>().Select( id => id.ToString() ).FirstOrDefault() ?? String.Empty;
        }

    }

}
