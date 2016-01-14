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
    /// Enclosed, possibly empty comma separated list of <see cref="ISqlIdentifier"/>.
    /// </summary>
    public sealed class SqlEnclosedIdentifierCommaList : ASqlNodeEnclosableSeparatedList<SqlTokenOpenPar,ISqlIdentifier,SqlTokenComma,SqlTokenClosePar>, 
                                                        ISqlStructurallyEnclosed
    {
        /// <summary>
        /// Initializes a new list of identifiers.
        /// </summary>
        /// <param name="openPar">Opening parenthesis. Can not be null.</param>
        /// <param name="tokens">Comma separated list of <see cref="ISqlIdentifier"/>.</param>
        /// <param name="closePar">Closing parenthesis. Can not be null.</param>
        public SqlEnclosedIdentifierCommaList( SqlTokenOpenPar openPar, IEnumerable<ISqlNode> tokens, SqlTokenClosePar closePar )
            : base( 0, openPar, tokens, closePar )
        {
        }

        SqlEnclosedIdentifierCommaList( SqlEnclosedIdentifierCommaList o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( o, 0, leading, items, trailing )
        {
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlEnclosedIdentifierCommaList( this, leading, children, trailing );
        }

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor ) => visitor.Visit( this );

    }

}
