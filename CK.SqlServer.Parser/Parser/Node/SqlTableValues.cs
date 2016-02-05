using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;

namespace CK.SqlServer.Parser
{
    public sealed class SqlTableValues : SqlNode
    {
        readonly SNode<SqlTokenIdentifier, SqlMultiCommaList> _content;

        public SqlTableValues( SqlTokenIdentifier valuesT, SqlMultiCommaList values, ImmutableList<SqlTrivia> leading = null, ImmutableList<SqlTrivia> trailing = null )
            : base( leading, trailing )
        {
            _content = new SNode<SqlTokenIdentifier, SqlMultiCommaList>( valuesT, values );
            CheckContent();
        }

        void CheckContent()
        {
            Helper.CheckToken( ValuesT, nameof( ValuesT ), SqlTokenType.Values );
            Helper.CheckNotNull( Values, nameof( Values ) );
        }

        SqlTableValues( SqlTableValues o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _content = o._content;
            else
            {
                _content = new SNode<SqlTokenIdentifier, SqlMultiCommaList>( items );
                CheckContent();
            }
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlTableValues( this, leading, content, trailing );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public SqlTableValues AppendValue( ISqlNode expression ) => new SqlTableValues( ValuesT, Values.AppendValue( expression ), LeadingTrivias, TrailingTrivias );

        public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

        public SqlTokenIdentifier ValuesT => _content.V1;

        public SqlMultiCommaList Values => _content.V2;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

    }
}
