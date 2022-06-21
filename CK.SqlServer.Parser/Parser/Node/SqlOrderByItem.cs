using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CK.SqlServer.Parser
{
    public sealed class SqlOrderByItem : SqlNonTokenAutoWidth
    {
        readonly SNode<ISqlNode, SqlTokenIdentifier> _content;

        public SqlOrderByItem( ISqlNode definition, SqlTokenIdentifier ascOrDesc = null )
            : base( null, null )
        {
            _content = new SNode<ISqlNode, SqlTokenIdentifier>( definition, ascOrDesc );
            CheckContent();
        }

        void CheckContent()
        {
            Helper.CheckNotNull( Definition, nameof( Definition ) );
            Helper.CheckNullableToken( AscOrDescT, nameof( AscOrDescT ), SqlTokenType.Asc, SqlTokenType.Desc );
        }

        SqlOrderByItem( SqlOrderByItem o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _content = o._content;
            else
            {
                _content = new SNode<ISqlNode, SqlTokenIdentifier>( items );
                CheckContent();
            }
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlOrderByItem( this, leading, content, trailing );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

        public ISqlNode Definition => _content.V1;

        /// <summary>
        /// Gets the 'asc' or 'desc' token. Null if not specified.
        /// </summary>
        public SqlTokenIdentifier AscOrDescT => _content.V2;

        /// <summary>
        /// True if the <see cref="AscOrDescT"/> is not specified or if it is 'asc'.
        /// </summary>
        public bool IsAsc => _content.V2 == null || _content.V2.IsToken( SqlTokenType.Asc );
        
        /// <summary>
        /// True id 'desc' is specified.
        /// </summary>
        public bool IsDesc => _content.V2 != null && _content.V2.IsToken( SqlTokenType.Desc );

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

    }
}
