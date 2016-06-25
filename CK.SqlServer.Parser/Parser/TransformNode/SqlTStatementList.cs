using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using CK.Core;
using System.Collections.Immutable;
using CK.Text;

namespace CK.SqlServer.Parser
{
    /// <summary>
    /// List of possibly empty <see cref="ISqlTStatement">transformer statements</see>. 
    /// </summary>
    public sealed class SqlTStatementList : ASqlNodeList<ISqlTStatement>
    {
        public SqlTStatementList( IEnumerable<ISqlTStatement> statements )
            : base( 0, statements )
        {
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
