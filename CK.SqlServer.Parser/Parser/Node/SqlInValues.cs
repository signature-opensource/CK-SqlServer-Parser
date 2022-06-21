using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CK.SqlServer.Parser
{

    public class SqlInValues : SqlNonTokenAutoWidth
    {
        readonly SNode<ISqlNode, SqlTokenIdentifier, SqlTokenIdentifier, SqlEnclosedCommaList> _content;

        public SqlInValues( ISqlNode left, SqlTokenIdentifier notT, SqlTokenIdentifier inT, SqlEnclosedCommaList values )
            : base( null, null )
        {
            _content = new SNode<ISqlNode, SqlTokenIdentifier, SqlTokenIdentifier, SqlEnclosedCommaList>( left, notT, inT, values );
            CheckContent();
        }

        void CheckContent()
        {
            Helper.CheckNotNull( Left, nameof( Left ) );
            Helper.CheckToken( InT, nameof( InT ), SqlTokenType.In );
            Helper.CheckNotNull( Values, nameof( Values ) );
        }

        SqlInValues( SqlInValues o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _content = o._content;
            else
            {
                _content = new SNode<ISqlNode, SqlTokenIdentifier, SqlTokenIdentifier, SqlEnclosedCommaList>( items );
                CheckContent();
            }
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlInValues( this, leading, content, trailing );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

        public ISqlNode Left => _content.V1;

        public bool IsNotIn => _content.V2 != null;

        public SqlTokenIdentifier NotT => _content.V2;

        public SqlTokenIdentifier InT => _content.V3;

        public SqlEnclosedCommaList Values => _content.V4;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

    }


}
