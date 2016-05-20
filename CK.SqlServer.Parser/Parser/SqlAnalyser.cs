using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using CK.Core;
using CK.SqlServer;

namespace CK.SqlServer.Parser
{
    public partial class SqlAnalyser
    {
        readonly SqlTokenReader R;

        public class ErrorResult
        {
            readonly string _errorMessage;
            readonly string _headSource;
            public bool IsError { get { return this != NoError; } }
            public static implicit operator bool ( ErrorResult r ) { return r == NoError; }

            internal ErrorResult( string errorMessage, string headSource )
            {
                Debug.Assert( NoError == null || (errorMessage != null && headSource != null) );
                _errorMessage = errorMessage;
                _headSource = headSource;
            }

            public string ErrorMessage => _errorMessage;

            public string HeadSource => _headSource;

            public override string ToString()
            {
                return IsError ? string.Format( "Error: {0}\r\nText: {1}", _errorMessage, _headSource ) : "<success>";
            }

            static internal readonly ErrorResult NoError = new ErrorResult( null, null );

            /// <summary>
            /// Logs the error message if <see cref="IsError"/> is true, otherwise does nothing.
            /// </summary>
            /// <param name="monitor">Monitor to log into.</param>
            /// <param name="asWarning">True to log a warning instead of an error.</param>
            public void LogOnError( IActivityMonitor monitor, bool asWarning = false )
            {
                if( monitor == null ) throw new ArgumentNullException( "monitor" );
                if( IsError )
                {
                    using( asWarning ? monitor.OpenWarn().Send( _errorMessage ) : monitor.OpenError().Send( _errorMessage ) )
                    {
                        // OpenError automatically sets the filter to Debug for the group, but not OpenWarn.
                        if( asWarning ) monitor.SetMinimalFilter( LogFilter.Debug );
                        monitor.Info().Send( _headSource );
                    }
                }
            }
        }

        [DebuggerStepThrough]
        public static ErrorResult ParseStatement( out ISqlStatement statement, string text )
        {
            SqlAnalyser a = new SqlAnalyser( text );
            statement = a.IsExtendedStatement( true );
            return statement != null ? ErrorResult.NoError : a.CreateErrorResult();
        }

        [DebuggerStepThrough]
        public static ErrorResult Parse( out ISqlNode sql, ParseMode mode, string text )
        {
            sql = null;
            SqlAnalyser a = new SqlAnalyser( text );
            sql = a.Parse( mode );
            return sql != null ? ErrorResult.NoError : a.CreateErrorResult();
        }

        /// <summary>
        /// Gets the current result.
        /// </summary>
        public ErrorResult GetCurrentResult() => R.IsError ? CreateErrorResult() : ErrorResult.NoError;

        /// <summary>
        /// Initializes a new <see cref="SqlAnalyser"/> bound to a new <see cref="SqlTokenizer"/> instance 
        /// on a text (<see cref="Reset"/> is automatically called).
        /// </summary>
        /// <param name="text">Text to analyse.</param>
        public SqlAnalyser( string text )
        {
            R = new SqlTokenReader( new SqlTokenizer() );
            Reset( text );
        }

        /// <summary>
        /// Initializes a new <see cref="SqlAnalyser"/> bound to an existing tokenizer.
        /// </summary>
        /// <param name="t">The tokenizer.</param>
        public SqlAnalyser( SqlTokenizer t )
        {
            R = new SqlTokenReader( t );
            R.MoveNext();
        }

        /// <summary>
        /// Initializes a new <see cref="SqlAnalyser"/>.
        /// <see cref="Reset(string)"/> must be called.
        /// </summary>
        public SqlAnalyser()
        {
            R = new SqlTokenReader( new SqlTokenizer() );
        }

        public bool Reset( string text )
        {
            _statementLevel = 0;
            R.Reset( text );
            return R.MoveNext();
        }

        public ISqlNode Parse( ParseMode mode = ParseMode.OneOrMoreStatements )
        {
            switch( mode )
            {
                case ParseMode.OneExpression: return IsOneExpression( true );
                case ParseMode.ExtendedExpression: return IsExtendedExpression( true );
                case ParseMode.AnyExpression: return IsAnyExpression( true );
                case ParseMode.Statement: return IsExtendedStatement( true );
                case ParseMode.Script: return IsList( true, IsExtendedStatement, i => new SqlStatementList( i ) );
                default:
                    {
                        Debug.Assert( mode == ParseMode.OneOrMoreStatements );
                        return IsOneOrMoreStatements( false );
                    }
            }
        }

        /// <summary>
        /// Returns either one <see cref="IsExtendedStatement"/> if only one statement id found or 
        /// a <see cref="SqlNodeList"/> of ExtendedStatement.
        /// </summary>
        /// <param name="expected">True to set an error if no statements are parsed.</param>
        /// <returns>The parsed node.</returns>
        public ISqlNode IsOneOrMoreStatements( bool expected )
        {
            List<ISqlNode> items = new List<ISqlNode>();
            if( !R.CollectUntil<SqlToken>( items, IsExtendedStatement, R.GetDepthBasedStopper() ) ) return null;
            if( expected && items.Count == 0 )
            {
                R.SetCurrentError( "Expected one or more statement." );
                return null;
            }
            return items.Count == 1 ? items[0] : new SqlNodeList( items );
        }

        public override string ToString()
        {
            return R.ToString();
        }

        ErrorResult CreateErrorResult()
        {
            return new ErrorResult( R.GetErrorMessage(), R.ToString() );
        }

    }


}

