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
            Debug.Assert( SqlKeyword.ToString( SqlTokenType.ClosePar ) == ")" );
            w.Write( SqlTokenType.ClosePar, ")", whiteSpaceBefore: false, whiteSpaceAfter : null );
        }

        public override string ToString() => ")";

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlTokenClosePar( leading, trailing );
        }

    }

}
