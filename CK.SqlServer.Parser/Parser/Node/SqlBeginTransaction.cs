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
    public sealed class SqlBeginTransaction : SqlNode, ISqlNamedStatement
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
            Helper.CheckToken( BeginT, nameof( BeginT ), SqlTokenType.Begin );
            Helper.CheckToken( TranT, nameof( TranT ), SqlTokenType.Transaction );
            Helper.CheckNullableToken( WithT, nameof( WithT ), SqlTokenType.With );
            Helper.CheckNullableToken( MarkT, nameof( MarkT ), SqlTokenType.Mark );
            Helper.CheckBothNullOrNot( WithT, nameof( WithT ), MarkT, nameof( MarkT ) );
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlBeginTransaction( this, leading, content, trailing );
        }

        public StatementKnownName StatementKnownName => StatementKnownName.BeginTransaction;

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

        public SqlTokenIdentifier BeginT => _content.V1;

        public SqlTokenIdentifier TranT => _content.V2;

        public SqlTokenIdentifier TranNameOrVariable => _content.V3;

        public SqlTokenIdentifier WithT => _content.V4;

        public SqlTokenIdentifier MarkT => _content.V5;

        public SqlTokenLiteralString Description => _content.V6;

        public SqlTokenTerminal StatementTerminator => _content.V7;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

    }


}
