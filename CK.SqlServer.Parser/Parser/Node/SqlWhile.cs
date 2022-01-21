using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CK.SqlServer.Parser
{
    using CNode = SNode<SqlTokenIdentifier, ISqlNode, ISqlStatement, SqlTokenTerminal>;

    /// <summary>
    /// 
    /// </summary>
    public sealed class SqlWhile : SqlNonTokenAutoWidth, ISqlNamedStatement
    {
        readonly CNode _content;

        public SqlWhile( SqlTokenIdentifier whileT, ISqlNode condition, ISqlStatement statement, SqlTokenTerminal terminator )
            : base( null, null )
        {
            _content = new CNode( whileT, condition, statement, terminator );
            CheckContent();
        }

        void CheckContent()
        {
            Helper.CheckToken( WhileT, nameof( WhileT ), SqlTokenType.While );
            Helper.CheckNotNull( Condition, nameof( Condition ) );
            Helper.CheckNotNull( Statement, nameof( Statement ) );
        }

        SqlWhile( SqlWhile o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
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
            return new SqlWhile( this, leading, content, trailing );
        }

        public StatementKnownName StatementKnownName => StatementKnownName.While;

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

        public SqlTokenIdentifier WhileT => _content.V1;

        public ISqlNode Condition => _content.V2;

        public ISqlStatement Statement => _content.V3;

        public SqlTokenTerminal StatementTerminator => _content.V4;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

    }


}
