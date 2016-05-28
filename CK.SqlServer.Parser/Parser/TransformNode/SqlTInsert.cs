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
    using CNode = SNode<
            SqlTokenIdentifier,
            SqlTokenTerminal,
            ISqlNode,
            SqlTokenTerminal,
            SqlTokenIdentifier,
            SqlTLocationSelector,
            SqlTokenTerminal>;

    /// <summary>
    /// insert (raw|statement) I
    /// </summary>
    public sealed class SqlTInsert : SqlNonToken, ISqlTStatement
    {
        readonly CNode _content;

        public SqlTInsert( SqlTokenIdentifier insertT, SqlTokenTerminal opener, ISqlNode content, SqlTokenTerminal closer, SqlTokenIdentifier afterOrBeforeT, SqlTLocationSelector location, SqlTokenTerminal terminator )
            : base( null, null )
        {
            _content = new CNode( insertT, opener, content, closer, afterOrBeforeT, location, terminator );
            CheckContent();
        }

        void CheckContent()
        {
            Helper.CheckToken( InsertT, nameof( InsertT ), SqlTokenType.Insert );
            Helper.CheckToken( Opener, nameof( Opener ), SqlTokenType.OpenCurly );
            Helper.CheckNotNull( Content, nameof( Content ) );
            Helper.CheckToken( Closer, nameof( Closer ), SqlTokenType.CloseCurly );
            Helper.CheckToken( AfterOrBeforeT, nameof( AfterOrBeforeT ), SqlTokenType.After, SqlTokenType.Before );
            Helper.CheckNotNull( Location, nameof( Location ) );
        }

        SqlTInsert( SqlTInsert o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
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
            return new SqlTInsert( this, leading, content, trailing );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

        public SqlTokenIdentifier InsertT => _content.V1;

        public SqlTokenTerminal  Opener => _content.V2;

        public ISqlNode Content => _content.V3;

        public string TextContent => Content is ISqlHasStringValue ? ((ISqlHasStringValue)Content).Value : Content.ToString( true, true );

        public SqlTokenTerminal Closer => _content.V4;

        public SqlTokenIdentifier AfterOrBeforeT => _content.V5;

        /// <summary>
        /// Gets whether this is "before...".  Otherwise it is "after...".
        /// </summary>
        public bool IsBefore => AfterOrBeforeT.TokenType == SqlTokenType.Before;

        public SqlTLocationSelector Location => _content.V6;

        public SqlTokenTerminal StatementTerminator => _content.V7;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

    }

}
