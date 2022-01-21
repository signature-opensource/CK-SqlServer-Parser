using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member


namespace CK.SqlServer.Parser
{
    /// <summary>
    /// List of possibly empty <see cref="ISqlTStatement">transformer statements</see> enclosed in a <see cref="SqlTokenType.Begin"/>
    /// and <see cref="SqlTokenType.End"/>. 
    /// </summary>
    public sealed class SqlTStatementList : ASqlNodeEnclosableList<SqlTokenIdentifier, ISqlTStatement,SqlTokenIdentifier>
    {
        public SqlTStatementList( SqlTokenIdentifier beginT, IEnumerable<ISqlTStatement> statements, SqlTokenIdentifier endT )
            : base( 0, beginT, statements, endT )
        {
            if( beginT.TokenType != SqlTokenType.Begin ) throw new ArgumentException();
            if( endT.TokenType != SqlTokenType.End ) throw new ArgumentException();
        }

        SqlTStatementList( SqlTStatementList o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> statements, ImmutableList<SqlTrivia> trailing )
            : base( o, 0, leading, statements, trailing )
        {
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlTStatementList( this, leading, content, trailing );
        }

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );
    }


}
