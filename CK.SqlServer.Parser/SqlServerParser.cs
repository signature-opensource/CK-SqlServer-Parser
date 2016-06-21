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

            public ParseResult( T result, SqlAnalyser.ErrorResult e )
            {
                Result = result;
                _error = e;
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

        public ISqlServerParserResult<ISqlServerView> ParseView( string text )
        {
            _a.Reset( text );
            return new ParseResult<ISqlServerView>( _a.ParseStatement );
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
            SqlStatementList statements = _a.IsStatementList( true );
            if( statements != null )
            {
                // SingleOrDefault throws on multiple items.
                //var onlyOne = statements.Where( s => !s.IsGOSeparator() ).SingleOrDefault() as ISqlServerParsedText;
                ISqlStatement onlyOne = null;
                foreach( var s in statements )
                {
                    if( !s.IsGOSeparator() )
                    {
                        if( onlyOne != null )
                        {
                            onlyOne = null;
                            break;
                        }
                        else onlyOne = s;
                    }
                }
                ISqlServerParsedText parsed = onlyOne as ISqlServerParsedText;
                if( parsed != null )
                {
                    return new ParseResult<ISqlServerParsedText>( parsed, _a.CreateErrorResult() );
                }
            }
            return new ParseResult<ISqlServerParsedText>( statements, _a.CreateErrorResult() );
        }

        public ISqlServerParserResult<ISqlServerScript> ParseScript( string text )
        {
            _a.Reset( text );
            return new ParseResult<ISqlServerScript>( _a.IsStatementList( true ), _a.CreateErrorResult() );
        }

    }
}
