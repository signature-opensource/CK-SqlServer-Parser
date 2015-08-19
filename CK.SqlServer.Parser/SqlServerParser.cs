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

        public ISqlServerParserError ParseStoredFunctionInlineTable( string text, out ISqlServerFunctionInlineTable sqlFInlineTable )
        {
            return SqlAnalyser.ParseStatement( out sqlFInlineTable, text );
        }

        public ISqlServerParserError ParseStoredFunctionScalar( string text, out ISqlServerFunctionScalar sqlFScalar )
        {
            return SqlAnalyser.ParseStatement( out sqlFScalar, text );
        }

        public ISqlServerParserError ParseStoredFunctionTable( string text, out ISqlServerFunctionTable sqlFTable )
        {
            return SqlAnalyser.ParseStatement( out sqlFTable, text );
        }

        public ISqlServerParserError ParseStoredProcedure( string text, out ISqlServerStoredProcedure sqlProcedure )
        {
            return SqlAnalyser.ParseStatement( out sqlProcedure, text );
        }
    }
}
