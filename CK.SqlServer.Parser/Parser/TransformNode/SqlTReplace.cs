using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CK.SqlServer.Parser
{
    using CNode = SNode<
            SqlTokenIdentifier,
            ISqlTLocationFinder,
            SqlTokenIdentifier,
            ISqlHasStringValue,
            SqlTokenTerminal>;

    /// <summary>
    /// Replace a <see cref="ISqlTLocationFinder"/> with an unparsed text.
    /// </summary>
    public sealed class SqlTReplace : SqlNonTokenAutoWidth, ISqlTStatement
    {
        readonly CNode _content;

        public SqlTReplace( SqlTokenIdentifier replaceT,
                            ISqlTLocationFinder location,
                            SqlTokenIdentifier withT,
                            ISqlHasStringValue content,
                            SqlTokenTerminal terminator )
            : base( null, null )
        {
            _content = new CNode( replaceT, location, withT, content, terminator );
            CheckContent();
        }

        void CheckContent()
        {
            Helper.CheckToken( ReplaceT, nameof( ReplaceT ), SqlTokenType.Replace );
            Helper.CheckNotNull( Location, nameof( Location ) );
            Helper.CheckToken( WithT, nameof( WithT ), SqlTokenType.With );
            Helper.CheckNotNull( Content, nameof( Content ) );
        }

        SqlTReplace( SqlTReplace o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
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
            return new SqlTReplace( this, leading, content, trailing );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

        public SqlTokenIdentifier ReplaceT => _content.V1;

        public ISqlTLocationFinder Location => _content.V2;

        public SqlTokenIdentifier WithT => _content.V3;

        public ISqlHasStringValue Content => _content.V4;

        public SqlTokenTerminal StatementTerminator => _content.V5;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

    }

}
