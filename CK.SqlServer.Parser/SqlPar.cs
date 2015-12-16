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
    public class SqlPar : SqlNode
    {
        readonly SNode<SqlTokenOpenPar, ISqlNode, SqlTokenClosePar> _items;

        public SqlPar( SqlTokenOpenPar opener, ISqlNode content, SqlTokenClosePar closer )
            : this( null, opener, content, closer, null )
        {
        }

        public SqlTokenOpenPar Opener => _items.O1;

        public ISqlNode Content => _items.O2;

        public SqlTokenClosePar Closer => _items.O3;

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _items;

        public override ISqlNode UnPar => Content.UnPar;

        SqlPar( ImmutableList<SqlTrivia> leading, SqlTokenOpenPar opener, ISqlNode content, SqlTokenClosePar closer, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            _items = new SNode<SqlTokenOpenPar, ISqlNode, SqlTokenClosePar>( opener, content, closer );
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IReadOnlyList<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlPar( leading, (SqlTokenOpenPar)children[0], children[1], (SqlTokenClosePar)children[2], trailing );
        }

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor )
        {
            return visitor.Visit( this );
        }

    }
}
