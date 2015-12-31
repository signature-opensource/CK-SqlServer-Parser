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
    /// Specific <see cref="SqlTokenTerminal"/> for <see cref="SqlTokenType.ClosePar"/>.
    /// </summary>
    public sealed class SqlTokenClosePar : SqlTokenTerminal 
    {
        public SqlTokenClosePar( ImmutableList<SqlTrivia> leadingTrivia = null, ImmutableList<SqlTrivia> trailingTrivia = null )
            : base( SqlTokenType.ClosePar, leadingTrivia, trailingTrivia )
        {
        }

        public override void WriteWithoutTrivias( ISqlTextWriter w )
        {
            Debug.Assert( SqlTokenizer.Explain( SqlTokenType.ClosePar ) == ")" );
            w.Write( ")", whiteSpaceBefore: false, whiteSpaceAfter : null );
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlTokenClosePar( leading, trailing );
        }

    }

}
