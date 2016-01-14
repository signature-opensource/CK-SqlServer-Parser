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
    /// Specific <see cref="SqlTokenTerminal"/> for <see cref="SqlTokenType.Comma"/>.
    /// </summary>
    public sealed class SqlTokenComma : SqlTokenTerminal 
    {
        public SqlTokenComma( ImmutableList<SqlTrivia> leadingTrivia = null, ImmutableList<SqlTrivia> trailingTrivia = null )
            : base( SqlTokenType.Comma, leadingTrivia, trailingTrivia )
        {
        }

        public override void WriteWithoutTrivias( ISqlTextWriter w )
        {
            Debug.Assert( SqlKeyword.ToString( SqlTokenType.Comma ) == "," );
            w.Write( ",", whiteSpaceBefore: false, whiteSpaceAfter: null );
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlTokenComma( leading, trailing );
        }
    }

}
