using CK.Core;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CK.SqlServer.Parser
{

    /// <summary>
    /// Comma separated list of <see cref="ISqlNode"/>. Possibly enclosed and empty.
    /// </summary>
    public sealed class SqlEnclosableCommaList : ASqlNodeEnclosableSeparatedList<SqlTokenOpenPar,ISqlNode,SqlTokenComma,SqlTokenClosePar>
    {
        /// <summary>
        /// Initializes a new <see cref="SqlEnclosableCommaList"/>.
        /// </summary>
        /// <param name="openPar">Opening parenthesis.</param>
        /// <param name="content">Items and comma tokens.</param>
        /// <param name="closePar">Closing parenthesis.</param>
        /// <param name="leading">Optional leading trivias.</param>
        /// <param name="trailing">Optional trailing trivias.</param>
        public SqlEnclosableCommaList( SqlTokenOpenPar openPar, IEnumerable<ISqlNode> content, SqlTokenClosePar closePar, ImmutableList<SqlTrivia> leading = null, ImmutableList<SqlTrivia> trailing = null )
            : base( 0, openPar, content, closePar, leading, trailing )
        {
        }

        SqlEnclosableCommaList( SqlEnclosableCommaList o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( o, 0, leading, items, trailing )
        {
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlEnclosableCommaList( this, leading, content, trailing );
        }

        public ISqlNode Enclose( SqlTokenOpenPar opener, SqlTokenClosePar closer )
        {
            Throw.CheckNotNullArgument( opener );
            Throw.CheckNotNullArgument( closer );
            if( IsEnclosed ) return new SqlPar( opener, this, closer, LeadingTrivias, TrailingTrivias );
            return new SqlEnclosableCommaList( opener, ChildrenNodes, closer, LeadingTrivias, TrailingTrivias );
        }

        
        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

    }

}
