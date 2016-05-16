using CK.Core;
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

        delegate SqlAnalyser.ErrorResult ParseFunc<T>( out T parsed );

        class ParseResult<T> : ISqlServerParserResult<T> where T : class, ISqlServerParsedText
        {
            readonly SqlAnalyser.ErrorResult _error;

            public ParseResult( ParseFunc<T> f )
            {
                T result;
                _error = f( out result );
                Result = result;
            }

            public string ErrorMessage => _error.ErrorMessage;

            public string HeadSource => _error.HeadSource;

            public bool IsError => _error.IsError;

            public T Result { get; }

            public void LogOnError( IActivityMonitor monitor, bool asWarning ) => _error.LogOnError( monitor, asWarning );
        }

        public ISqlServerParserResult<ISqlServerObject> ParseObject( string text )
        {
            _a.Reset( text );
            return new ParseResult<ISqlServerObject>( _a.ParseStatement );
        }

        public ISqlServerParserResult<ISqlServerTransformer> ParseTransformer( string text )
        {
            _a.Reset( text );
            return new ParseResult<ISqlServerTransformer>( _a.ParseStatement );
        }

        public ISqlServerParserResult<ISqlServerFunctionInlineTable> ParseFunctionInlineTable( string text )
        {
            _a.Reset( text );
            return new ParseResult<ISqlServerFunctionInlineTable>( _a.ParseStatement );
        }

        public ISqlServerParserResult<ISqlServerFunctionScalar> ParseFunctionScalar( string text )
        {
            _a.Reset( text );
            return new ParseResult<ISqlServerFunctionScalar>( _a.ParseStatement );
        }

        public ISqlServerParserResult<ISqlServerFunctionTable> ParseFunctionTable( string text )
        {
            _a.Reset( text );
            return new ParseResult<ISqlServerFunctionTable>( _a.ParseStatement );
        }

        public ISqlServerParserResult<ISqlServerStoredProcedure> ParseStoredProcedure( string text )
        {
            _a.Reset( text );
            return new ParseResult<ISqlServerStoredProcedure>( _a.ParseStatement );
        }

        public ISqlServerParserResult<ISqlServerParsedText> Parse( string text )
        {
            _a.Reset( text );
            return new ParseResult<ISqlServerParsedText>( _a.ParseStatement );
        }

        public ISqlServerParserResult<ISqlServerScript> ParseScript( string text )
        {
            _a.Reset( text );
            return new ParseResult<ISqlServerScript>( _a.ParseStatement );
        }

    }
}
