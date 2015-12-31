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

        ISqlNode UnPar { get; }

        bool IsToken( SqlTokenType t );

        /// <summary>
        /// Sets or removes a child at a given index in <see cref="ChildrenNodes"/>.
        /// </summary>
        /// <param name="i">The index.</param>
        /// <param name="child">Null to remove or the node to replace.</param>
        /// <returns>A new immutable object or this if no change occurred.</returns>
        ISqlNode ReplaceChildNode( int i, ISqlNode child );

        /// <summary>
        /// Sets new children nodes.
        /// </summary>
        /// <param name="childrenNodes">Children nodes.</param>
        /// <returns>A new immutable object or this if no change occurred.</returns>
        ISqlNode SetChildrenNodes( IReadOnlyList<ISqlNode> childrenNodes );

        /// <summary>
        /// Inserts or replace one or more children at a given index in <see cref="ChildrenNodes"/>.
        /// </summary>
        /// <param name="iStart">The index.</param>
        /// <param name="count">The number of children to replace.</param>
        /// <param name="child">The children to insert.</param>
        /// <returns>A new immutable object or this if no change occurred.</returns>
        ISqlNode StuffChildren( int iStart, int count, IReadOnlyList<ISqlNode> children );
        
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
        /// <returns>This node text representation.</returns>
        string ToString( bool withThisTrivia );

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