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
    /// List of variable declarations.
    /// </summary>
    public sealed class SqlVariableDeclarationList : ASqlNodeSeparatedList<SqlVariableDeclaration,SqlTokenComma>
    {
        /// <summary>
        /// Initializes a new list of variable declarations.
        /// </summary>
        /// <param name="items">Comma separated list of <see cref="SqlVariableDeclaration"/> (must not be empty).</param>
        public SqlVariableDeclarationList( IEnumerable<ISqlNode> items )
            : base( null, 1, null, items, null )
        {
        }

        SqlVariableDeclarationList( SqlVariableDeclarationList o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( o, 1, leading, items, trailing )
        {
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlVariableDeclarationList( this, leading, content, trailing );
        }

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

    }

}
