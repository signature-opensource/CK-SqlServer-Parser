using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CK.SqlServer.Parser
{
    using CNode = SNode<SqlTokenIdentifier, SqlTokenOpenPar, SqlOverClausePartition, SqlOrderByClause, SqlOverClauseRowOrRange, SqlTokenClosePar>;

    /// <summary>
    /// See https://docs.microsoft.com/en-us/sql/t-sql/queries/select-over-clause-transact-sql#syntax
    /// OVER (   
    ///    [ PARTITION BY clause ] ==> SqlOverClausePartition  
    ///    [ ORDER BY clause ]     ==> SqlOrderByClause.
    ///    [ ROW or RANGE clause ] ==> SqlOverClauseRowOrRange
    ///   )  
    /// </summary>
    public sealed class SqlOverClause : SqlNonTokenAutoWidth
    {
        readonly CNode _content;

        public SqlOverClause( SqlTokenIdentifier overT, SqlTokenOpenPar opener, SqlOverClausePartition partition, SqlOrderByClause orderBy, SqlOverClauseRowOrRange rowOrRange, SqlTokenClosePar closer )
            : base( null, null )
        {
            _content = new CNode( overT, opener, partition, orderBy, rowOrRange, closer );
            CheckContent();
        }

        void CheckContent()
        {
            Helper.CheckToken( OverT, nameof( OverT ), SqlTokenType.Over );
            Helper.CheckNotNull( Opener, nameof( Opener ) );
            Helper.CheckNotNull( Closer, nameof( Closer ) );
        }

        SqlOverClause( SqlOverClause o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _content = o._content;
            else
            {
                _content = new CNode( items );
                CheckContent();
            }
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlOverClause( this, leading, content, trailing );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

        public SqlTokenIdentifier OverT => _content.V1;

        public SqlTokenOpenPar Opener => _content.V2;

        /// <summary>
        /// Gets the over partition by clause. May be null. 
        /// </summary>
        public SqlOverClausePartition Partition => _content.V3;

        /// <summary>
        /// Gets the over order by clause. May be null. 
        /// </summary>
        public SqlOrderByClause OrderBy => _content.V4;

        /// <summary>
        /// Gets the over row or range clause. May be null. 
        /// </summary>
        public SqlOverClauseRowOrRange RowOrRange => _content.V5;

        public SqlTokenClosePar Closer => _content.V6;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

    }

}
