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
    /// Token for identifiers.
    /// </summary>
    public sealed class SqlTokenIdentifier : SqlToken, ISqlIdentifier, IReadOnlyList<ISqlIdentifier>
    {
        readonly string _name;

        public SqlTokenIdentifier( SqlTokenType t, string name, ImmutableList<SqlTrivia> leadingTrivia = null, ImmutableList<SqlTrivia> trailingTrivia = null )
            : base( t, leadingTrivia, trailingTrivia )
        {
            if( (t&SqlTokenType.IsIdentifier) == 0 ) throw new ArgumentException( "Invalid token type.", "t" );
            if( string.IsNullOrWhiteSpace( name ) ) throw new ArgumentNullException( "name" );
            if( IsVariable && name[0] != '@' ) throw new ArgumentException( "Invalid variable name.", "name" );
            _name = name;
        }

        /// <summary>
        /// True if this <see cref="SqlTokenIdentifier"/> is a @Variable or a @@SystemFunction.
        /// </summary>
        public bool IsVariable => TokenType.IsVariable();

        bool ISqlIdentifier.IsOpenDataSouce => false;

        IReadOnlyList<ISqlIdentifier> ISqlIdentifier.Identifiers => this;

        int IReadOnlyCollection<ISqlIdentifier>.Count => 1;

        ISqlIdentifier IReadOnlyList<ISqlIdentifier>.this[int index]
        {
            get
            {
                if( index != 0 ) throw new IndexOutOfRangeException();
                return this;
            }
        }

        IEnumerator<ISqlIdentifier> IEnumerable<ISqlIdentifier>.GetEnumerator()
        {
            return new CKEnumeratorMono<SqlTokenIdentifier>( this );
        }

        public SqlTokenIdentifier RemoveQuoteIfPossible( bool keepIfReservedKeyword )
        {
            // Already quotes free.
            if( !TokenType.IsQuotedIdentifier() ) return this;
            
            // Quotes exist.
            
            // Are quotes required? If yes, don't do it.
            if( SqlToken.IsQuoteRequired( _name ) ) return this;

            // If it is a known (reserved) keyword and it must be preserved, do not do anything.
            SqlTokenType typeWithoutQuote;
            bool isReservedKeyWord = SqlKeyword.IsReservedKeyword( _name, out typeWithoutQuote );
            if( keepIfReservedKeyword && isReservedKeyWord ) return this;
            if( typeWithoutQuote == SqlTokenType.None ) typeWithoutQuote = SqlTokenType.IdentifierStandard;

            return new SqlTokenIdentifier( typeWithoutQuote, _name, LeadingTrivias, TrailingTrivias );
        }

        /// <summary>
        /// Gets the identifier string (without quotes or brackets if this is quoted).
        /// </summary>
        public string Name => _name; 

        public bool NameEquals( string name )
        { 
            return String.Compare( _name, name, StringComparison.OrdinalIgnoreCase ) == 0; 
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlTokenIdentifier( TokenType, _name, leading, trailing );
        }

        public override string ToString()
        {
            switch( TokenType )
            {
                case SqlTokenType.IdentifierQuoted:
                    return "\"" + _name.Replace( "\"", "\"\"" ) + "\"";
                case SqlTokenType.IdentifierQuotedBracket:
                    return "[" + _name.Replace( "]", "]]" ) + "]";
                default: return _name;
            }
        }

        public override void WriteWithoutTrivias( ISqlTextWriter w ) => w.Write( TokenType, ToString() );

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

    }


}
