using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Parser
{
    public sealed class SqlOrderByItem : SqlNode
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
            SNode.CheckNotNull( Definition, nameof( Definition ) );
            SNode.CheckNullableToken( AscOrDescT, nameof( AscOrDescT ), SqlTokenType.Asc, SqlTokenType.Desc );
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

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IReadOnlyList<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlOrderByItem( this, leading, children, trailing );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

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
        internal protected override ISqlNode Accept( SqlItemVisitor visitor ) => visitor.Visit( this );

    }
}