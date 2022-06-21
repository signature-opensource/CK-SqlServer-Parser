using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CK.SqlServer.Parser
{
    using CNode = SNode<ISqlNode, SqlTokenIdentifier, SqlTokenIdentifier, SqlTokenIdentifier, ISqlNode>;

    public sealed class SqlAtTimeZone : SqlNonTokenAutoWidth
    {
        readonly CNode _content;

        public SqlAtTimeZone( ISqlNode left, SqlTokenIdentifier atT, SqlTokenIdentifier timeT, SqlTokenIdentifier zoneT, ISqlNode timeZone )
            : base( null, null )
        {
            _content = new CNode( left, atT, timeT, zoneT, timeZone );
            CheckContent();
        }

        void CheckContent()
        {
            Helper.CheckNotNull( Left, nameof( Left ) );
            Helper.CheckToken( AtT, nameof( AtT ), SqlTokenType.At );
            Helper.CheckToken( TimeT, nameof( TimeT ), SqlTokenType.TimeDbType );
            Helper.CheckToken( ZoneT, nameof( ZoneT ), SqlTokenType.Zone );
            Helper.CheckNotNull( TimeZone, nameof( TimeZone ) );
        }

        SqlAtTimeZone( SqlAtTimeZone o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
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
            return new SqlAtTimeZone( this, leading, content, trailing );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

        public ISqlNode Left => _content.V1;

        public SqlTokenIdentifier AtT => _content.V2;

        public SqlTokenIdentifier TimeT => _content.V3;

        public SqlTokenIdentifier ZoneT => _content.V4;

        public ISqlNode TimeZone => _content.V5;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

    }

}
