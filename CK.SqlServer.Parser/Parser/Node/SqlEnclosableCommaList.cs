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
    /// Comma separated list of <see cref="ISqlNode"/>. Possibly enclosed and empty.
    /// </summary>
    public sealed class SqlEnclosableCommaList : ASqlNodeEnclosableSeparatedList<SqlTokenOpenPar,ISqlNode,SqlTokenComma,SqlTokenClosePar>
    {
        /// <summary>
        /// Initializes a new <see cref="SqlEnclosableCommaList"/>.
        /// </summary>
        /// <param name="content">Items and comma tokens.</param>
        public SqlEnclosableCommaList( SqlTokenOpenPar openPar, IEnumerable<ISqlNode> content, SqlTokenClosePar closePar, ImmutableList<SqlTrivia> leading = null, ImmutableList<SqlTrivia> trailing = null )
            : base( 0, openPar, content, closePar, leading, trailing )
        {
        }

        SqlEnclosableCommaList( SqlEnclosableCommaList o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( o, 0, leading, items, trailing )
        {
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlEnclosableCommaList( this, leading, children, trailing );
        }

        public ISqlNode Enclose( SqlTokenOpenPar opener, SqlTokenClosePar closer )
        {
            if( opener == null ) throw new ArgumentNullException( nameof( opener ) );
            if( closer == null ) throw new ArgumentNullException( nameof( closer ) );
            if( IsEnclosed ) return new SqlPar( opener, this, closer, LeadingTrivias, TrailingTrivias );
            return new SqlEnclosableCommaList( opener, ChildrenNodes, closer, LeadingTrivias, TrailingTrivias );
        }

        
        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor ) => visitor.Visit( this );

    }

}
