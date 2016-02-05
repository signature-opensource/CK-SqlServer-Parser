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
        readonly SqlAnalyser _a = new SqlAnalyser();

        public ISqlServerParserError ParseObject( string text, out ISqlServerObject sqlObject )
        {
            _a.Reset( text );
            return _a.ParseStatement( out sqlObject );
        }

        public ISqlServerParserError ParseStoredFunctionInlineTable( string text, out ISqlServerFunctionInlineTable sqlFInlineTable )
        {
            _a.Reset( text );
            return _a.ParseStatement( out sqlFInlineTable );
        }

        public ISqlServerParserError ParseStoredFunctionScalar( string text, out ISqlServerFunctionScalar sqlFScalar )
        {
            _a.Reset( text );
            return _a.ParseStatement( out sqlFScalar );
        }

        public ISqlServerParserError ParseStoredFunctionTable( string text, out ISqlServerFunctionTable sqlFTable )
        {
            _a.Reset( text );
            return _a.ParseStatement( out sqlFTable );
        }

        public ISqlServerParserError ParseStoredProcedure( string text, out ISqlServerStoredProcedure sqlProcedure )
        {
            _a.Reset( text );
            return _a.ParseStatement( out sqlProcedure );
        }
    }
}
