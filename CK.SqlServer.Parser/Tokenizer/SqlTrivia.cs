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
    public struct SqlTrivia
    {
        readonly SqlTokenType _tokenType;
        readonly string _text;

        /// <summary>
        /// A single space.
        /// </summary>
        public static readonly ImmutableList<SqlTrivia> OneSpace = ImmutableList.Create( new SqlTrivia( SqlTokenType.None, " " ) );

        public SqlTrivia( SqlTokenType tokenType, string text )
        {
            if( tokenType != SqlTokenType.None && tokenType != SqlTokenType.LineComment && tokenType != SqlTokenType.StarComment )
            {
                throw new ArgumentException( "Must be none, star or line comment.", "tokenType" );
            }
            if( text == null ) text = String.Empty;
            _tokenType = tokenType;
            _text = text ?? String.Empty;
        }

        /// <summary>
        /// Gets a token type that can be <see cref="SqlTokenType.None"/> for white space
        /// or <see cref="SqlTokenType.LineComment"/> or <see cref="SqlTokenType.StarComment"/>. 
        /// </summary>
        public SqlTokenType TokenType { get { return _tokenType; } }

        /// <summary>
        /// Gets whether this trivia is empty.
        /// </summary>
        public bool IsEmpty => _tokenType == SqlTokenType.None && (_text == null || _text.Length == 0);

        /// <summary>
        /// Gets the text of this trivia. Never null. 
        /// When it is a <see cref="SqlTokenType.LineComment"/> or <see cref="SqlTokenType.StarComment"/>,
        /// the -- or /* */ characters do not appear.
        /// </summary>
        public string Text { get { return _text ?? String.Empty; } }

        public override int GetHashCode()
        {
            return Util.Hash.Combine( (long)TokenType, Text.GetHashCode() ).GetHashCode();
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
                case SqlTokenType.LineComment: return "--" + Text + Environment.NewLine;
                case SqlTokenType.StarComment: return "/*" + Text + "*/";
            }
            return Text;
        }

        public string ToString( SqlTriviaWriteOption option )
        {
            switch( TokenType )
            {
                case SqlTokenType.LineComment: return option == SqlTriviaWriteOption.None 
                                                        ? "--" + Text + Environment.NewLine
                                                        : " ";
                case SqlTokenType.StarComment: return option == SqlTriviaWriteOption.None
                                                        ? "/*" + Text + "*/"
                                                        : " ";
            }
            if( _text != null && _text.Length > 0 )
            {
                return option == SqlTriviaWriteOption.None ? _text : " ";
            }
            return String.Empty;
        }


        /// <summary>
        /// Writes this trivia either its normal content or only one space.
        /// Note that when <see cref="IsEmpty"/> is true, nothing is written and false is returned.
        /// </summary>
        /// <param name="b">The StringBuilder to use.</param>
        /// <param name="option">The write option.</param>
        /// <returns>True if something has been written.</returns>
        public bool Write( StringBuilder b, SqlTriviaWriteOption option )
        {
            switch( TokenType )
            {
                case SqlTokenType.LineComment:
                    {
                        if( option == SqlTriviaWriteOption.OneSpace ) b.Append( ' ' );
                        else b.Append( "--" ).Append( Text ).Append( Environment.NewLine );
                        break;
                    }
                case SqlTokenType.StarComment:
                    {
                        if( option == SqlTriviaWriteOption.OneSpace ) b.Append( ' ' );
                        else b.Append( "/*" ).Append( Text ).Append( "*/" );
                        break;
                    }
                default:
                    {
                        if( _text == null || _text.Length == 0 ) return false;
                        if( option == SqlTriviaWriteOption.OneSpace ) b.Append( ' ' );
                        else b.Append( _text );
                        break;
                    }
            }
            return true;
        }
    }

}
