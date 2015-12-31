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

    public sealed class SqlCursorDefinition92 : SqlNode, ISqlCursorDefinition
    {
        readonly SNode<SqlTokenIdentifier,
            SqlTokenIdentifier,
            SqlTokenIdentifier,
            SqlTokenIdentifier,
            ISqlNode,
            SqlTokenIdentifier,
            SqlTokenIdentifier,
            SqlTokenIdentifier,
            SqlTokenIdentifier,
            SqlTokenIdentifier,
            SqlIdentifierCommaList> _content;

        public SqlCursorDefinition92(
            SqlTokenIdentifier insensitiveOrScrollToken,
            SqlTokenIdentifier scrollOrInsensitiveToken,
            SqlTokenIdentifier cursorToken,
            SqlTokenIdentifier forToken,
            ISqlNode selectNode,
            SqlTokenIdentifier forOptionsToken,
            SqlTokenIdentifier readToken,
            SqlTokenIdentifier onlyToken,
            SqlTokenIdentifier updateToken,
            SqlTokenIdentifier ofToken,
            SqlIdentifierCommaList updateColumns )
            : base( null, null )
        {
            _content = new SNode<SqlTokenIdentifier, SqlTokenIdentifier, SqlTokenIdentifier, SqlTokenIdentifier, ISqlNode, SqlTokenIdentifier, SqlTokenIdentifier, SqlTokenIdentifier, SqlTokenIdentifier, SqlTokenIdentifier, SqlIdentifierCommaList>(
                insensitiveOrScrollToken, 
                scrollOrInsensitiveToken, 
                cursorToken, 
                forToken, 
                selectNode, 
                forOptionsToken, 
                readToken, 
                onlyToken, 
                updateToken, 
                ofToken, 
                updateColumns );
            CheckContent();
        }

        void CheckContent()
        {
            SNode.CheckNullableToken( InsensitiveOrScrollT, nameof( InsensitiveOrScrollT ), SqlTokenType.Insensitive, SqlTokenType.Scroll );
            SNode.CheckNullableToken( ScrollOrInsensitiveT, nameof( ScrollOrInsensitiveT ), SqlTokenType.Insensitive, SqlTokenType.Scroll );
            SNode.CheckToken( CursorT, nameof( CursorT ), SqlTokenType.Cursor );
            SNode.CheckToken( ForT, nameof( ForT ), SqlTokenType.For );
            SNode.CheckUnPar<ISelectSpecification>( SelectNode, nameof( Select ) );
            SNode.CheckNullableToken( ForOptionsT, nameof( ForOptionsT ), SqlTokenType.For );
            if( ForOptionsT != null )
            {
                SNode.CheckNullableToken( ReadT, nameof( ReadT ), SqlTokenType.Read );
                SNode.CheckNullableToken( OnlyT, nameof( OnlyT ), SqlTokenType.Only );
                SNode.CheckBothNullOrNot( ReadT, nameof( ReadT ), OnlyT, nameof( OnlyT ) );
                SNode.CheckNullableToken( UpdateT, nameof( UpdateT ), SqlTokenType.Update );
                SNode.CheckXORNull( ReadT, nameof( ReadT ), UpdateT, nameof( UpdateT ) );
                SNode.CheckBothNullOrNot( ForOptionsT, nameof( ForOptionsT ), UpdateT, nameof( UpdateT ) );
                SNode.CheckNullableToken( OfT, nameof( OfT ), SqlTokenType.Of );
                SNode.CheckBothNullOrNot( OfT, nameof( OfT ), UpdateColumns, nameof( UpdateColumns ) );
            }
            else
            {
                SNode.CheckNull( ReadT, nameof( ReadT ) );
                SNode.CheckNull( OnlyT, nameof( OnlyT ) );
                SNode.CheckNull( UpdateT, nameof( UpdateT ) );
                SNode.CheckNull( OfT, nameof( OfT ) );
                SNode.CheckNull( UpdateT, nameof( UpdateT ) );
            }
        }

        SqlCursorDefinition92( SqlCursorDefinition92 o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _content = o._content;
            else
            {
                _content = new SNode<SqlTokenIdentifier, SqlTokenIdentifier, SqlTokenIdentifier, SqlTokenIdentifier, ISqlNode, SqlTokenIdentifier, SqlTokenIdentifier, SqlTokenIdentifier, SqlTokenIdentifier, SqlTokenIdentifier, SqlIdentifierCommaList>( items );
                CheckContent();
            }
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlCursorDefinition92( this, leading, children, trailing );
        }

        public bool IsSql92Syntax => true;

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public SqlTokenIdentifier InsensitiveOrScrollT => _content.V1;

        public SqlTokenIdentifier ScrollOrInsensitiveT => _content.V2;

        public SqlTokenIdentifier CursorT => _content.V3;

        public SqlTokenIdentifier ForT => _content.V4;

        public ISqlNode SelectNode => _content.V5;

        public ISelectSpecification Select => (ISelectSpecification)_content.V5.UnPar;

        public SqlTokenIdentifier ForOptionsT => _content.V6;

        public SqlTokenIdentifier ReadT => _content.V7;

        public SqlTokenIdentifier OnlyT => _content.V8;

        public SqlTokenIdentifier UpdateT => _content.V9;

        public SqlTokenIdentifier OfT => _content.V10;

        public SqlIdentifierCommaList UpdateColumns => _content.V11;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor )
        {
            return visitor.Visit( this );
        }

    }


}
