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
    /// <summary>
    /// Generic list of <see cref="ISqlNode"/>.
    /// </summary>
    public sealed class SqlNodeList : ASqlNodeList<ISqlNode>
    {
        static public readonly SqlNodeList Empty = new SqlNodeList( Util.Array.Empty<ISqlNode>() );

        public SqlNodeList( IEnumerable<ISqlNode> items )
            : base( 0, items )
        {
        }

        public SqlNodeList( params ISqlNode[] items )
            : base( 0, items )
        {
        }

        SqlNodeList( SqlNodeList o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( o, 0, leading, items, trailing )
        {
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlNodeList( this, leading, content, trailing );
        }

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

    }


}
