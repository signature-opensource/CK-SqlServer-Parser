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
    using CNode = SNode<SqlTokenIdentifier,
            SqlTokenIdentifier,
            SqlTokenIdentifier,
            SqlTokenIdentifier,
            ISqlNode,
            SqlTokenIdentifier,
            SqlTokenIdentifier,
            SqlTokenIdentifier,
            SqlTokenIdentifier,
            SqlTokenIdentifier,
            SqlIdentifierCommaList>;

    public sealed class SqlCursorDefinition92 : SqlNonTokenAutoWidth, ISqlCursorDefinition
    {
        readonly CNode _content;

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
            _content = new CNode(
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
            Helper.CheckNullableToken( InsensitiveOrScrollT, nameof( InsensitiveOrScrollT ), SqlTokenType.Insensitive, SqlTokenType.Scroll );
            Helper.CheckNullableToken( ScrollOrInsensitiveT, nameof( ScrollOrInsensitiveT ), SqlTokenType.Insensitive, SqlTokenType.Scroll );
            Helper.CheckToken( CursorT, nameof( CursorT ), SqlTokenType.Cursor );
            Helper.CheckToken( ForT, nameof( ForT ), SqlTokenType.For );
            Helper.CheckUnPar<ISelectSpecification>( SelectNode, nameof( Select ) );
            Helper.CheckNullableToken( ForOptionsT, nameof( ForOptionsT ), SqlTokenType.For );
            if( ForOptionsT != null )
            {
                Helper.CheckNullableToken( ReadT, nameof( ReadT ), SqlTokenType.Read );
                Helper.CheckNullableToken( OnlyT, nameof( OnlyT ), SqlTokenType.Only );
                Helper.CheckBothNullOrNot( ReadT, nameof( ReadT ), OnlyT, nameof( OnlyT ) );
                Helper.CheckNullableToken( UpdateT, nameof( UpdateT ), SqlTokenType.Update );
                Helper.CheckXORNull( ReadT, nameof( ReadT ), UpdateT, nameof( UpdateT ) );
                Helper.CheckBothNullOrNot( ForOptionsT, nameof( ForOptionsT ), UpdateT, nameof( UpdateT ) );
                Helper.CheckNullableToken( OfT, nameof( OfT ), SqlTokenType.Of );
                Helper.CheckBothNullOrNot( OfT, nameof( OfT ), UpdateColumns, nameof( UpdateColumns ) );
            }
            else
            {
                Helper.CheckNull( ReadT, nameof( ReadT ) );
                Helper.CheckNull( OnlyT, nameof( OnlyT ) );
                Helper.CheckNull( UpdateT, nameof( UpdateT ) );
                Helper.CheckNull( OfT, nameof( OfT ) );
                Helper.CheckNull( UpdateT, nameof( UpdateT ) );
            }
        }

        SqlCursorDefinition92( SqlCursorDefinition92 o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _content = o._content;
            else
            {
                _content = new CNode( items );
                CheckContent();
            }
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlCursorDefinition92( this, leading, content, trailing );
        }

        public bool IsSql92Syntax => true;

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

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
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

    }


}
