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

        /// <summary>
        /// Gets the direct children if any. Never null.
        /// </summary>
        public abstract IReadOnlyList<SqlNode> ChildrenNodes { get; }

        /// <summary>
        /// Leading <see cref="SqlTrivia"/>. Never null but can be empty.
        /// </summary>
        public readonly ImmutableList<SqlTrivia> LeadingTrivias;

        /// <summary>
        /// Trailing <see cref="SqlTrivia"/>. Never null but can be empty.
        /// </summary>
        public readonly ImmutableList<SqlTrivia> TrailingTrivias;

        /// <summary>
        /// Gets the leading nodes from this one to the deepest left-most children.
        /// </summary>
        public IEnumerable<SqlNode> LeadingNodes
        {
            get
            {
                var n = this;
                for( ;;)
                {
                    yield return n;
                    if( n.ChildrenNodes.Count == 0 ) yield break;
                    n = n.ChildrenNodes[0];
                }
            }
        }

        /// <summary>
        /// Gets the trailing nodes from this one to the deepest right-most children.
        /// </summary>
        public IEnumerable<SqlNode> TrailingNodes
        {
            get
            {
                var n = this;
                for( ;;)
                {
                    yield return n;
                    if( n.ChildrenNodes.Count == 0 ) yield break;
                    n = n.ChildrenNodes[n.ChildrenNodes.Count-1];
                }
            }
        }

        /// <summary>
        /// Gets the whole leading trivias for this node and its <see cref="LeadingNodes"/>.
        /// </summary>
        public IEnumerable<SqlTrivia> FullLeadingTrivias => LeadingNodes.SelectMany( n => n.LeadingTrivias );

        /// <summary>
        /// Gets the whole trailing trivias for this node and its <see cref="TrailingNodes"/>.
        /// </summary>
        public IEnumerable<SqlTrivia> FullTrailingTrivias => TrailingNodes.Reverse().SelectMany( n => n.TrailingTrivias );

        //public SqlNode LiftLeadingTrivias()
        //{
        //    ImmutableList<SqlTrivia> t = ImmutableList.CreateRange( FullLeadingTrivias );
        //    foreach( var n in LeadingNodes.Skip(1) )
        //}

        /// <summary>
        /// Sets trivias around this node.
        /// </summary>
        /// <param name="leading">Leading trivia. Can be null.</param>
        /// <param name="trailing">Trailing trivia. Can be null.</param>
        /// <returns>A new immutable node.</returns>
        public abstract SqlNode SetTrivias( ImmutableList<SqlTrivia> leading, ImmutableList<SqlTrivia> trailing );

        /// <summary>
        /// Handles null and compares with the current trivias.
        /// </summary>
        /// <param name="leading">>Leading trivias.</param>
        /// <param name="trailing">Trailing trivias.</param>
        /// <returns>True if the new trivias are not the same than the current ones.</returns>
        protected bool TriviasDiffer( ref ImmutableList<SqlTrivia> leading, ref ImmutableList<SqlTrivia> trailing )
        {
            if( leading == null ) leading = ImmutableList<SqlTrivia>.Empty;
            if( trailing == null ) trailing = ImmutableList<SqlTrivia>.Empty;
            return leading != LeadingTrivias || trailing != TrailingTrivias;
        }

    }
}
