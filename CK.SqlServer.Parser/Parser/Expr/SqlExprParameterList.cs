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
    public class SqlExprParameterList : SqlExprBaseExprList<SqlExprParameter>, ISqlServerParameterList
    {
        /// <summary>
        /// Initializes a new list of parameters with enclosing parenthesis.
        /// </summary>
        /// <param name="openPar">Opening parenthesis. Can not be null.</param>
        /// <param name="content">Comma separated list of <see cref="SqlExprParameter"/> (possibly empty).</param>
        /// <param name="closePar">Closing parenthesis. Can not be null.</param>
        public SqlExprParameterList( SqlTokenOpenPar openPar, IList<ISqlNode> content, SqlTokenClosePar closePar )
            : base( openPar, content, closePar, true )
        {
        }

        /// <summary>
        /// Initializes a new list of parameters without parenthesis.
        /// </summary>
        /// <param name="content">Comma separated list of <see cref="SqlExprParameter"/> (possibly empty).</param>
        public SqlExprParameterList( IList<ISqlNode> content )
            : base( content, true )
        {
        }

        internal SqlExprParameterList( ImmutableList<SqlTrivia> leading, ISqlNode[] items, ImmutableList<SqlTrivia> trailing )
            : base( leading, items, trailing )
        {
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IReadOnlyList<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlExprParameterList( leading, EnsureArray( children ), trailing );
        }

        string ISqlServerParameterList.ToStringClean() => ChildrenNodes.ToStringCompact();

        ISqlServerParameter IReadOnlyList<ISqlServerParameter>.this[int i]
        {
            get { return this[i]; }
        }

        IEnumerator<ISqlServerParameter> IEnumerable<ISqlServerParameter>.GetEnumerator()
        {
            return GetEnumerator();
        }

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor )
        {
            return visitor.Visit( this );
        }

    }

}
