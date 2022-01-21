using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CK.SqlServer.Parser
{
    /// <summary>
    /// Captures the optional "INTO table".
    /// </summary>
    public sealed class SelectInto : SqlNonTokenAutoWidth
    {
        readonly SNode<SqlTokenIdentifier, ISqlIdentifier> _content;

        public SelectInto( SqlTokenIdentifier intoToken, ISqlIdentifier tableName )
            : base( null, null )
        {
            _content = new SNode<SqlTokenIdentifier, ISqlIdentifier>( intoToken, tableName );
            CheckContent();
        }

        void CheckContent()
        {
            Helper.CheckToken( IntoT, nameof( IntoT ), SqlTokenType.Into );
            Helper.CheckNotNull( TableName, nameof( TableName ) );
        }

        SelectInto( SelectInto o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _content = o._content;
            else
            {
                _content = new SNode<SqlTokenIdentifier, ISqlIdentifier>( items );
                CheckContent();
            }
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return new SelectInto( this, leading, content, trailing );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

        public SqlTokenIdentifier IntoT => _content.V1;

        public ISqlIdentifier TableName => _content.V2;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

    }


}
