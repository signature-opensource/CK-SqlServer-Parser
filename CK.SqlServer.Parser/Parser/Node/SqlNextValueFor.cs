using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CK.SqlServer.Parser
{
    using CNode = SNode<SqlTokenIdentifier, SqlTokenIdentifier, SqlTokenIdentifier, ISqlIdentifier, SqlOverClause>;

    /// <summary>
    /// Defines "next value for {sequence}>" expression.
    /// </summary>
    public sealed class SqlNextValueFor : SqlNonTokenAutoWidth
    {
        readonly CNode _content;

        public SqlNextValueFor( SqlTokenIdentifier nextT, SqlTokenIdentifier valueT, SqlTokenIdentifier forT, ISqlIdentifier seqName, SqlOverClause overClause )
            : base( null, null )
        {
            _content = new CNode( nextT, valueT, forT, seqName, overClause );
            CheckContent();
        }

        void CheckContent()
        {
            Helper.CheckToken( NextT, nameof( NextT ), SqlTokenType.Next );
            Helper.CheckToken( ValueT, nameof( ValueT ), SqlTokenType.Value );
            Helper.CheckToken( ForT, nameof( ForT ), SqlTokenType.For );
            Helper.CheckNotNull( SequenceName, nameof( SequenceName ) );
        }

        SqlNextValueFor( SqlNextValueFor o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
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
            return new SqlNextValueFor( this, leading, content, trailing );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

        public SqlTokenIdentifier NextT => _content.V1;

        public SqlTokenIdentifier ValueT => _content.V2;

        public SqlTokenIdentifier ForT => _content.V3;

        public ISqlIdentifier SequenceName=> _content.V4;

        public SqlOverClause OverClause => _content.V5;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

    }


}
