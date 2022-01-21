using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CK.SqlServer.Parser
{
    /// <summary>
    /// Enclosed, possibly empty comma separated list of <see cref="ISqlIdentifier"/>.
    /// </summary>
    public sealed class SqlEnclosedIdentifierCommaList : ASqlNodeEnclosableSeparatedList<SqlTokenOpenPar, ISqlIdentifier, SqlTokenComma, SqlTokenClosePar>,
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

        /// <summary>
        /// Initializes a new list of identifiers with one or zero identifier inside.
        /// </summary>
        /// <param name="id">An optional <see cref="ISqlIdentifier"/>.</param>
        public SqlEnclosedIdentifierCommaList( ISqlIdentifier id = null )
            : base( 0, SqlKeyword.OpenPar, id == null ? Array.Empty<ISqlIdentifier>() : new[] { id }, SqlKeyword.ClosePar )
        {
        }

        SqlEnclosedIdentifierCommaList( SqlEnclosedIdentifierCommaList o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( o, 0, leading, items, trailing )
        {
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlEnclosedIdentifierCommaList( this, leading, content, trailing );
        }

        public SqlEnclosedIdentifierCommaList InsertAt( int idx, ISqlIdentifier identifier )
        {
            return (SqlEnclosedIdentifierCommaList)DoInsertAt( idx, identifier );
        }

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

    }

}
