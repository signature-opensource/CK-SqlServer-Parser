using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using CK.Core;
using System.Collections.Immutable;
using System.Collections;

namespace CK.SqlServer.Parser
{
    /// <summary>
    /// A possibly empty and possibly enclosed comma separated list of <see cref="SqlParameter"/>.
    /// </summary>
    public sealed class SqlParameterList : ASqlNodeEnclosableSeparatedList<SqlTokenOpenPar,SqlParameter,SqlTokenComma,SqlTokenClosePar>
    {
        readonly ModelParametersAdapter _modelParameters;

        /// <summary>
        /// Initializes a new list of parameters with optional enclosing parenthesis.
        /// </summary>
        /// <param name="openPar">Opening parenthesis. Can be null.</param>
        /// <param name="content">Comma separated list of <see cref="SqlParameter"/> (possibly empty).</param>
        /// <param name="closePar">Closing parenthesis. Can be null.</param>
        public SqlParameterList( SqlTokenOpenPar openPar, IEnumerable<ISqlNode> content, SqlTokenClosePar closePar )
            : base( 0, openPar, content, closePar )
        {
            _modelParameters = new ModelParametersAdapter( this );
        }


        SqlParameterList( SqlParameterList o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( o, 0, leading, items, trailing )
        {
            _modelParameters = new ModelParametersAdapter( this );
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlParameterList( this, leading, content, trailing );
        }

        class ModelParametersAdapter : ISqlServerParameterList
        {
            readonly SqlParameterList _p;

            public ModelParametersAdapter( SqlParameterList p )
            {
                _p = p;
            }
            public ISqlServerParameter this[int index] => _p[index];

            public int Count => _p.Count;

            public IEnumerator<ISqlServerParameter> GetEnumerator() => _p.GetEnumerator();

            public string ToStringClean() => _p.ChildrenNodes.ToStringCompact();

            IEnumerator IEnumerable.GetEnumerator() => _p.GetEnumerator();
        }

        internal ISqlServerParameterList ModelParameters => _modelParameters;

        /// <summary>
        /// Inserts a new <see cref="SqlParameter"/> in this list.
        /// </summary>
        /// <param name="idx">Index to isert. Can be equal to the current number of parameters.</param>
        /// <param name="p">The parameter to insert.</param>
        /// <returns>A new list.</returns>
        public SqlParameterList InsertAt( int idx, SqlParameter p )
        {
            return (SqlParameterList)base.DoInsertAt( idx, p );
        }

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

    }

}
