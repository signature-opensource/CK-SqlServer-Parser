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
    /// Captures the optional "From ..." select part.
    /// </summary>
    public class SelectFrom : ASqlNodeArrayBased
    {
        public SelectFrom( SqlTokenIdentifier fromT, ISqlNode content )
            : this( null, CreateArray( fromT, content ), null )
        {
        }

        internal SelectFrom( ImmutableList<SqlTrivia> leading, ISqlNode[] items, ImmutableList<SqlTrivia> trailing )
            : base( leading, items, trailing )
        {
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IReadOnlyList<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SelectFrom( leading, EnsureArray( children ), trailing );
        }

        public SqlTokenIdentifier FromT => (SqlTokenIdentifier)Children[0];
        
        public ISqlNode Content => Children[1];

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor ) => visitor.Visit( this );

    }


}
