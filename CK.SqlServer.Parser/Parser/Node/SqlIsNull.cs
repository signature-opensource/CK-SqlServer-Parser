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
    public class SqlIsNull : SqlNode
    {
        readonly SNode<ISqlNode, SqlTokenIdentifier, SqlTokenIdentifier, SqlTokenIdentifier> _content;

        public SqlIsNull( ISqlNode left, SqlTokenIdentifier isT, SqlTokenIdentifier notT, SqlTokenIdentifier nullT )
            : base( null, null )
        {
            _content = new SNode<ISqlNode, SqlTokenIdentifier, SqlTokenIdentifier, SqlTokenIdentifier>( left, isT, notT, nullT );
            CheckContent();
        }

        void CheckContent()
        {
            Helper.CheckNotNull( Left, nameof(Left) );
            Helper.CheckToken( IsT, nameof( IsT ), SqlTokenType.Is );
            Helper.CheckNullableToken( NotT, nameof( NotT ), SqlTokenType.Not );
            Helper.CheckToken( NullT, nameof( NullT ), SqlTokenType.Null );
        }

        SqlIsNull( SqlIsNull o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _content = o._content;
            else
            {
                _content = new SNode<ISqlNode, SqlTokenIdentifier, SqlTokenIdentifier, SqlTokenIdentifier>( items );
                CheckContent();
            }
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlIsNull( this, leading, content, trailing );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

        public ISqlNode Left => _content.V1;

        public SqlTokenIdentifier IsT => _content.V2;

        public bool IsNotNull => _content.V3 != null;

        public SqlTokenIdentifier NotT => _content.V3;

        public SqlTokenIdentifier NullT => _content.V4;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

    }


}
