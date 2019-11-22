using CK.SqlServer.Parser;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Transform
{
    /// <summary>
    /// Builds a range from the start to the first node that has a matching trivia or from
    /// the end of the last node that has a matching trivia up to the end.
    /// </summary>
    public sealed class SqlNodeScopeFromTriviaMatcher : SqlNodeScopeExtrema
    {
        readonly Func<SqlTrivia, bool> _triviaMatcher;
        readonly string _triviaDescription;

        /// <summary>
        /// Initializes a new <see cref="SqlNodeScopeFromTriviaMatcher"/>.
        /// </summary>
        /// <param name="afterRange">Whether the range after the match must be build.</param>
        /// <param name="triviaMatcher">The trivia predicate.</param>
        /// <param name="triviaDescription">The description of the trivia predicate.</param>
        public SqlNodeScopeFromTriviaMatcher( bool afterRange, Func<SqlTrivia, bool> triviaMatcher, string triviaDescription )
            : base( new SqlNodeScopeBreadthPredicate( afterRange
                                                        ? (Func<ISqlNode, bool>)(n => n.TrailingTrivias.Any( t => triviaMatcher( t ) ))
                                                        : n => n.LeadingTrivias.Any( t => triviaMatcher( t ) ) ),
                    afterRange ? Option.AfterIncluded : Option.BeforeIncluded )
        {
            if( triviaMatcher == null ) throw new ArgumentException( nameof( triviaMatcher ) );
            if( triviaDescription == null ) throw new ArgumentException( nameof( triviaDescription ) );
            _triviaMatcher = triviaMatcher;
            _triviaDescription = triviaDescription;
        }

        /// <summary>
        /// Overridden to return the description of this builder.
        /// </summary>
        /// <returns>A readable string.</returns>
        public override string ToString() => ToString( _triviaDescription );
    
    }

}
