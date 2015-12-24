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
    public sealed class SqlParameterList : ASqlNodeEnclosedSeparatedList<SqlTokenOpenPar,SqlParameter,SqlTokenComma,SqlTokenClosePar>, ISqlServerParameterList
    {
        /// <summary>
        /// Initializes a new list of parameters with optional enclosing parenthesis.
        /// </summary>
        /// <param name="openPar">Opening parenthesis. Can be null.</param>
        /// <param name="content">Comma separated list of <see cref="SqlParameter"/> (possibly empty).</param>
        /// <param name="closePar">Closing parenthesis. Can be null.</param>
        public SqlParameterList( SqlTokenOpenPar openPar, IEnumerable<ISqlNode> content, SqlTokenClosePar closePar )
            : base( 0, true, openPar, content, closePar )
        {
        }


        SqlParameterList( SqlParameterList o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( o, 0, true, leading, items, trailing )
        {
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IReadOnlyList<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlParameterList( this, leading, children, trailing );
        }

        string ISqlServerParameterList.ToStringClean() => ChildrenNodes.ToStringCompact();

        ISqlServerParameter IReadOnlyList<ISqlServerParameter>.this[int i] => this[i];

        IEnumerator<ISqlServerParameter> IEnumerable<ISqlServerParameter>.GetEnumerator() => GetEnumerator();

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor ) => visitor.Visit( this );

    }

}
