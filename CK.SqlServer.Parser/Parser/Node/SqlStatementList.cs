using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using CK.Core;
using System.Collections.Immutable;

namespace CK.SqlServer.Parser
{
    /// <summary>
    /// List of possibly empty <see cref="ISqlStatement">statements</see>. 
    /// </summary>
    public sealed class SqlStatementList : ASqlNodeList<ISqlStatement>, ISqlStatement
    {
        public SqlStatementList( IEnumerable<ISqlStatement> statements )
            : base( 0, statements )
        {
        }

        SqlStatementList( SqlStatementList o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> statements, ImmutableList<SqlTrivia> trailing )
            : base( o, 0, leading, statements, trailing )
        {
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlStatementList( this, leading, content, trailing );
        }

        SqlTokenTerminal ISqlStatement.StatementTerminator => null;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );
    }


}
