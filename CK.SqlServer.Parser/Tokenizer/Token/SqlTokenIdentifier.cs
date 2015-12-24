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
    /// Token for identifiers. An identifier can be <see cref="IsQuoted"/>, be <see cref="IsVariable"/>, be <see cref="IsKeywordName"/>.
    /// </summary>
    public sealed class SqlTokenIdentifier : SqlToken, ISqlIdentifier, IReadOnlyList<SqlTokenIdentifier>
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
        /// True for star (*) identifier. 
        /// </summary>
        public bool IsStar => TokenType == SqlTokenType.IdentifierStar;

        /// <summary>
        /// True for type names like int or sql_variant. 
        /// </summary>
        public bool IsDbType => (TokenType&SqlTokenType.IdentifierTypeMask) == SqlTokenType.IdentifierDbType; 

        /// <summary>
        /// True if this <see cref="SqlTokenIdentifier"/> is [quoted] or "quoted".
        /// </summary>
        public bool IsQuoted => TokenType == SqlTokenType.IdentifierQuoted || TokenType == SqlTokenType.IdentifierQuotedBracket;

        /// <summary>
        /// True if this <see cref="SqlTokenIdentifier"/> is a @Variable or a @@SystemFunction.
        /// </summary>
        public bool IsVariable => TokenType == SqlTokenType.IdentifierVariable;

        /// <summary>
        /// True if this <see cref="SqlTokenIdentifier"/> denotes a reserved keyword (select, create, declare, etc.)
        /// or a standard identifer that starts a statement (throw, get, move, etc.).
        /// </summary>
        public bool IsStartStatement => TokenType.IsStartStatement();

        /// <summary>
        /// True if this <see cref="SqlTokenIdentifier"/> is a reserved keyword.
        /// </summary>
        public bool IsReservedKeyword => TokenType.IsReservedKeyword();

        IReadOnlyList<SqlTokenIdentifier> ISqlIdentifier.Identifiers => this;

        int IReadOnlyCollection<SqlTokenIdentifier>.Count => 1;

        SqlTokenIdentifier IReadOnlyList<SqlTokenIdentifier>.this[int index]
        {
            get
            {
                if( index != 0 ) throw new IndexOutOfRangeException();
                return this;
            }
        }

        IEnumerator<SqlTokenIdentifier> IEnumerable<SqlTokenIdentifier>.GetEnumerator()
        {
            return new CKEnumeratorMono<SqlTokenIdentifier>( this );
        }

        public SqlTokenIdentifier RemoveQuoteIfPossible( bool keepIfReservedKeyword )
        {
            // Already quote free.
            if( !IsQuoted ) return this;
            
            // Quotes exist.
            
            // Are quotes required? If yes, don't do it.
            if( SqlToken.IsQuoteRequired( Name ) ) return this;

            // If it is a known (reserved) keyword and it must be preserved, do not do anything.
            SqlTokenType typeWithoutQuote;
            bool isReservedKeyWord = SqlKeyword.IsReservedKeyword( Name, out typeWithoutQuote );
            if( keepIfReservedKeyword && isReservedKeyWord ) return this;
            if( typeWithoutQuote == SqlTokenType.None ) typeWithoutQuote = SqlTokenType.IdentifierStandard;

            return new SqlTokenIdentifier( typeWithoutQuote, Name, LeadingTrivias, TrailingTrivias );
        }

        public string Name => _name; 

        public bool NameEquals( string name )
        { 
            return String.Compare( _name, name, StringComparison.OrdinalIgnoreCase ) == 0; 
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IReadOnlyList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlTokenIdentifier( TokenType, _name, leading, trailing );
        }

        public override string ToString()
        {
            switch( TokenType )
            {
                case SqlTokenType.IdentifierQuoted:
                    return "\"" + Name.Replace( "\"", "\"\"" ) + "\"";
                case SqlTokenType.IdentifierQuotedBracket:
                    return "[" + Name.Replace( "]", "]]" ) + "]";
                default: return Name;
            }
        }

        public override void WriteWithoutTrivias( ISqlTextWriter w ) => w.Write( ToString() );

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor ) => visitor.Visit( this );

    }


}
