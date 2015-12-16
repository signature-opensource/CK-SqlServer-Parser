using System;
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
        public SqlPar( SqlTokenOpenPar opener, SqlNode content, SqlTokenClosePar closer )
            : this( null, opener, content, closer, null )
        {
            if( opener == null ) throw new ArgumentException();
            if( closer == null ) throw new ArgumentException();
            if( content == null ) throw new ArgumentException();
        }

        public SqlTokenOpenPar Opener { get; }

        public SqlNode Content { get; }

        public SqlTokenClosePar Closer { get; }

        public override IReadOnlyList<SqlNode> ChildrenNodes => new[] { Opener, Content, Closer };

        public override SqlNode UnPar => Content.UnPar;

        SqlPar( ImmutableList<SqlTrivia> leading, SqlTokenOpenPar opener, SqlNode content, SqlTokenClosePar closer, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            Opener = opener;
            Content = content;
            Closer = closer;
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IReadOnlyList<SqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlPar( leading, (SqlTokenOpenPar)children[0], children[1], (SqlTokenClosePar)children[2], trailing );
        }

        [DebuggerStepThrough]
        internal protected override SqlNode Accept( SqlItemVisitor visitor )
        {
            return visitor.Visit( this );
        }

    }
}
