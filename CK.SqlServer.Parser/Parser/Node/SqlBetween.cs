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
            Helper.CheckNotNull( Left, nameof( Left ) );
            Helper.CheckNullableToken( NotT, nameof( NotT ), SqlTokenType.Not );
            Helper.CheckToken( BetweenT, nameof( BetweenT ), SqlTokenType.Between );
            Helper.CheckNotNull( Start, nameof( Start ) );
            Helper.CheckToken( AndT, nameof( AndT ), SqlTokenType.And );
            Helper.CheckNotNull( Stop, nameof( Stop ) );
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlBetween( this, leading, content, trailing );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

        public ISqlNode Left => _content.V1;

        public bool IsNotBetween => _content.V2 != null;

        public SqlTokenIdentifier NotT => _content.V2;

        public SqlTokenIdentifier BetweenT => _content.V3;

        public ISqlNode Start => _content.V4;

        public SqlTokenIdentifier AndT => _content.V5;

        public ISqlNode Stop => _content.V6;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

    }


}
