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
    public sealed class SqlCursorDefinition : SqlNode, ISqlCursorDefinition
    {
        readonly SNode<
                    SqlTokenIdentifier,
                    SqlNodeList,
                    SqlTokenIdentifier,
                    ISqlNode,
                    SqlTokenIdentifier,
                    SqlTokenIdentifier,
                    SqlTokenIdentifier,
                    SqlIdentifierCommaList> _content;

        public SqlCursorDefinition( 
            SqlTokenIdentifier cursorT,
            SqlNodeList options, 
            SqlTokenIdentifier forT,
            ISqlNode selectNode, 
            SqlTokenIdentifier forOptionsT, 
            SqlTokenIdentifier updateT, 
            SqlTokenIdentifier ofT, 
            SqlIdentifierCommaList updateColumns )
            : base( null, null )
        {
            _content = new SNode<SqlTokenIdentifier, SqlNodeList, SqlTokenIdentifier, ISqlNode, SqlTokenIdentifier, SqlTokenIdentifier, SqlTokenIdentifier, SqlIdentifierCommaList>(
                cursorT, 
                options, 
                forT, 
                selectNode, 
                forOptionsT, 
                updateT, 
                ofT, 
                updateColumns );
            CheckContent();
        }

        void CheckContent()
        {
            SNode.CheckToken( CursorT, nameof( CursorT ), SqlTokenType.Cursor );
            SNode.CheckToken( ForT, nameof( ForT ), SqlTokenType.For );
            SNode.CheckUnPar<ISelectSpecification>( SelectNode, nameof( Select ) );
            SNode.CheckNullableToken( ForOptionsT, nameof( ForOptionsT ), SqlTokenType.For );
            SNode.CheckNullableToken( UpdateT, nameof( UpdateT ), SqlTokenType.Update );
            SNode.CheckBothNullOrNot( ForOptionsT, nameof( ForOptionsT ), UpdateT, nameof( UpdateT ) );
            SNode.CheckNullableToken( OfT, nameof( OfT ), SqlTokenType.Of );
            SNode.CheckBothNullOrNot( OfT, nameof( OfT ), UpdateColumns, nameof( UpdateColumns ) );
        }

        SqlCursorDefinition( SqlCursorDefinition o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _content = o._content;
            else
            {
                _content = new SNode<SqlTokenIdentifier, SqlNodeList, SqlTokenIdentifier, ISqlNode, SqlTokenIdentifier, SqlTokenIdentifier, SqlTokenIdentifier, SqlIdentifierCommaList>( items );
                CheckContent();
            }
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlCursorDefinition( this, leading, children, trailing );
        }

        public bool IsSql92Syntax => false;

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public SqlTokenIdentifier CursorT => _content.V1;

        /// <summary>
        /// Gets the options (null if none). 
        /// Can be [LOCAL|GLOBAL][FORWARD_ONLY|SCROLL][STATIC|KEYSET|DYNAMIC|FAST_FORWARD][READ_ONLY|SCROLL_LOCKS|OPTIMISTIC][TYPE_WARNING]. 
        /// </summary>
        public SqlNodeList Options => _content.V2;

        public SqlTokenIdentifier ForT => _content.V3;

        /// <summary>
        /// Gets the select specification for this cursor.
        /// </summary>
        public ISqlNode SelectNode => _content.V4;

        public ISelectSpecification Select => (ISelectSpecification)_content.V4.UnPar;

        public SqlTokenIdentifier ForOptionsT => _content.V5;

        public SqlTokenIdentifier UpdateT => _content.V6;

        public SqlTokenIdentifier OfT => _content.V7;

        public SqlIdentifierCommaList UpdateColumns => _content.V8;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor ) => visitor.Visit( this );

    }


}
