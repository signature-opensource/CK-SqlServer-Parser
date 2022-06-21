using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CK.SqlServer.Parser
{
    /// <summary>
    /// Generic list of <see cref="ISqlNode"/>.
    /// </summary>
    public sealed class SqlNodeList : ASqlNodeList<ISqlNode>
    {
        static public readonly SqlNodeList Empty = new SqlNodeList( Array.Empty<ISqlNode>() );

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
