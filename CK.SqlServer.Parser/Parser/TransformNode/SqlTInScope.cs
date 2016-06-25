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
            SqlTLocationFinder,
            SqlTokenIdentifier,
            SqlTStatementList,
            SqlTokenIdentifier,
            SqlTokenTerminal>;

    /// <summary>
    /// Replace a <see cref="SqlTLocationFinder"/> with an unparsed text.
    /// </summary>
    public sealed class SqlTInScope : SqlNonToken, ISqlTStatement
    {
        readonly CNode _content;

        public SqlTInScope( SqlTokenIdentifier inT, SqlTLocationFinder location, SqlTokenIdentifier beginT, SqlTStatementList statements, SqlTokenIdentifier endT, SqlTokenTerminal terminator )
            : base( null, null )
        {
            _content = new CNode( inT, location, beginT, statements, endT, terminator );
            CheckContent();
        }

        void CheckContent()
        {
            Helper.CheckToken( InT, nameof( InT ), SqlTokenType.In );
            Helper.CheckNotNull( Location, nameof( Location ) );
            Helper.CheckToken( BeginT, nameof( BeginT ), SqlTokenType.Begin );
            Helper.CheckNotNull( Statements, nameof( Statements ) );
            Helper.CheckToken( EndT, nameof( EndT ), SqlTokenType.End );
        }

        SqlTInScope( SqlTInScope o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
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
            return new SqlTInScope( this, leading, content, trailing );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

        public SqlTokenIdentifier InT => _content.V1;

        public SqlTLocationFinder Location => _content.V2;

        public SqlTokenIdentifier BeginT => _content.V3;

        public SqlTStatementList Statements => _content.V4;

        public SqlTokenIdentifier EndT => _content.V5;

        public SqlTokenTerminal StatementTerminator => _content.V6;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

    }

}
