using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CK.SqlServer.Parser
{
    using CNode = SNode<
            SqlTokenIdentifier,
            ISqlNode,
            ISqlNode,
            SqlTokenTerminal>;

    /// <summary>
    /// Create a scope around a <see cref="SqlTStatementList"/> or another <see cref="SqlTInScope"/> statement.
    /// </summary>
    public sealed class SqlTInScope : SqlNonTokenAutoWidth, ISqlTStatement
    {
        readonly CNode _content;

        /// <summary>
        /// Initializes a new <see cref="SqlTInScope"/>.
        /// </summary>
        /// <param name="inT">The 'in' token.</param>
        /// <param name="location">The location is a <see cref="ISqlTLocationFinder"/> or a <see cref="SqlTRangeLocationFinder"/>.</param>
        /// <param name="body">The transform statements (<see cref="SqlTStatementList"/>) or a subordinated <see cref="SqlTInScope"/>.</param>
        /// <param name="terminator">Optional ';' terminator.</param>
        public SqlTInScope( SqlTokenIdentifier inT, ISqlNode location, ISqlNode body, SqlTokenTerminal terminator )
            : base( null, null )
        {
            _content = new CNode( inT, location, body, terminator );
            CheckContent();
        }

        void CheckContent()
        {
            Helper.CheckToken( InT, nameof( InT ), SqlTokenType.In );
            Helper.CheckNotNull<ISqlTLocationFinder,SqlTRangeLocationFinder>( Location, nameof( Location ) );
            Helper.CheckNotNull<SqlTStatementList,SqlTInScope>( Body, nameof( Body ) );
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

        /// <summary>
        /// Gets the location that a is <see cref="ISqlTLocationFinder"/> or a <see cref="SqlTRangeLocationFinder"/>.
        /// </summary>
        public ISqlNode Location => _content.V2;

        public ISqlNode Body => _content.V3;

        public SqlTokenTerminal StatementTerminator => _content.V4;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

    }

}
