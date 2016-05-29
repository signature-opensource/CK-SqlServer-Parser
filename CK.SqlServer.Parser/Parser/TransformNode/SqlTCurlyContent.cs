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
    /// List of one or more <see cref="ISqlNode"> enclosed in curly braces: {...}. 
    /// </summary>
    public sealed class SqlTCurlyContent : ASqlNodeEnclosableList<SqlTokenTerminal,ISqlNode,SqlTokenTerminal>, ISqlStructurallyEnclosed
    {
        public SqlTCurlyContent( SqlTokenTerminal opener, IEnumerable<ISqlNode> items, SqlTokenTerminal closer )
            : base( 1, opener, items, closer )
        {
            if( opener.TokenType != SqlTokenType.OpenCurly ) throw new ArgumentException();
            if( closer.TokenType != SqlTokenType.CloseCurly ) throw new ArgumentException();
        }

        SqlTCurlyContent( SqlTCurlyContent o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> statements, ImmutableList<SqlTrivia> trailing )
            : base( o, 1, leading, statements, trailing )
        {
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlTCurlyContent( this, leading, content, trailing );
        }

        /// <summary>
        /// Gets the content string with all its trivias.
        /// </summary>
        public string ContentString
        {
            get
            {
                ISqlTextWriter w = SqlTextWriter.CreateDefault( new StringBuilder(), true );
                for( int i = 1; i < ChildrenNodes.Count - 1; ++i ) ChildrenNodes[i].Write( w );
                return w.ToString();
            }
        }

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

    }


}
