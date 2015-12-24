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
    public sealed class SqlIf : SqlNode, ISqlStatement
    {
        readonly SNode<
            SqlTokenIdentifier, 
            ISqlNode, 
            ISqlStatement, 
            SqlTokenIdentifier, 
            ISqlStatement, 
            SqlTokenTerminal> _content;

        public SqlIf( 
                SqlTokenIdentifier ifToken, 
                ISqlNode condition, 
                ISqlStatement thenStatement, 
                SqlTokenIdentifier elseToken, 
                ISqlStatement elseStatement, 
                SqlTokenTerminal terminator )
            : base( null, null )
        {
            _content = new SNode<SqlTokenIdentifier, ISqlNode, ISqlStatement, SqlTokenIdentifier, ISqlStatement, SqlTokenTerminal>(
                ifToken, 
                condition, 
                thenStatement, 
                elseToken, 
                elseStatement, 
                terminator );
            CheckContent();
        }

        void CheckContent()
        {
            SNode.CheckToken( IfT, nameof( IfT ), SqlTokenType.If );
            SNode.CheckNotNull( Condition, nameof( Condition ) );
            SNode.CheckNotNull( ThenStatement, nameof( ThenStatement ) );
            SNode.CheckNullableToken( ElseT, nameof( ElseT ), SqlTokenType.Else );
            SNode.CheckBothNullOrNot( ElseT, nameof( ElseT ), ElseStatement, nameof(ElseStatement) );
        }

        SqlIf( SqlIf o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _content = o._content;
            else
            {
                _content = new SNode<SqlTokenIdentifier, ISqlNode, ISqlStatement, SqlTokenIdentifier, ISqlStatement, SqlTokenTerminal>( items );
                CheckContent();
            }
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IReadOnlyList<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlIf( this, leading, children, trailing );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public SqlTokenIdentifier IfT => _content.V1;

        public ISqlNode Condition => _content.V2;

        public ISqlStatement ThenStatement => _content.V3;

        public bool HasElse => _content.V4 != null;

        public SqlTokenIdentifier ElseT => _content.V4;

        public ISqlStatement ElseStatement => _content.V5;

        public SqlTokenTerminal StatementTerminator => _content.V6;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor ) => visitor.Visit( this );

    }


}
