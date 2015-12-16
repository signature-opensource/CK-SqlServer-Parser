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
    /// Specific <see cref="SqlTokenTerminal"/> for <see cref="SqlTokenType.OpenPar"/>.
    /// </summary>
    public sealed class SqlTokenOpenPar : SqlTokenTerminal 
    {
        public SqlTokenOpenPar( ImmutableList<SqlTrivia> leadingTrivia = null, ImmutableList<SqlTrivia> trailingTrivia = null )
            : base( SqlTokenType.OpenPar, leadingTrivia, trailingTrivia )
        {
        }

        public override void WriteWithoutTrivias( ISqlTextWriter w )
        {
            Debug.Assert( SqlTokenizer.Explain( SqlTokenType.OpenPar ) == "(" );
            w.Write( "(", whiteSpaceBefore: false, whiteSpaceAfter: false );
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IReadOnlyList<SqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlTokenOpenPar( leading, trailing );
        }

    }

}
