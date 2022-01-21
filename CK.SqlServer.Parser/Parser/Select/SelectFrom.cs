using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CK.SqlServer.Parser
{
    /// <summary>
    /// Captures the optional "From ..." select part.
    /// </summary>
    public sealed class SelectFrom : SqlNonTokenAutoWidth
    {
        readonly SNode<SqlTokenIdentifier, ISqlNode> _content;

        public SelectFrom( SqlTokenIdentifier fromT, ISqlNode content )
            : base( null, null )
        {
            _content = new SNode<SqlTokenIdentifier, ISqlNode>( fromT, content );
            CheckContent();
        }

        void CheckContent()
        {
            Helper.CheckToken( FromT, nameof( FromT ), SqlTokenType.From );
            Helper.CheckNotNull( Content, nameof( Content ) );
        }

        SelectFrom( SelectFrom o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _content = o._content;
            else
            {
                _content = new SNode<SqlTokenIdentifier, ISqlNode>( items );
                CheckContent();
            }
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return new SelectFrom( this, leading, content, trailing );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

        public SqlTokenIdentifier FromT => _content.V1;
        
        public ISqlNode Content => _content.V2;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

    }


}
