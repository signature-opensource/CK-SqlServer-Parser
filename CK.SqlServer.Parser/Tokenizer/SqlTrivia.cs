using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using CK.Core;
using System.Diagnostics;
using System.Globalization;
using System.Collections.Immutable;

namespace CK.SqlServer.Parser
{

    public struct SqlTrivia : ISqlServerComment
    {
        readonly SqlTokenType _tokenType;
        readonly string _text;

        /// <summary>
        /// A single space.
        /// </summary>
        public static readonly ImmutableList<SqlTrivia> OneSpace = ImmutableList.Create( new SqlTrivia( SqlTokenType.None, " " ) );

        /// <summary>
        /// The /*[ - Useless (By CK)*/ special comment.
        /// </summary>
        public static readonly SqlTrivia OpenBracketUselessComment = new SqlTrivia( SqlTokenType.StarComment, "[ - Useless (By CK)" );

        /// <summary>
        /// The /*] - Useless (By CK)*/ special comment.
        /// </summary>
        public static readonly SqlTrivia CloseBracketUselessComment = new SqlTrivia( SqlTokenType.StarComment, "] - Useless (By CK)" );

        /// <summary>
        /// The /*" - Useless (By CK)*/ special comment.
        /// </summary>
        public static readonly SqlTrivia QuoteUselessComment = new SqlTrivia( SqlTokenType.StarComment, "\" - Useless (By CK)" );


        public SqlTrivia( SqlTokenType tokenType, string text )
        {
            if( tokenType != SqlTokenType.None 
                && tokenType != SqlTokenType.LineComment 
                && tokenType != SqlTokenType.StarComment )
            {
                throw new ArgumentException( "Must be none, star or line comment.", nameof( tokenType ) );
            }
            _tokenType = tokenType;
            _text = text ?? string.Empty;
        }

        /// <summary>
        /// Gets a token type that can be <see cref="SqlTokenType.None"/> for white space
        /// or <see cref="SqlTokenType.LineComment"/> or <see cref="SqlTokenType.StarComment"/>. 
        /// </summary>
        public SqlTokenType TokenType => _tokenType;

        /// <summary>
        /// Gets whether this trivia is empty.
        /// </summary>
        public bool IsEmpty => _tokenType == SqlTokenType.None && (_text == null || _text.Length == 0);

        /// <summary>
        /// Gets the text of this trivia. Never null. 
        /// When it is a <see cref="SqlTokenType.LineComment"/> or <see cref="SqlTokenType.StarComment"/>,
        /// the -- or /* */ characters do not appear.
        /// </summary>
        public string Text => _text ?? String.Empty;

        bool ISqlServerComment.IsLineComment => TokenType == SqlTokenType.LineComment;

        string ISqlServerComment.Text => _text;

        public override int GetHashCode()
        {
            return HashCode.Combine( (long)TokenType, Text );
        }

        public override bool Equals( object obj )
        {
            if( obj is SqlTrivia )
            {
                SqlTrivia t = (SqlTrivia)obj;
                return t.TokenType == TokenType && t.Text == Text;
            }
            return false;
        }

        public override string ToString()
        {
            switch( TokenType )
            {
                case SqlTokenType.LineComment: return "--" + _text + Environment.NewLine;
                case SqlTokenType.StarComment: return "/*" + _text + "*/";
            }
            return Text;
        }


        static public void ToMiddle<TL, TM, TR>( ref TL left, ref TM middle, ref TR right )
            where TL : ISqlNode
            where TM : ISqlNode
            where TR : ISqlNode
        {
            ToRight( ref left, ref middle );
            ToLeft( ref middle, ref right );
        }

        static public void ToLeft<TL, TR>( ref TL left, ref TR right )
            where TL : ISqlNode
            where TR : ISqlNode
        {
            var transfer = right.LeadingTrivias;
            right = right.SetTrivias( null, right.TrailingTrivias );
            left = left.SetTrivias( left.LeadingTrivias, left.TrailingTrivias.AddRange( transfer ) );
        }


        static public void ToRight<TL, TR>( ref TL left, ref TR right )
            where TL : ISqlNode
            where TR : ISqlNode
        {
            var transfer = left.TrailingTrivias;
            left = left.SetTrivias( left.LeadingTrivias, null );
            right = right.SetTrivias( transfer.AddRange( right.LeadingTrivias ), right.TrailingTrivias );
        }

        static public void WhiteSpaceToMiddle<TL, TM, TR>( ref TL left, ref TM middle, ref TR right )
            where TL : ISqlNode
            where TM : ISqlNode
            where TR : ISqlNode
        {
            WhiteSpaceToRight( ref left, ref middle );
            WhiteSpaceToLeft( ref middle, ref right );
        }

        static public Tuple<TL, TM, TR> WhiteSpaceToMiddle<TL, TM, TR>( TL left, TM middle, TR right )
             where TL : ISqlNode
             where TM : ISqlNode
             where TR : ISqlNode
        {
            WhiteSpaceToRight( ref left, ref middle );
            WhiteSpaceToLeft( ref middle, ref right );
            return Tuple.Create( left, middle, right );
        }

        static public void WhiteSpaceToRight<TL, TR>( ref TL left, ref TR right )
            where TL : ISqlNode
            where TR : ISqlNode
        {
            ISqlNode r = right;
            left = left.ExtractTrailingTrivias( ( t, idx ) =>
            {
                if( t.TokenType == SqlTokenType.None )
                {
                    r = r.AddLeadingTrivia( t );
                    return true;
                }
                return false;
            } );
            right = (TR)r;
        }

        static public Tuple<TL, TR> WhiteSpaceToRight<TL, TR>( TL left, TR right )
             where TL : ISqlNode
             where TR : ISqlNode
        {
            WhiteSpaceToRight( ref left, ref right );
            return Tuple.Create( left, right );
        }

        static public void WhiteSpaceToLeft<TL, TR>( ref TL left, ref TR right )
            where TL : ISqlNode
            where TR : ISqlNode
        {
            ISqlNode l = left;
            right = right.ExtractLeadingTrivias( ( t, idx ) =>
            {
                if( t.TokenType == SqlTokenType.None )
                {
                    l = l.AddTrailingTrivia( t );
                    return true;
                }
                return false;

            } );
            left = (TL)l;
        }


        static public Tuple<TL, TR> WhiteSpaceToLeft<TL, TR>( TL left, TR right )
             where TL : ISqlNode
             where TR : ISqlNode
        {
            WhiteSpaceToLeft( ref left, ref right );
            return Tuple.Create( left, right );
        }

    }

}
