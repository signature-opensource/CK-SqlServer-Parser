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
    /// <summary>
    /// Sql analyzer that parses strings into <see cref="ISqlNode"/> parse trees.
    /// </summary>
    public sealed partial class SqlAnalyser
    {
        readonly SqlTokenReader R;

        /// <summary>
        /// Captures any form of analysis error.
        /// </summary>
        public class ErrorResult
        {
            readonly string _errorMessage;
            readonly string _headSource;

            /// <summary>
            /// Implicitly converts this result into a boolean.
            /// </summary>
            /// <param name="r">A result.</param>
            public static implicit operator bool ( ErrorResult r ) { return r == NoError; }

            internal ErrorResult( string errorMessage, string headSource )
            {
                Debug.Assert( NoError == null || (errorMessage != null && headSource != null) );
                _errorMessage = errorMessage;
                _headSource = headSource;
            }

            /// <summary>
            /// Gets whether the analysis failed.
            /// </summary>
            public bool IsError => this != NoError;

            /// <summary>
            /// Gets whether the analysis was successful.
            /// </summary>
            public bool Success => this == NoError;

            /// <summary>
            /// Gets the error message.
            /// Null if no error occurred.
            /// </summary>
            public string? ErrorMessage => _errorMessage;

            /// <summary>
            /// Gets the text where the error occurred.
            /// Null if no error occurred.
            /// </summary>
            public string? HeadSource => _headSource;

            /// <summary>
            /// Overridden to return the error and the <see cref="HeadSource"/>.
            /// </summary>
            /// <returns>A readable string.</returns>
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
                Throw.CheckNotNullArgument( monitor );
                if( IsError )
                {
                    using( asWarning ? monitor.OpenWarn( _errorMessage ) : monitor.OpenError( _errorMessage ) )
                    {
                        // OpenError automatically sets the filter to Debug for the group, but not OpenWarn.
                        if( asWarning ) monitor.TemporarilySetMinimalFilter( LogFilter.Debug );
                        monitor.Info( _headSource );
                    }
                }
            }
        }

        /// <summary>
        /// Parses a string that must be a sql statement.
        /// </summary>
        /// <param name="statement">The output statement. Null if an error occur.</param>
        /// <param name="text">The text to parse.</param>
        /// <returns>An error result.</returns>
        [DebuggerStepThrough]
        public static ErrorResult ParseStatement( out ISqlStatement? statement, string text )
        {
            SqlAnalyser a = new SqlAnalyser( text );
            statement = a.IsExtendedStatement( true );
            return statement != null ? ErrorResult.NoError : a.CreateErrorResult();
        }

        /// <summary>
        /// Parses a piece of Sql according to the <see cref="ParseMode"/>.
        /// </summary>
        /// <param name="sql">The output statement. Null if an error occur.</param>
        /// <param name="mode">How the text should be analyzed.</param>
        /// <param name="text">The text to parse.</param>
        /// <returns>An error result.</returns>
        [DebuggerStepThrough]
        public static ErrorResult Parse( out ISqlNode? sql, ParseMode mode, string text )
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
        /// <param name="text">Text to analyze.</param>
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

        /// <summary>
        /// Reinitializes this analyzer on a new input.
        /// </summary>
        /// <param name="text">The text to parse.</param>
        /// <returns>True on success, false if the first token cannot be read.</returns>
        public bool Reset( string text )
        {
            _statementLevel = 0;
            R.Reset( text );
            return R.MoveNext();
        }

        /// <summary>
        /// Analyzes the current input text according to a <see cref="ParseMode"/>.
        /// </summary>
        /// <param name="mode">How to analyze the text.</param>
        /// <returns>The resulting node or null if the text cannot be parsed.</returns>
        public ISqlNode? Parse( ParseMode mode = ParseMode.OneOrMoreStatements )
        {
            switch( mode )
            {
                case ParseMode.OneExpression: return IsOneExpression( true );
                case ParseMode.ExtendedExpression: return IsExtendedExpression( true );
                case ParseMode.AnyExpression: return IsAnyExpression( true );
                case ParseMode.Statement: return IsExtendedStatement( true );
                case ParseMode.TransformStatement: return IsTransformStatement( true );
                case ParseMode.Script: return IsStatementList( true );
                default:
                    {
                        Debug.Assert( mode == ParseMode.OneOrMoreStatements );
                        return IsOneOrMoreStatements( false );
                    }
            }
        }

        /// <summary>
        /// Returns either one <see cref="ISqlStatement"/> (<see cref="IsExtendedStatement"/>) if only one statement is found or 
        /// a <see cref="SqlStatementList"/> of ExtendedStatement.
        /// </summary>
        /// <param name="expected">True to set an error if no statements are parsed.</param>
        /// <returns>The parsed node or null.</returns>
        public ISqlNode? IsOneOrMoreStatements( bool expected )
        {
            var statements = IsStatementList( expected );
            if( statements == null ) return null;
            return statements.Count == 1 ? (ISqlNode)statements[0] : statements;
        }

        /// <summary>
        /// Returns a <see cref="SqlStatementList"/>.
        /// </summary>
        /// <param name="expected">True to expect at least one statement.</param>
        /// <returns>Non null statement list on success, otherwise null.</returns>
        public SqlStatementList? IsStatementList( bool expected )
        {
            return IsList( expected, IsExtendedStatement, i => new SqlStatementList( i ) );
        }

        /// <summary>
        /// Overridden to return the state of this analyzer.
        /// </summary>
        /// <returns>A (unfortunately not so) readable string.</returns>
        public override string ToString()
        {
            return R.ToString();
        }

        /// <summary>
        /// Creates an error object with the current error if there is one.
        /// </summary>
        /// <returns>An error object (may be without any error).</returns>
        public ErrorResult CreateErrorResult()
        {
            return R.IsError ? new ErrorResult( R.GetErrorMessage(), R.ToString() ) : ErrorResult.NoError;
        }

    }


}

