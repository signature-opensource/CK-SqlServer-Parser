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
    /// 
    /// </summary>
    public class SqlExprLike : SqlExpr
    {
        public SqlExprLike( SqlExpr left, SqlTokenIdentifier notToken, SqlTokenIdentifier likeToken, SqlExpr pattern, SqlTokenIdentifier escapeToken = null, SqlTokenLiteralString escapeChar = null )
            : this( null, Build( left, notToken, likeToken, pattern, escapeToken, escapeChar ), null )
        {
        }

        static ISqlNode[] Build( SqlExpr left, SqlTokenIdentifier notToken, SqlTokenIdentifier likeToken, SqlExpr pattern, SqlTokenIdentifier escapeToken = null, SqlTokenLiteralString escapeChar = null )
        {
            if( notToken == null )
            {
                if( escapeToken == null )
                {
                    return CreateArray<SqlNode>( SqlToken.EmptyOpenPar, left, likeToken, pattern, SqlToken.EmptyClosePar );
                }
                else
                {
                    if( escapeChar == null ) throw new ArgumentNullException( "escape" );
                    return CreateArray<SqlNode>( SqlToken.EmptyOpenPar, left, likeToken, pattern, escapeToken, escapeChar, SqlToken.EmptyClosePar );
                }
            }
            else
            {
                if( escapeToken == null )
                {
                    return CreateArray<SqlNode>( SqlToken.EmptyOpenPar, left, notToken, likeToken, pattern, SqlToken.EmptyClosePar );
                }
                else
                {
                    if( escapeChar == null ) throw new ArgumentNullException( "escape" );
                    return CreateArray<SqlNode>( SqlToken.EmptyOpenPar, left, notToken, likeToken, pattern, escapeToken, escapeChar, SqlToken.EmptyClosePar );
                }
            }
        }

        SqlExprLike( ImmutableList<SqlTrivia> leading, ISqlNode[] items, ImmutableList<SqlTrivia> trailing )
            : base( leading, items, trailing )
        {
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IReadOnlyList<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlExprLike( leading, EnsureArray( children ), trailing );
        }


        public SqlExpr Left { get { return (SqlExpr)Slots[1]; } }

        public bool IsNotLike { get { return Slots.Length == 6 || Slots.Length == 8; } }

        public SqlTokenIdentifier NotT { get { return IsNotLike ? (SqlTokenIdentifier)Slots[2] : null; } }

        public SqlTokenIdentifier LikeT { get { return (SqlTokenIdentifier)Slots[IsNotLike ? 3 : 2]; } }

        public SqlExpr Pattern { get { return (SqlExpr)Slots[IsNotLike ? 4 : 3]; } }

        public bool HasEscape { get { return Slots.Length > 6; } }

        public SqlTokenIdentifier EscapeT { get { return HasEscape ? (SqlTokenIdentifier)Slots[IsNotLike ? 5 : 4] : null; } }

        public SqlTokenLiteralString EscapeChar { get { return HasEscape ? (SqlTokenLiteralString)Slots[IsNotLike ? 6 : 5] : null; } }

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor )
        {
            return visitor.Visit( this );
        }

    }


}
