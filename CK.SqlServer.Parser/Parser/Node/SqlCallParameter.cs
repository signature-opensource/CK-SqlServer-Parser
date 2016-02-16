using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;

namespace CK.SqlServer.Parser
{
    /// <summary>
    /// Captures a select column definition: it is either 'definition as name', 'name = definition' or the definition alone.
    /// The horrible syntax 'definition name' is also supported.
    /// </summary>
    public sealed class SqlCallParameter : SqlNonToken
    {
        readonly SNode<SqlTokenIdentifier, SqlTokenTerminal, ISqlNode> _content;

        public SqlCallParameter( SqlTokenIdentifier name, SqlTokenTerminal assignT, ISqlNode value )
            : base( null, null )
        {
            _content = new SNode<SqlTokenIdentifier, SqlTokenTerminal, ISqlNode>( name, assignT, value );
            CheckContent();
        }

        void CheckContent()
        {
            Helper.CheckNullableToken( Name, nameof( Name ), SqlTokenType.IdentifierVariable );
            Helper.CheckNullableToken( AsssignT, nameof( AsssignT ), SqlTokenType.Assign );
            Helper.CheckBothNullOrNot( Name, nameof( Name ), AsssignT, nameof( AsssignT ) );
            Helper.CheckNotNull( Value, nameof( Value ) );
        }

        SqlCallParameter( SqlCallParameter o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _content = o._content;
            else
            {
                _content = new SNode<SqlTokenIdentifier, SqlTokenTerminal, ISqlNode>( items );
                CheckContent();
            }
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlCallParameter( this, leading, content, trailing );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

        public SqlTokenIdentifier Name => _content.V1;

        public SqlToken AsssignT => _content.V2;

        public ISqlNode Value => _content.V3;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

    }


}
