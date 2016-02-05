using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace CK.SqlServer.Parser
{
    /// <summary>
    /// Base interface for all Sql nodes.
    /// This is an immutable object that carries leading and trailing <see cref="SqlTrivia"/>.
    /// </summary>
    public interface ISqlNode
    {
        /// <summary>
        /// Gets the tokens that compose this node.
        /// </summary>
        IEnumerable<SqlToken> AllTokens { get; }

        /// <summary>
        /// Gets the direct children if any. Never null.
        /// </summary>
        IReadOnlyList<ISqlNode> ChildrenNodes { get; }

        /// <summary>
        /// Extracts a mutable copy of the children. This list is either a truly dynamic <see cref="List{T}"/> 
        /// with non null ISqlNode in it or an array with potentially null nodes that may be changed but 
        /// its length can not change.
        /// </summary>
        /// <returns>A List&lt;ISqlNode&gt; or a ISqlode[] array.</returns>
        IList<ISqlNode> GetRawContent();

        /// <summary>
        /// Leading <see cref="SqlTrivia"/>. Never null but can be empty.
        /// </summary>
        ImmutableList<SqlTrivia> LeadingTrivias { get; }

        /// <summary>
        /// Trailing <see cref="SqlTrivia"/>. Never null but can be empty.
        /// </summary>
        ImmutableList<SqlTrivia> TrailingTrivias { get; }

        /// <summary>
        /// Gets the whole leading trivias for this node and its <see cref="LeadingNodes"/>.
        /// </summary>
        IEnumerable<SqlTrivia> FullLeadingTrivias { get; }

        /// <summary>
        /// Gets the whole trailing trivias for this node and its <see cref="TrailingNodes"/>.
        /// </summary>
        IEnumerable<SqlTrivia> FullTrailingTrivias { get; }

        /// <summary>
        /// Gets the leading nodes from this one to the deepest left-most children.
        /// </summary>
        IEnumerable<ISqlNode> LeadingNodes { get; }

        /// <summary>
        /// Gets the trailing nodes from this one to the deepest right-most children.
        /// </summary>
        IEnumerable<ISqlNode> TrailingNodes { get; }

        /// <summary>
        /// Gets this node or the inner node of a <see cref="SqlPar"/>.
        /// </summary>
        ISqlNode UnPar { get; }

        bool IsToken( SqlTokenType t );

        /// <summary>
        /// Overriden to return the result of <see cref="WriteWithoutTrivias"/> with 
        /// a one line, compact, writer (<see cref="SqlTextWriter.CreateOneLineCompact"/>).
        /// </summary>
        /// <returns>The mere node.</returns>
        string ToString();

        /// <summary>
        /// Returns the result of <see cref="Write"/> or <see cref="WriteWithoutTrivias"/> with 
        /// a default writer (<see cref="SqlTextWriter.CreateDefault"/>): all internal trivias appear.
        /// </summary>
        /// <param name="withThisTrivia">
        /// True to include this <see cref="LeadingTrivias"/> and <see cref="TrailingTrivias"/>, false otherwise.
        /// </param>
        /// <param name="restoreUselessComments">
        /// True to restore injected comments (<see cref="SqlTrivia.QuoteUselessComment"/> and others)
        /// to their original text.
        /// </param>
        /// <returns>This node text representation.</returns>
        string ToString( bool withThisTrivia, bool restoreUselessComments = false );

        /// <summary>
        /// Writes the node with its <see cref="LeadingTrivia"/> and <see cref="TrailingTrivia"/>.
        /// </summary>
        /// <param name="w">The <see cref="ISqlTextWriter"/> to write to.</param>
        void Write( ISqlTextWriter w );
        
        /// <summary>
        /// Writes the token without this leading nor traling trivias.
        /// </summary>
        /// <param name="w">The <see cref="ISqlTextWriter"/> to write to.</param>
        void WriteWithoutTrivias( ISqlTextWriter w );
    }
}