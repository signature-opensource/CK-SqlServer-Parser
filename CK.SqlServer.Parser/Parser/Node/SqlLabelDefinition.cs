using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CK.SqlServer.Parser
{
    /// <summary>
    /// Label definition (a target for the goto).
    /// </summary>
    public sealed class SqlLabelDefinition : SqlNonTokenAutoWidth, ISqlNamedStatement
    {
        readonly SNode<SqlTokenIdentifier, SqlTokenTerminal> _content;

        public SqlLabelDefinition( SqlTokenIdentifier id, SqlTokenTerminal colon )
            : base( null, null )
        {
            _content = new SNode<SqlTokenIdentifier, SqlTokenTerminal>( id, colon );
            CheckContent();
        }

        void CheckContent()
        {
            if( IdentifierT == null
                || IdentifierT.TokenType.IsQuotedIdentifier()
                || SqlKeyword.IsReservedKeyword( IdentifierT.Name )
                || IdentifierT.TrailingTrivias.Count > 0
                || Colon == null
                || Colon.TokenType != SqlTokenType.Colon
                || Colon.LeadingTrivias.Count > 0 )
            {
                throw new ArgumentException( "Invalid 'label:' definition." );
            }
        }

        SqlLabelDefinition( SqlLabelDefinition o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _content = o._content;
            else
            {
                _content = new SNode<SqlTokenIdentifier, SqlTokenTerminal>( items );
                CheckContent();
            }
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlLabelDefinition( this, leading, content, trailing );
        }

        public StatementKnownName StatementKnownName => StatementKnownName.LabelDefinition;

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();


        public SqlTokenIdentifier IdentifierT => _content.V1;

        public SqlTokenTerminal Colon => _content.V2;

        SqlTokenTerminal ISqlStatement.StatementTerminator => null;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

    }


}
