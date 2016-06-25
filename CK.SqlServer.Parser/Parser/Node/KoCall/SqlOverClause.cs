using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;

namespace CK.SqlServer.Parser
{
    using CNode = SNode<SqlTokenIdentifier, SqlTokenOpenPar, SqlNodeList, SqlTokenClosePar>;

    /// <summary>
    /// Captures a select column definition. 
    /// </summary>
    public sealed class SqlOverClause : SqlNonToken
    {
        readonly CNode _content;

        public SqlOverClause( SqlTokenIdentifier overT, SqlTokenOpenPar opener, SqlNodeList overExpression, SqlTokenClosePar closer )
            : base( null, null )
        {
            _content = new CNode( overT, opener, overExpression, closer );
            CheckContent();
        }

        void CheckContent()
        {
            Helper.CheckToken( OverT, nameof( OverT ), SqlTokenType.Over );
            Helper.CheckNotNull( Opener, nameof( Opener ) );
            Helper.CheckNotNull( OverContent, nameof( OverContent ) );
            Helper.CheckNotNull( Closer, nameof( Closer ) );
        }

        SqlOverClause( SqlOverClause o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
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
            return new SqlOverClause( this, leading, content, trailing );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

        public SqlTokenIdentifier OverT => _content.V1;

        public SqlTokenOpenPar Opener => _content.V2;

        public SqlNodeList OverContent => _content.V3;

        public SqlTokenClosePar Closer => _content.V4;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

    }

}
