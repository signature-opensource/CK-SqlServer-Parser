using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Parser
{
    /// <summary>
    /// Facade model implementation.
    /// </summary>
    public class SqlServerParser : ISqlServerParser
    {
        public ISqlServerParserError ParseObject( string text, out ISqlServerObject sqlObject )
        {
            return SqlAnalyser.ParseStatement( out sqlObject, text );
        }

        public ISqlServerParserError ParseStoredProcedure( string text, out ISqlServerStoredProcedure sqlProcedure )
        {
            return SqlAnalyser.ParseStatement( out sqlProcedure, text );
        }
    }
}
