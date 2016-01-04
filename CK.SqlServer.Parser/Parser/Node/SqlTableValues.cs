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

        public SqlTableValues( SqlTokenIdentifier valuesT, SqlMultiCommaList values )
            : base( null, null )
        {
            _content = new SNode<SqlTokenIdentifier, SqlMultiCommaList>( valuesT, values );
            CheckContent();
        }

        void CheckContent()
        {
            SNode.CheckToken( ValuesT, nameof( ValuesT ), SqlTokenType.Values );
            SNode.CheckNotNull( Values, nameof( Values ) );
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

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlTableValues( this, leading, children, trailing );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public SqlTokenIdentifier ValuesT => _content.V1;

        public SqlMultiCommaList Values => _content.V2;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor ) => visitor.Visit( this );

    }
}
