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
    /// <summary>
    /// Specific <see cref="SqlTokenTerminal"/> for <see cref="SqlTokenType.DoubleDots"/> and <see cref="SqlTokenType.TripleDots"/>.
    /// </summary>
    public sealed class SqlTokenMultiDots : SqlTokenTerminal, ISqlTokenIdentifierSeparator
    {
        public SqlTokenMultiDots( SqlTokenType tokenType, ImmutableList<SqlTrivia> leadingTrivia = null, ImmutableList<SqlTrivia> trailingTrivia = null )
            : base( tokenType, leadingTrivia, trailingTrivia )
        {
            if( tokenType != SqlTokenType.DoubleDots && tokenType != SqlTokenType.TripleDots ) throw new ArgumentException();
        }

        public override void WriteWithoutTrivias( ISqlTextWriter w )
        {
            Debug.Assert( SqlKeyword.ToString( SqlTokenType.DoubleDots ) == ".." );
            Debug.Assert( SqlKeyword.ToString( SqlTokenType.TripleDots ) == "..." );
            w.Write( TokenType, ToString(), whiteSpaceBefore: false, whiteSpaceAfter: false );
        }

        public override string ToString() => TokenType == SqlTokenType.DoubleColons ? ".." : "...";

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlTokenMultiDots( TokenType, leading, trailing );
        }
    }

}
