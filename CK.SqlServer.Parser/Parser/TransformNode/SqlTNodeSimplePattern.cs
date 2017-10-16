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
    using CNode = SNode<SqlTokenIdentifier, SqlTCurlyPattern>;

    /// <summary>
    /// [part|statement|range] SqlTCurlyPattern
    /// Defaults to part.
    /// </summary>
    public sealed class SqlTNodeSimplePattern : SqlNonTokenAutoWidth
    {
        readonly CNode _content;

        public SqlTNodeSimplePattern( SqlTokenIdentifier matchKindT, SqlTCurlyPattern pattern )
            : base( null, null )
        {
            _content = new CNode( matchKindT, pattern );
            CheckContent();
        }

        void CheckContent()
        {
            Helper.CheckNullableToken( MatchKindT, nameof( MatchKindT ), SqlTokenType.Part, SqlTokenType.Statement, SqlTokenType.Range );
            Helper.CheckNotNull( Pattern, nameof( Pattern ) );
        }

        SqlTNodeSimplePattern( SqlTNodeSimplePattern o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
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
            return new SqlTNodeSimplePattern( this, leading, content, trailing );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

        /// <summary>
        /// Gets the optional 'statement', 'part' or 'range' token.
        /// When null, <see cref="IsMatchRange"/> is true: the default is to match the raw range. 
        /// </summary>
        public SqlTokenIdentifier MatchKindT => _content.V1;

        /// <summary>
        /// Gets whether this pattern must match well known parts.
        /// </summary>
        public bool IsMatchPart => _content.V1 != null && _content.V1.TokenType == SqlTokenType.Part;

        /// <summary>
        /// Gets whether this pattern must match statements.
        /// </summary>
        public bool IsMatchStatement => _content.V1 != null && _content.V1.TokenType == SqlTokenType.Statement;

        /// <summary>
        /// Gets whether this pattern must match ranges.
        /// Thisi is the default.
        /// </summary>
        public bool IsMatchRange => _content.V1 == null || _content.V1.TokenType == SqlTokenType.Range;

        public SqlTCurlyPattern Pattern => _content.V2;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

    }

}
