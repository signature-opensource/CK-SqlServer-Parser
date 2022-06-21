using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CK.SqlServer.Parser
{

    public sealed class SqlOpenJSON : SqlNonTokenAutoWidth
    {
        readonly SNode<SqlTokenIdentifier, SqlEnclosedCommaList, SqlWithParOptions> _content;

        public SqlOpenJSON( SqlTokenIdentifier openJSON, SqlEnclosedCommaList parameters, SqlWithParOptions options, ImmutableList<SqlTrivia> leading = null, ImmutableList<SqlTrivia> trailing = null )
            : base( leading, trailing )
        {
            _content = new SNode<SqlTokenIdentifier, SqlEnclosedCommaList, SqlWithParOptions>( openJSON, parameters, options );
            CheckContent();
        }

        void CheckContent()
        {
            Helper.CheckToken( FunName, nameof( FunName ), SqlTokenType.OpenJSON );
        }

        SqlOpenJSON( SqlOpenJSON o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _content = o._content;
            else
            {
                _content = new SNode<SqlTokenIdentifier, SqlEnclosedCommaList, SqlWithParOptions>( items );
                CheckContent();
            }
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlOpenJSON( this, leading, content, trailing );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

        public SqlTokenIdentifier FunName => _content.V1;

        public SqlEnclosedCommaList Parameters => _content.V2;

        public bool HasSchema => _content.V3 != null;

        public SqlWithParOptions Schema => _content.V3;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

    }


}
