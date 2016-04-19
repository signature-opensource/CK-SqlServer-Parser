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
    /// Enclosed comma separated list of <see cref="ISqlNode"/>. Possibly empty.
    /// </summary>
    public sealed class SqlEnclosedCommaList : ASqlNodeEnclosableSeparatedList<SqlTokenOpenPar,ISqlNode,SqlTokenComma,SqlTokenClosePar>,
                                               ISqlStructurallyEnclosed
    {
        /// <summary>
        /// Initializes a new <see cref="SqlEnclosedCommaList"/>.
        /// </summary>
        /// <param name="openPar">Can not be null.</param>
        /// <param name="content">Items and comma tokens.</param>
        /// <param name="closePar">Can not be null.</param>
        public SqlEnclosedCommaList( SqlTokenOpenPar openPar, IEnumerable<ISqlNode> content, SqlTokenClosePar closePar )
            : base( 0, openPar, content, closePar )
        {
        }

        /// <summary>
        /// Initializes a new <see cref="SqlEnclosedCommaList"/> with one or zero item in it.
        /// </summary>
        /// <param name="item">Optional item.</param>
        public SqlEnclosedCommaList( ISqlNode item = null )
            : base( 0, SqlKeyword.OpenPar, item == null ? Util.Array.Empty<ISqlNode>() : new[] { item }, SqlKeyword.ClosePar )
        {
        }

        SqlEnclosedCommaList( SqlEnclosedCommaList o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( o, 0, leading, items, trailing )
        {
        }

        public SqlEnclosedCommaList InsertAt( int idx, ISqlNode item ) => (SqlEnclosedCommaList)DoInsertAt( idx, item );

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlEnclosedCommaList( this, leading, content, trailing );
        }
        
        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

    }

}
