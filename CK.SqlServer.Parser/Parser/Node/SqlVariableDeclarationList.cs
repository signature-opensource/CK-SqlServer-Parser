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
    public sealed class SqlVariableDeclarationList : ASqlNodeSeparatedList<SqlVariableDeclaration,SqlTokenComma>
    {
        /// <summary>
        /// Initializes a new list of variable declarations.
        /// </summary>
        /// <param name="content">Comma separated list of <see cref="SqlVariableDeclaration"/> (must not be empty).</param>
        public SqlVariableDeclarationList( IEnumerable<ISqlNode> items )
            : base( null, 1, null, items, null )
        {
        }

        SqlVariableDeclarationList( SqlVariableDeclarationList o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( o, 1, leading, items, trailing )
        {
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlVariableDeclarationList( this, leading, children, trailing );
        }

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor ) => visitor.Visit( this );

    }

}
