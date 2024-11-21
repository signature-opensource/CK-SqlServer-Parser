using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Parser;

/// <summary>
/// Base class for all non token nodes.
/// </summary>
public abstract class SqlNonToken : SqlNode
{
    private protected SqlNonToken( ImmutableList<SqlTrivia> leading = null, ImmutableList<SqlTrivia> trailing = null )
        : base( leading, trailing )
    {
    }

    /// <summary>
    /// Always false.
    /// </summary>
    /// <param name="t">The token type to challenge.</param>
    /// <returns>Always false.</returns>
    public override sealed bool IsToken( SqlTokenType t ) => false;

    /// <summary>
    /// Gets a list starting with this node and the first nodes recursively.
    /// </summary>
    public override sealed IEnumerable<ISqlNode> LeadingNodes
    {
        get
        {
            ISqlNode n = this;
            for(; ; )
            {
                yield return n;
                if( n.ChildrenNodes.Count == 0 ) yield break;
                n = n.ChildrenNodes[0];
            }
        }
    }

    /// <summary>
    /// Gets a list starting with this node and the last nodes recursively.
    /// </summary>
    public override sealed IEnumerable<ISqlNode> TrailingNodes
    {
        get
        {
            ISqlNode n = this;
            for(; ; )
            {
                yield return n;
                if( n.ChildrenNodes.Count == 0 ) yield break;
                n = n.ChildrenNodes[n.ChildrenNodes.Count - 1];
            }
        }
    }

    /// <summary>
    /// Gets the leading trivias of all the <see cref="LeadingNodes"/>.
    /// </summary>
    public override sealed IEnumerable<SqlTrivia> FullLeadingTrivias => LeadingNodes.SelectMany( n => n.LeadingTrivias );

    /// <summary>
    /// Gets the trailing trivias of all the <see cref="TrailingNodes"/>.
    /// </summary>
    public override sealed IEnumerable<SqlTrivia> FullTrailingTrivias => TrailingNodes.Reverse().SelectMany( n => n.TrailingTrivias );

    /// <summary>
    /// Enumerates all the tokens that this node contains.
    /// </summary>
    public override sealed IEnumerable<SqlToken> AllTokens => ChildrenNodes.ToTokens();

}
