using System.Text;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CK.SqlServer.Parser;

public class SqlTextWriter
{
    /// <summary>
    /// Creates a default writer that writes everything.
    /// </summary>
    /// <param name="b">An optional existing String builder.</param>
    /// <param name="restoreUselessComments">
    /// True to restore injected comments (<see cref="SqlTrivia.QuoteUselessComment"/> and others)
    /// to their original text.
    /// </param>
    /// <returns>The text writer.</returns>
    public static ISqlTextWriter CreateDefault( StringBuilder b = null, bool restoreUselessComments = false )
    {
        return new Full( b ?? new StringBuilder(), restoreUselessComments );
    }

    /// <summary>
    /// Creates a writer on one line without any comments.
    /// Existing spaces (and line separators) are compacted but still appear in the result.
    /// </summary>
    /// <param name="b">An optional existing String builder.</param>
    /// <returns>The text writer.</returns>
    public static ISqlTextWriter CreateOneLineCompact( StringBuilder b = null )
    {
        return new OneLineCompact( b ?? new StringBuilder() );
    }

    /// <summary>
    /// Creates a writer without any optional separators: a white space appears only when it is
    /// syntaxically mandatory.
    /// </summary>
    /// <param name="b">An optional existing String builder.</param>
    /// <returns>The text writer.</returns>
    public static ISqlTextWriter CreateHyperCompact( StringBuilder b = null )
    {
        return new HyperCompact( b ?? new StringBuilder() );
    }

    class Full : ISqlTextWriter
    {
        readonly StringBuilder _b;
        readonly bool _restoreUselessComments;
        SqlTokenType _prevTokenType;

        public Full( StringBuilder b, bool restoreUselessComments )
        {
            _b = b;
            _restoreUselessComments = restoreUselessComments;
        }

        public bool SkipLineComment => false;

        public bool SkipStarComment => false;

        public void Write( SqlTrivia t )
        {
            if( t.IsEmpty ) return;
            _prevTokenType = t.TokenType;
            switch( t.TokenType )
            {
                case SqlTokenType.LineComment: _b.Append( "--" ).Append( t.Text ).AppendLine(); break;
                case SqlTokenType.StarComment:
                {
                    if( _restoreUselessComments )
                    {
                        if( ReferenceEquals( t.Text, SqlTrivia.OpenBracketUselessComment.Text ) )
                        {
                            _b.Append( '[' );
                            return;
                        }
                        if( ReferenceEquals( t.Text, SqlTrivia.CloseBracketUselessComment.Text ) )
                        {
                            _b.Append( ']' );
                            return;
                        }
                        if( ReferenceEquals( t.Text, SqlTrivia.QuoteUselessComment.Text ) )
                        {
                            _b.Append( '*' );
                            return;
                        }
                    }
                    _b.Append( "/*" ).Append( t.Text ).Append( "*/" );
                    break;
                }
                default: _b.Append( t.Text ); break;
            }
        }

        public void Write( SqlTokenIdentifier id, bool? whiteSpaceBefore = null, bool? whiteSpaceAfter = null ) => Write( id.TokenType, id.ToString(), whiteSpaceBefore, whiteSpaceAfter );

        public void Write( SqlTokenType type, string text, bool? whiteSpaceBefore = null, bool? whiteSpaceAfter = null )
        {
            if( SqlToken.RequiresSeparatorBetween( _prevTokenType, type ) )
            {
                _b.Append( ' ' );
            }
            _prevTokenType = type;
            _b.Append( text );
        }

        public override string ToString()
        {
            return _b.ToString();
        }
    }

    class OneLineCompact : ISqlTextWriter
    {
        readonly StringBuilder _b;
        bool _ensureWhiteSpace;
        bool _allowWhiteSpaceAfter;
        SqlTokenType _prevTokenType;

        public OneLineCompact( StringBuilder b ) { _b = b; }

        public bool SkipLineComment => true;

        public bool SkipStarComment => true;

        public void Write( SqlTrivia t )
        {
            if( !t.IsEmpty )
            {
                _ensureWhiteSpace = _allowWhiteSpaceAfter;
            }
        }

        public void Write( SqlTokenIdentifier id, bool? whiteSpaceBefore = null, bool? whiteSpaceAfter = null ) => Write( id.TokenType, id.ToString(), whiteSpaceBefore, whiteSpaceAfter );

        public void Write( SqlTokenType type, string text, bool? whiteSpaceBefore = null, bool? whiteSpaceAfter = null )
        {
            if( text.Length > 0 )
            {
                if( (!whiteSpaceBefore.HasValue && _ensureWhiteSpace)
                    || (whiteSpaceBefore.HasValue && whiteSpaceBefore.Value)
                    || SqlToken.RequiresSeparatorBetween( _prevTokenType, type ) )
                {
                    _b.Append( ' ' );
                }
                _b.Append( text );
                _prevTokenType = type;
                if( whiteSpaceAfter.HasValue )
                {
                    _allowWhiteSpaceAfter = _ensureWhiteSpace = whiteSpaceAfter.Value;
                }
                else
                {
                    _allowWhiteSpaceAfter = true;
                    _ensureWhiteSpace = false;
                }
            }
        }

        public override string ToString()
        {
            return _b.ToString();
        }
    }

    class HyperCompact : ISqlTextWriter
    {
        readonly StringBuilder _b;
        SqlTokenType _prevTokenType;

        public HyperCompact( StringBuilder b ) { _b = b; }

        public bool SkipLineComment => true;

        public bool SkipStarComment => true;

        public void Write( SqlTrivia t )
        {
        }

        public void Write( SqlTokenIdentifier id, bool? whiteSpaceBefore = null, bool? whiteSpaceAfter = null )
        {
            id = id.RemoveQuoteIfPossible( true );
            Write( id.TokenType, id.ToString(), whiteSpaceBefore, whiteSpaceAfter );
        }

        public void Write( SqlTokenType type, string text, bool? whiteSpaceBefore = null, bool? whiteSpaceAfter = null )
        {
            if( text.Length > 0 )
            {
                if( SqlToken.RequiresSeparatorBetween( _prevTokenType, type ) )
                {
                    _b.Append( ' ' );
                }
                _b.Append( text );
                _prevTokenType = type;
            }
        }

        public override string ToString()
        {
            return _b.ToString();
        }
    }

}
