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
    /// Specific <see cref="SqlTokenTerminal"/> for <see cref="SqlTokenType.Dot"/>.
    /// </summary>
    public sealed class SqlTokenDot : SqlTokenTerminal, ISqlTokenIdentifierSeparator
    {
        public SqlTokenDot( ImmutableList<SqlTrivia> leadingTrivia = null, ImmutableList<SqlTrivia> trailingTrivia = null )
            : base( SqlTokenType.Dot, leadingTrivia, trailingTrivia )
        {
        }

        public override void WriteWithoutTrivias( ISqlTextWriter w )
        {
            Debug.Assert( SqlKeyword.ToString( SqlTokenType.Dot ) == "." );
            w.Write( SqlTokenType.Dot, ".", whiteSpaceBefore: false, whiteSpaceAfter: false );
        }

        public override string ToString() => ".";

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlTokenDot( leading, trailing );
        }
    }

}
