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

        SqlEnclosedCommaList( SqlEnclosedCommaList o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( o, 0, leading, items, trailing )
        {
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlEnclosedCommaList( this, leading, children, trailing );
        }
        
        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor ) => visitor.Visit( this );

    }

}
