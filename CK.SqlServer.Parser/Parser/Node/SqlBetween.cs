using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using CK.Core;
using System.Collections.Immutable;

namespace CK.SqlServer.Parser
{
  
    /// <summary>
    /// 
    /// </summary>
    public sealed class SqlBetween : SqlNode
    {
        readonly SNode<ISqlNode, SqlTokenIdentifier, SqlTokenIdentifier, ISqlNode, SqlTokenIdentifier, ISqlNode> _content;

        public SqlBetween( ISqlNode left, SqlTokenIdentifier notT, SqlTokenIdentifier betweenT, ISqlNode start, SqlTokenIdentifier andT, ISqlNode stop )
            : base( null, null )
        {
            _content = new SNode<ISqlNode, SqlTokenIdentifier, SqlTokenIdentifier, ISqlNode, SqlTokenIdentifier, ISqlNode>(
                left,
                notT,
                betweenT,
                start,
                andT,
                stop );
            CheckContent();
        }

        SqlBetween( SqlBetween o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _content = o._content;
            else
            {
                _content = new SNode<ISqlNode, SqlTokenIdentifier, SqlTokenIdentifier, ISqlNode, SqlTokenIdentifier, ISqlNode>( items );
                CheckContent();
            }
        }

        void CheckContent()
        {
            SNode.CheckNotNull( Left, nameof( Left ) );
            SNode.CheckNullableToken( NotT, nameof( NotT ), SqlTokenType.Not );
            SNode.CheckToken( BetweenT, nameof( BetweenT ), SqlTokenType.Between );
            SNode.CheckNotNull( Start, nameof( Start ) );
            SNode.CheckToken( AndT, nameof( AndT ), SqlTokenType.And );
            SNode.CheckNotNull( Stop, nameof( Stop ) );
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IReadOnlyList<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlBetween( this, leading, children, trailing );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public ISqlNode Left => _content.V1;

        public bool IsNotBetween => _content.V2 != null;

        public SqlTokenIdentifier NotT => _content.V2;

        public SqlTokenIdentifier BetweenT => _content.V3;

        public ISqlNode Start => _content.V4;

        public SqlTokenIdentifier AndT => _content.V5;

        public ISqlNode Stop => _content.V6;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor ) => visitor.Visit( this );

    }


}
