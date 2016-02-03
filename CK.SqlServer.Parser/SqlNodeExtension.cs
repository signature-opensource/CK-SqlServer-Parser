using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace CK.SqlServer.Parser
{
    public static class SqlNodeExtension
    {
        /// <summary>
        /// Gets a flattened list of <see cref="SqlToken"/>.
        /// </summary>
        /// <param name="@this">This enumerable of SqlNode.</param>
        /// <returns>The flattened list of tokens.</returns>
        static public IEnumerable<SqlToken> ToTokens( this IEnumerable<ISqlNode> @this )
        {
            foreach( var a in @this )
            {
                SqlToken t = a as SqlToken;
                if( t != null ) yield return t;
                else foreach( var ta in ToTokens( a.AllTokens ) ) yield return ta;
            }
        }


        /// <summary>
        /// Sets trivias around this node.
        /// </summary>
        /// <param name="leading">Leading trivia. Can be null for empty trivias.</param>
        /// <param name="trailing">Trailing trivia. Can be null for empty trivias.</param>
        /// <returns>A new immutable object or this if no change occurred.</returns>
        static public T SetTrivias<T>( this T @this, IEnumerable<SqlTrivia> leading, IEnumerable<SqlTrivia> trailing ) where T : ISqlNode
        {
            return @this.SetTrivias( leading != null ? leading.ToImmutableList() : ImmutableList<SqlTrivia>.Empty, 
                                     trailing != null ? trailing.ToImmutableList() : ImmutableList<SqlTrivia>.Empty );
        }

        /// <summary>
        /// Sets trivias around this node.
        /// </summary>
        /// <param name="leading">Leading trivia. Can be null for empty trivias.</param>
        /// <param name="trailing">Trailing trivia. Can be null for empty trivias.</param>
        /// <returns>A new immutable object or this if no change occurred.</returns>
        static public T SetTrivias<T>( this T @this, ImmutableList<SqlTrivia> leading, ImmutableList<SqlTrivia> trailing ) where T : ISqlNode
        {
            return (T)((SqlNode)(object)@this).DoSetTrivias( leading, trailing );
        }

        /// <summary>
        /// Adds a leading trivia.
        /// </summary>
        /// <param name="t">The trivia to add in front.</param>
        /// <returns>A new immutable object.</returns>
        static public T AddLeadingTrivia<T>( this T @this, SqlTrivia t ) where T : ISqlNode
        {
            return (T)((SqlNode)(object)@this).DoAddLeadingTrivia( t );
        }

        /// <summary>
        /// Adds a trailing trivia.
        /// </summary>
        /// <param name="t">The trivia to append.</param>
        /// <returns>A new immutable object.</returns>
        static public T AddTrailingTrivia<T>( this T @this, SqlTrivia t ) where T : ISqlNode
        {
            return (T)((SqlNode)(object)@this).DoAddTrailingTrivia( t );
        }

        /// <summary>
        /// Removes leading trivias (in <see cref="FullLeadingTrivias"/>) from left to right that match
        /// the predicate. Extraction ends as soon as the predicate returns false.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <returns>A new immutable object or this if no change occurred.</returns>
        static public T ExtractLeadingTrivias<T>( this T @this, Func<SqlTrivia, bool> predicate ) where T : ISqlNode
        {
            return (T)((SqlNode)(object)@this).DoExtractLeadingTrivias( predicate );
        }

        /// <summary>
        /// Removes trailing trivias (in <see cref="FullTrailingTrivias"/>) from right to end that match
        /// the predicate. Extraction ends as soon as the predicate returns false.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <returns>A new immutable object or this if no change occurred.</returns>
        static public T ExtractTrailingTrivias<T>( this T @this, Func<SqlTrivia, bool> predicate ) where T : ISqlNode
        {
            return (T)((SqlNode)(object)@this).DoExtractTrailingTrivias( predicate );
        }

        /// <summary>
        /// Lifts leading and trailing trivias: <see cref="TrailingNodes"/> and <see cref="LeadingNodes"/> do not 
        /// have trailing trivias any more.
        /// </summary>
        /// <returns>A new immutable object or this if no change occurred.</returns>
        static public T LiftBothTrivias<T>(this T @this ) where T : ISqlNode
        {
            return (T)((SqlNode)(object)@this).DoLiftBothTrivias();
        }


        /// <summary>
        /// Lifts leading trivias: <see cref="LeadingNodes"/> do not have leading trivias any more.
        /// </summary>
        /// <returns>A new immutable object or this if no change occurred.</returns>
        static public T LiftLeadingTrivias<T>(this T @this ) where T : ISqlNode
        {
            return (T)((SqlNode)(object)@this).DoLiftLeadingTrivias();
        }

        /// <summary>
        /// Lifts trailing trivias: <see cref="TrailingNodes"/> do not have trailing trivias any more.
        /// </summary>
        /// <returns>A new immutable object or this if no change occurred.</returns>
        static public T LiftTrailingTrivias<T>( this T @this ) where T : ISqlNode
        {
            return (T)((SqlNode)(object)@this).DoLiftTrailingTrivias();
        }

        /// <summary>
        /// Sets or removes/clears a child at a given index in raw children (see <see cref="ISqlNode.GetRawContent"/>).
        /// </summary>
        /// <param name="i">The index that must be replaced.</param>
        /// <param name="child">The replacement. Null to remove or clear the node.</param>
        /// <returns>A new immutable object or this node if no change occurred.</returns>
        static public T ReplaceContentNode<T>( this T @this, int i, ISqlNode child ) where T : ISqlNode
        {
            return (T)((SqlNode)(object)@this).DoReplaceContentNode( i, child );
        }

        /// <summary>
        /// Sets or removes/clears two children at given indexes in raw children (see <see cref="ISqlNode.GetRawContent"/>).
        /// </summary>
        /// <param name="i1">The first index that must be replaced.</param>
        /// <param name="child1">The first replacement. Null to remove or clear the node.</param>
        /// <param name="i2">The first index that must be replaced.</param>
        /// <param name="child2">The first replacement. Null to remove or clear the node.</param>
        /// <returns>A new immutable object or this node if no change occurred.</returns>
        static public T ReplaceContentNode<T>( this T @this, int i1, ISqlNode child1, int i2, ISqlNode child2 ) where T : ISqlNode
        {
            return (T)((SqlNode)(object)@this).DoReplaceContentNode( i1, child1, i2, child2 );
        }

        /// <summary>
        /// Sets new children nodes.
        /// </summary>
        /// <param name="childrenNodes">Children nodes.</param>
        /// <returns>A new immutable object or this if no change occurred.</returns>
        static public T SetRawContent<T>( this T @this, IList<ISqlNode> childrenNodes ) where T : ISqlNode
        {
            return (T)((SqlNode)(object)@this).DoSetRawContent( childrenNodes );
        }

        /// <summary>
        /// Inserts or replace one or more children at a given index in <see cref="GetRawContent"/>.
        /// </summary>
        /// <param name="iStart">The index.</param>
        /// <param name="count">The number of children to replace.</param>
        /// <param name="child">The children to insert.</param>
        /// <returns>A new immutable object or this if no change occurred.</returns>
        static public T StuffRawContent<T>( this T @this, int iStart, int count, IReadOnlyList<ISqlNode> children ) where T : ISqlNode
        {
            return (T)((SqlNode)(object)@this).DoStuffRawContent( iStart, count, children );
        }

        /// <summary>
        /// Returns a hyper compact textual representation of this <see cref="ISqlNode"/>.
        /// This uses <see cref="SqlTextWriter.CreateHyperCompact(StringBuilder)"/>.
        /// </summary>
        /// <param name="this">This node.</param>
        /// <returns>A hyper compact string.</returns>
        public static string ToStringHyperCompact( this ISqlNode @this )
        {
            var w = SqlTextWriter.CreateHyperCompact();
            @this.WriteWithoutTrivias( w );
            return w.ToString();
        }

        /// <summary>
        /// Returns a compact textual representation of multiple <see cref="ISqlNode"/> (see <see cref="SqlTextWriter.CreateOneLineCompact(StringBuilder)"/>).
        /// </summary>
        /// <param name="this">This multiple nodes.</param>
        /// <returns>A compact string.</returns>
        public static string ToStringCompact( this IEnumerable<ISqlNode> @this )
        {
            return Write( @this, SqlTextWriter.CreateOneLineCompact() ).ToString();
        }

        public static ISqlTextWriter Write( this IEnumerable<ISqlNode> @this, ISqlTextWriter w )
        {
            foreach( var n in @this ) n.Write( w );
            return w;
        }

    }
}
