using CK.Core;
using CK.SqlServer.Parser;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Transform.Transformers
{
    /// <summary>
    /// Transformer that removes trivias.
    /// </summary>
    public class TriviaCleaner : SqlNodeLocationVisitor
    {
        readonly bool _clearStarcomment;
        readonly bool _clearLineComment;
        readonly bool _innerTriviasOnly;
        readonly ImmutableList<SqlTrivia>.Builder _builder;

        /// <summary>
        /// Initializes a new <see cref="TriviaCleaner"/>.
        /// </summary>
        /// <param name="monitor">The monitor to use.</param>
        /// <param name="clearLineComment">True to remove line comments.</param>
        /// <param name="clearStarComment">True to remove star comments.</param>
        /// <param name="innerTriviasOnly">
        /// True to remove only trivia that are inside the current selected range.
        /// False to also remove leading and trailing ones.
        /// </param>
        /// <exception cref="ArgumentException"></exception>
        public TriviaCleaner( IActivityMonitor monitor, bool clearLineComment, bool clearStarComment, bool innerTriviasOnly )
            : base( monitor )
        {
            if( !clearLineComment && !clearStarComment ) throw new ArgumentException();
            _clearLineComment = clearLineComment;
            _clearStarcomment = clearStarComment;
            _innerTriviasOnly = innerTriviasOnly;
            _builder = ImmutableList.CreateBuilder<SqlTrivia>();
        }

        /// <summary>
        /// Removes the trivias.
        /// </summary>
        /// <param name="e">The visited node.</param>
        /// <returns>The resulting node.</returns>
        protected override ISqlNode AfterVisitItem( ISqlNode e )
        {
            var leading = _innerTriviasOnly && VisitContext.RangeFilterStatus.IsBegAfter()
                            ? Remove( e.LeadingTrivias )
                            : e.LeadingTrivias;
            var trailing = _innerTriviasOnly && VisitContext.RangeFilterStatus.IsEndBefore()
                            ? Remove( e.TrailingTrivias )
                            : e.TrailingTrivias;
            return e.SetTrivias( leading, trailing );
        }

        ImmutableList<SqlTrivia> Remove( ImmutableList<SqlTrivia> trivias )
        {
            if( !trivias.IsEmpty )
            {
                _builder.Clear();
                bool hasEOL = false;
                foreach( var t in trivias )
                {
                    if( t.TokenType == SqlTokenType.LineComment && _clearLineComment )
                    {
                        if( !hasEOL )
                        {
                            _builder.Add( new SqlTrivia( SqlTokenType.None, Environment.NewLine ) );
                            hasEOL = true;
                        }
                    }
                    else if( t.TokenType == SqlTokenType.StarComment && _clearStarcomment )
                    {
                        _builder.Add( new SqlTrivia( SqlTokenType.None, " " ) );
                    }
                    else
                    {
                        hasEOL = false;
                        _builder.Add( t );
                    }
                }
                trivias = _builder.ToImmutableList();
            }
            return trivias;
        }
    }
}
