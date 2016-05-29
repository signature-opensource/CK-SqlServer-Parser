using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using CK.Core;
using System.Collections.Immutable;
using CK.Text;

namespace CK.SqlServer.Parser
{
    /// <summary>
    /// List of one or more <see cref="SqlToken"> enclosed in curly braces: {...}. 
    /// </summary>
    public sealed class SqlTCurlyPattern : ASqlNodeEnclosableList<SqlTokenTerminal,SqlToken,SqlTokenTerminal>, ISqlStructurallyEnclosed
    {
        public SqlTCurlyPattern( SqlTokenTerminal opener, IEnumerable<SqlToken> items, SqlTokenTerminal closer )
            : base( 1, opener, items, closer )
        {
            if( opener.TokenType != SqlTokenType.OpenCurly ) throw new ArgumentException();
            if( closer.TokenType != SqlTokenType.CloseCurly ) throw new ArgumentException();
        }

        SqlTCurlyPattern( SqlTCurlyPattern o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> statements, ImmutableList<SqlTrivia> trailing )
            : base( o, 1, leading, statements, trailing )
        {
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlTCurlyPattern( this, leading, content, trailing );
        }

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

    }


}
