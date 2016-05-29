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
            SqlTCurlyContent,
            SqlTokenIdentifier,
            SqlTCurlyContent,
            SqlTokenIdentifier,
            SqlTLocationFinder,
            SqlTokenTerminal>;

    /// <summary>
    /// Injects unparsed text around, before or after a <see cref="SqlTLocationFinder"/>.
    /// </summary>
    public sealed class SqlTInject : SqlNonToken, ISqlTStatement
    {
        readonly CNode _content;

        public SqlTInject( SqlTokenIdentifier insertT, SqlTCurlyContent content, SqlTokenIdentifier andT, SqlTCurlyContent content2, SqlTokenIdentifier afterBeforeOrAroundT, SqlTLocationFinder location, SqlTokenTerminal terminator )
            : base( null, null )
        {
            _content = new CNode( insertT, content, andT, content2, afterBeforeOrAroundT, location, terminator );
            CheckContent();
        }

        void CheckContent()
        {
            Helper.CheckToken( InjectT, nameof( InjectT ), SqlTokenType.Inject );
            Helper.CheckNotNull( Content, nameof( Content ) );
            Helper.CheckNullableToken( AndT, nameof( AndT ), SqlTokenType.And );
            Helper.CheckBothNullOrNot( AndT, nameof( AndT ), Content2, nameof(Content2) );
            Helper.CheckToken( AfterBeforeOrAroundT, nameof( AfterBeforeOrAroundT ), SqlTokenType.After, SqlTokenType.Before, SqlTokenType.Around );
            Helper.CheckNotNull( Location, nameof( Location ) );
        }

        SqlTInject( SqlTInject o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
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
            return new SqlTInject( this, leading, content, trailing );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

        public SqlTokenIdentifier InjectT => _content.V1;

        public SqlTCurlyContent Content => _content.V2;

        public SqlTokenIdentifier AndT => _content.V3;

        public SqlTCurlyContent Content2 => _content.V4;

        public string TextBefore => _content.V5.TokenType != SqlTokenType.After
                                        ? Content.ContentString
                                        : null;

        public string TextAfter => _content.V5.TokenType == SqlTokenType.After
                                        ? Content.ContentString
                                        : Content2?.ContentString;

        public SqlTokenIdentifier AfterBeforeOrAroundT => _content.V5;

        public SqlTLocationFinder Location => _content.V6;

        public SqlTokenTerminal StatementTerminator => _content.V7;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

    }

}
