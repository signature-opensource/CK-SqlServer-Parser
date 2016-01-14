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
        static public T LiftTrailingTrivias<T>(this T @this ) where T : ISqlNode
        {
            return (T)((SqlNode)(object)@this).DoLiftTrailingTrivias();
        }

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
