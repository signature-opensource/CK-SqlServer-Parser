using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Parser
{
    public class SqlPar : SqlItem
    {
        public SqlPar( SqlTokenOpenPar opener, SqlNode node, SqlTokenClosePar closer )
            : base( null, CreateArray( opener, node, closer ), null )
        {
            if( opener == null ) throw new ArgumentException();
            if( closer == null ) throw new ArgumentException();
        }

        internal SqlPar( ImmutableList<SqlTrivia> leading, SqlNode[] items, ImmutableList<SqlTrivia> trailing )
            : base( leading, items, trailing )
        {
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IReadOnlyList<SqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlPar( leading, EnsureArray( children ), trailing );
        }

        [DebuggerStepThrough]
        internal protected override T Accept<T>( ISqlItemVisitor<T> visitor )
        {
            return visitor.Visit( this );
        }

    }
}
