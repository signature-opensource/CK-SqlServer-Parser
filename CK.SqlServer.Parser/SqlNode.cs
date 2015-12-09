using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Parser
{
    /// <summary>
    /// Base class for all Sql nodes.
    /// This is an immutable object that carries leading and trailing <see cref="SqlTrivia"/>.
    /// </summary>
    public abstract class SqlNode
    {
        protected SqlNode( ImmutableList<SqlTrivia> leading = null, ImmutableList<SqlTrivia> trailing = null )
        {
            LeadingTrivias = leading ?? ImmutableList<SqlTrivia>.Empty;
            TrailingTrivias = trailing ?? ImmutableList<SqlTrivia>.Empty;
        }

        public readonly ImmutableList<SqlTrivia> LeadingTrivias;

        public readonly ImmutableList<SqlTrivia> TrailingTrivias;

        public abstract SqlNode SetTrivias( ImmutableList<SqlTrivia> leading, ImmutableList<SqlTrivia> trailing );

        protected bool TriviasDiffer( ref ImmutableList<SqlTrivia> leading, ref ImmutableList<SqlTrivia> trailing )
        {
            if( leading == null ) leading = ImmutableList<SqlTrivia>.Empty;
            if( trailing == null ) trailing = ImmutableList<SqlTrivia>.Empty;
            return leading != LeadingTrivias || trailing != TrailingTrivias;
        }

    }
}
