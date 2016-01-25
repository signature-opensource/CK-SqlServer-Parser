using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Parser
{
    public sealed class SqlPar : SqlNode
    {
        readonly SNode<SqlTokenOpenPar, ISqlNode, SqlTokenClosePar> _content;

        public SqlPar( SqlTokenOpenPar opener, ISqlNode content, SqlTokenClosePar closer, ImmutableList<SqlTrivia> leading = null, ImmutableList<SqlTrivia> trailing = null )
            : base( leading, trailing )
        {
            _content = new SNode<SqlTokenOpenPar, ISqlNode, SqlTokenClosePar>( opener, content, closer );
            CheckContent();
        }

        void CheckContent()
        {
            Helper.CheckNotNull( Opener, nameof( Opener ) );
            Helper.CheckNotNull( Content, nameof( Content ) );
            Helper.CheckNotNull( Closer, nameof( Closer ) );
        }

        SqlPar( SqlPar o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _content = o._content;
            else
            {
                _content = new SNode<SqlTokenOpenPar, ISqlNode, SqlTokenClosePar>( items );
                CheckContent();
            }
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlPar( this, leading, children, trailing );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

        public override ISqlNode UnPar => Content.UnPar;

        public SqlTokenOpenPar Opener => _content.V1;

        public ISqlNode Content => _content.V2;

        public SqlTokenClosePar Closer => _content.V3;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor ) => visitor.Visit( this );
    }
}
