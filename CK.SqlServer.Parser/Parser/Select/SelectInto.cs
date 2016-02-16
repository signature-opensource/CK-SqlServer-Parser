using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using CK.Core;
using System.Collections.Immutable;

namespace CK.SqlServer.Parser
{
    /// <summary>
    /// Captures the optional "INTO table".
    /// </summary>
    public sealed class SelectInto : SqlNonToken
    {
        readonly SNode<SqlTokenIdentifier, ISqlIdentifier> _content;

        public SelectInto( SqlTokenIdentifier intoToken, ISqlIdentifier tableName )
            : base( null, null )
        {
            _content = new SNode<SqlTokenIdentifier, ISqlIdentifier>( intoToken, tableName );
            CheckContent();
        }

        void CheckContent()
        {
            Helper.CheckToken( IntoT, nameof( IntoT ), SqlTokenType.Into );
            Helper.CheckNotNull( TableName, nameof( TableName ) );
        }

        SelectInto( SelectInto o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _content = o._content;
            else
            {
                _content = new SNode<SqlTokenIdentifier, ISqlIdentifier>( items );
                CheckContent();
            }
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return new SelectInto( this, leading, content, trailing );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

        public SqlTokenIdentifier IntoT => _content.V1;

        public ISqlIdentifier TableName => _content.V2;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

    }


}
