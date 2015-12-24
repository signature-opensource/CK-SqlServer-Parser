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
    public sealed class SqlDeclareVariable : SqlNode
    {
        readonly SNode<SqlTypedIdentifier, SqlTokenTerminal, ISqlNode> _content;

        public SqlDeclareVariable( SqlTypedIdentifier declVar, SqlTokenTerminal assignToken = null, ISqlNode initialValue = null )
            : base( null, null )
        {
            _content = new SNode<SqlTypedIdentifier, SqlTokenTerminal, ISqlNode>( declVar, assignToken, initialValue );
            CheckContent();
        }

        void CheckContent()
        {
            SNode.CheckIsVariable( Variable, nameof( Variable ) );
            SNode.CheckToken( AssignT, nameof( AssignT ), SqlTokenType.Assign );
            SNode.CheckBothNullOrNot( AssignT, nameof( AssignT ), InitialValue, nameof( InitialValue ) );
        }

        SqlDeclareVariable( SqlDeclareVariable o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _content = o._content;
            else
            {
                _content = new SNode<SqlTypedIdentifier, SqlTokenTerminal, ISqlNode>( items );
                CheckContent();
            }
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IReadOnlyList<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlDeclareVariable( this, leading, children, trailing );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public SqlTypedIdentifier Variable => _content.V1;

        public SqlTokenTerminal AssignT => _content.V2;

        public ISqlNode InitialValue => _content.V3;

        public bool HasInitialValue => _content.V3 != null;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor ) => visitor.Visit( this );

    }

}
