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
    using CNode = SNode<SqlTypedIdentifier, SqlTokenTerminal, ISqlNode>;

    public sealed class SqlVariableDeclaration : SqlNonTokenAutoWidth
    {
        readonly CNode _content;

        public SqlVariableDeclaration( SqlTypedIdentifier declVar, SqlTokenTerminal assignToken = null, ISqlNode initialValue = null )
            : base( null, null )
        {
            _content = new CNode( declVar, assignToken, initialValue );
            CheckContent();
        }

        void CheckContent()
        {
            Helper.CheckIsVariable( Variable, nameof( Variable ) );
            Helper.CheckNullableToken( AssignT, nameof( AssignT ), SqlTokenType.Assign );
            Helper.CheckBothNullOrNot( AssignT, nameof( AssignT ), InitialValue, nameof( InitialValue ) );
        }

        SqlVariableDeclaration( SqlVariableDeclaration o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _content = o._content;
            else
            {
                _content = new CNode( items );
                CheckContent();
            }
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlVariableDeclaration( this, leading, content, trailing );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

        public SqlTypedIdentifier Variable => _content.V1;

        public SqlTokenTerminal AssignT => _content.V2;

        public ISqlNode InitialValue => _content.V3;

        public bool HasInitialValue => _content.V3 != null;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

    }

}
