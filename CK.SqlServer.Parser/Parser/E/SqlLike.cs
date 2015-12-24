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
    public sealed class SqlLike : SqlNode
    {
        readonly SNode<ISqlNode, SqlTokenIdentifier, SqlTokenIdentifier, ISqlNode, SqlTokenIdentifier, SqlTokenLiteralString> _content;

        public SqlLike( ISqlNode left, SqlTokenIdentifier notToken, SqlTokenIdentifier likeToken, ISqlNode pattern, SqlTokenIdentifier escapeToken = null, SqlTokenLiteralString escapeChar = null )
            : base( null, null )
        {
            _content = new SNode<ISqlNode, SqlTokenIdentifier, SqlTokenIdentifier, ISqlNode, SqlTokenIdentifier, SqlTokenLiteralString>( left, notToken, likeToken, pattern, escapeToken, escapeChar );
            CheckContent();
        }

        void CheckContent()
        {
            SNode.CheckNotNull( Left, nameof( Left ) );
            SNode.CheckNullableToken( NotT, nameof( NotT ), SqlTokenType.Not );
            SNode.CheckToken( LikeT, nameof( LikeT ), SqlTokenType.Like );
            SNode.CheckNotNull( Pattern, nameof( Pattern ) );
            SNode.CheckNullableToken( EscapeT, nameof( EscapeT ), SqlTokenType.Escape );
            SNode.CheckBothNullOrNot( EscapeT, nameof( EscapeT ), EscapeChar, nameof(EscapeChar) );
        }

        SqlLike( SqlLike o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _content = o._content;
            else
            {
                _content = new SNode<ISqlNode, SqlTokenIdentifier, SqlTokenIdentifier, ISqlNode, SqlTokenIdentifier, SqlTokenLiteralString>( items );
                CheckContent();
            }
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IReadOnlyList<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlLike( this, leading, children, trailing );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public ISqlNode Left => _content.V1;

        public bool IsNotLike => _content.V2 != null;

        public SqlTokenIdentifier NotT => _content.V2;

        public SqlTokenIdentifier LikeT => _content.V3;

        public ISqlNode Pattern => _content.V4;

        public bool HasEscape => _content.V5 != null;

        public SqlTokenIdentifier EscapeT => _content.V5;

        public SqlTokenLiteralString EscapeChar => _content.V6;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor ) => visitor.Visit( this );

    }


}
