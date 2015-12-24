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
    /// 
    /// </summary>
    public sealed class SqlBeginTransaction : SqlNode, ISqlStatement
    {
        readonly SNode<SqlTokenIdentifier, SqlTokenIdentifier, SqlTokenIdentifier, SqlTokenIdentifier, SqlTokenIdentifier, SqlTokenLiteralString, SqlTokenTerminal> _content;

        public SqlBeginTransaction( SqlTokenIdentifier begin, SqlTokenIdentifier tranToken, SqlTokenIdentifier tranNameOrVariable, SqlTokenIdentifier withToken, SqlTokenIdentifier markToken, SqlTokenLiteralString description, SqlTokenTerminal terminator )
            : base( null, null )
        {
            _content = new SNode<SqlTokenIdentifier, SqlTokenIdentifier, SqlTokenIdentifier, SqlTokenIdentifier, SqlTokenIdentifier, SqlTokenLiteralString, SqlTokenTerminal>(
                begin,
                tranToken,
                tranNameOrVariable,
                withToken,
                markToken,
                description,
                terminator );
            CheckContent();
        }

        SqlBeginTransaction( SqlBeginTransaction o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _content = o._content;
            else
            {
                _content = new SNode<SqlTokenIdentifier, SqlTokenIdentifier, SqlTokenIdentifier, SqlTokenIdentifier, SqlTokenIdentifier, SqlTokenLiteralString, SqlTokenTerminal>( items );
                CheckContent();
            }
        }

        void CheckContent()
        {
            if( BeginT == null || BeginT.TokenType != SqlTokenType.Begin ) throw new ArgumentException( "begin" );
            if( TranT == null || TranT.TokenType != SqlTokenType.Transaction ) throw new ArgumentException( "tranToken" );
            if( WithT != null && WithT.TokenType != SqlTokenType.With ) throw new ArgumentException( "withToken" );
            if( WithT != null && (MarkT == null || !MarkT.NameEquals( "mark" )) ) throw new ArgumentException( "markToken" );
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IReadOnlyList<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlBeginTransaction( this, leading, children, trailing );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public SqlTokenIdentifier BeginT => _content.V1;

        public SqlTokenIdentifier TranT => _content.V2;

        public SqlTokenIdentifier TranNameOrVariable => _content.V3;

        public SqlTokenIdentifier WithT => _content.V4;

        public SqlTokenIdentifier MarkT => _content.V5;

        public SqlTokenLiteralString Description => _content.V6;

        public SqlTokenTerminal StatementTerminator => _content.V7;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor ) => visitor.Visit( this );

    }


}
