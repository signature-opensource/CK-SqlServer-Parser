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
    /// insert (raw|statement) I
    /// </summary>
    public sealed class SqlTInsert : SqlNonToken, ISqlTransformStatement
    {
        readonly SNode<
            SqlTokenIdentifier,
            SqlTokenOpenPar,
            ISqlNode,
            SqlTokenClosePar,
            SqlTLocation,
            SqlTokenTerminal> _content;

        public SqlTInsert( SqlTokenIdentifier insertT, SqlTokenOpenPar openPar, ISqlNode content, SqlTokenClosePar closePar, SqlTLocation location, SqlTokenTerminal terminator )
            : base( null, null )
        {
            _content = new SNode<SqlTokenIdentifier, SqlTokenOpenPar, ISqlNode, SqlTokenClosePar, SqlTLocation, SqlTokenTerminal>( insertT, openPar, content, closePar, location, terminator );
            CheckContent();
        }

        void CheckContent()
        {
            Helper.CheckToken( InsertT, nameof( InsertT ), SqlTokenType.Insert );
            Helper.CheckNotNull( OpenPar, nameof( OpenPar ) );
            Helper.CheckNotNull( Content, nameof( Content ) );
            Helper.CheckNotNull( ClosePar, nameof( ClosePar ) );
            Helper.CheckNotNull( Location, nameof( Location ) );
        }

        SqlTInsert( SqlTInsert o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _content = o._content;
            else
            {
                _content = new SNode<SqlTokenIdentifier, SqlTokenOpenPar, ISqlNode, SqlTokenClosePar, SqlTLocation, SqlTokenTerminal>( items );
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

        public SqlTokenOpenPar OpenPar => _content.V2;

        public ISqlNode Content => _content.V3;

        public string TextContent => Content is ISqlHasStringValue ? ((ISqlHasStringValue)Content).Value : Content.ToString( true, true );

        public SqlTokenClosePar ClosePar => _content.V4;

        public SqlTLocation Location => _content.V5;

        public SqlTokenTerminal StatementTerminator => _content.V6;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

    }

}
