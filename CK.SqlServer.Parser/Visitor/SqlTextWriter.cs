using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Parser
{
    public class SqlTextWriter
    {
        readonly StringBuilder _b;
        readonly StringBuilder _currentLine;
        string _newLine;
        string _pendingLine;
        bool _pendingSpace;
        bool _currentLineIsEmpty;

        public SqlTextWriter()
            : this( new StringBuilder() )
        {
        }

        public SqlTextWriter( StringBuilder b )
        {
            _newLine = Environment.NewLine;
            _b = b;
            _currentLine = new StringBuilder();
        }

        public enum WhiteSpaceOption
        {
            Default,
            Compact
        }

        public bool SkipStarComment { get; set; }

        public bool SkipLineComment { get; set; }

        public WhiteSpaceOption WhiteSpace { get; set; }

        public void Write( SqlTrivia t )
        {
            switch( t.TokenType )
            {
                case SqlTokenType.LineComment:
                    {
                        if( !SkipLineComment )
                        {
                            GetLineBuilder().Append( "--" ).Append( t.Text );
                        }
                        EmitCurrentLine();
                        break;
                    }
                case SqlTokenType.StarComment:
                    {
                        if( !SkipStarComment )
                        {
                            var text = t.Text;
                            GetLineBuilder().Append( "/*" );
                            WriteText( text );
                            GetLineBuilder().Append( "*/" );
                        }
                        break;
                    }
                default:
                    {
                        Debug.Assert( t.TokenType == SqlTokenType.None );
                        var text = t.Text;
                        if( WhiteSpace == WhiteSpaceOption.Default )
                        {
                            WriteText( text );
                        }
                        else if( WhiteSpace == WhiteSpaceOption.Compact )
                        {
                            int idx = text.LastIndexOf( Environment.NewLine );
                            if( idx >= 0 )
                            {
                                EmitCurrentLine();
                                GetLineBuilder().Append( text.Substring( idx + Environment.NewLine.Length ) );
                                _currentLineIsEmpty = true;
                            }
                            else EnsureWhiteSpace();
                        }
                        break;
                    }
            }
        }

        void WriteText( string text )
        {
            int lastIdx = 0;
            int idx, len;
            while( (idx = text.IndexOf( Environment.NewLine, lastIdx )) >= 0 )
            {
                len = idx - lastIdx;
                if( len > 0 )
                {
                    GetLineBuilder().Append( text, lastIdx, len );
                }
                EmitCurrentLine();
                lastIdx = idx + 2;
            }
            len = text.Length - lastIdx;
            if( len > 0 )
            {
                GetLineBuilder().Append( text, lastIdx, len );
            }
        }

        int _currentLineMustBeEmitted;
        bool _hasEmittedData;

        public StringBuilder GetLineBuilder( bool canOmitWhiteSpace = false )
        {
            if( _currentLineMustBeEmitted > 0 )
            {
                if( !(_currentLineIsEmpty && WhiteSpace == WhiteSpaceOption.Compact) )
                {
                    if( !_hasEmittedData ) --_currentLineMustBeEmitted;
                    while( --_currentLineMustBeEmitted >= 0 ) _b.Append( _newLine );
                    _b.Append( _currentLine.ToString() );
                    _hasEmittedData = true;
                }
                _currentLine.Clear();
                _currentLineIsEmpty = true;
                _pendingSpace = false;
                _currentLineMustBeEmitted = 0;
            }
            if( _pendingSpace && _currentLine.Length > 0 && !canOmitWhiteSpace )
            {
                _currentLine.Append( ' ' );
            }
            _currentLineIsEmpty = _pendingSpace = false;
            return _currentLine;
        }

        public void EmitCurrentLine()
        {
            _currentLineMustBeEmitted++;
        }

        void EnsureWhiteSpace()
        {
            _pendingSpace = true;
        }

        public override string ToString()
        {
            string s = _b.ToString();
            bool hasEmittedData = _hasEmittedData;
            if( !(_currentLineIsEmpty && WhiteSpace == WhiteSpaceOption.Compact) )
            {
                if( hasEmittedData ) s += _newLine;
                hasEmittedData = true;
                s += _currentLine.ToString();
            }
            int nbNewLines = _currentLineMustBeEmitted;
            if( nbNewLines > 0 )
            {
                if( !hasEmittedData ) --nbNewLines;
                while( --nbNewLines >= 0 ) s += _newLine;
            }
            return s;
        }
    }
}