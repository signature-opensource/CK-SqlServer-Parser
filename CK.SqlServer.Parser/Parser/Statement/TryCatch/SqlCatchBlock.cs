using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CK.SqlServer.Parser
{
    using CNode = SNode<SqlTokenIdentifier, SqlTokenIdentifier, SqlStatementList, SqlTokenIdentifier, SqlTokenIdentifier>;

    /// <summary>
    /// The 'begin catch' ... 'end catch' block of <see cref="SqlTryCatch"/> statement.
    /// </summary>
    public sealed class SqlCatchBlock : SqlNonTokenAutoWidth, ISqlStatementPart
    {
        readonly CNode _content;

        public SqlCatchBlock( SqlTokenIdentifier beginT, SqlTokenIdentifier catchT,
                              SqlStatementList body, 
                              SqlTokenIdentifier endT, SqlTokenIdentifier endCatchT )
            : base( null, null )
        {
            _content = new CNode( beginT, catchT, body, endT, endCatchT );
            CheckContent();
        }

        SqlCatchBlock( SqlCatchBlock o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _content = o._content;
            else
            {
                _content = new CNode( items );
                CheckContent();
            }
        }

        void CheckContent()
        {
            Helper.CheckToken( BeginT, nameof( BeginT ), SqlTokenType.Begin );
            Helper.CheckToken( CatchT, nameof( CatchT ), SqlTokenType.Catch );

            Helper.CheckNotNull( Body, nameof( Body ) );

            Helper.CheckToken( EndT, nameof( EndT ), SqlTokenType.End );
            Helper.CheckToken( EndCatchT, nameof( EndCatchT ), SqlTokenType.Catch );
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlCatchBlock( this, leading, content, trailing );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

        public SqlTokenIdentifier BeginT => _content.V1;

        public SqlTokenIdentifier CatchT => _content.V2;

        public SqlStatementList Body => _content.V3;

        public SqlTokenIdentifier EndT => _content.V4;

        public SqlTokenIdentifier EndCatchT => _content.V5;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

    }


}
