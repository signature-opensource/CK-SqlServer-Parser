using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CK.SqlServer.Parser
{
    /// <summary>
    /// Defines "next value for {sequence}>" expression.
    /// </summary>
    public sealed class SqlCTEStatement : SqlNonTokenAutoWidth, ISqlNamedStatement
    {
        readonly SNode<SqlTokenIdentifier, SqlCTEXmlNamespace,SqlCTENameList, ISqlStatement> _content;

        public SqlCTEStatement( SqlTokenIdentifier withT,
                                SqlCTEXmlNamespace namespaces,
                                SqlCTENameList names,
                                ISqlStatement outerStatement )
            : base( null, null )
        {
            _content = new SNode<SqlTokenIdentifier, SqlCTEXmlNamespace, SqlCTENameList, ISqlStatement>( withT, namespaces, names, outerStatement );
            CheckContent();
        }

        void CheckContent()
        {
            Helper.CheckToken( WithT, nameof( WithT ), SqlTokenType.With );
            Helper.CheckNotNull( Names, nameof( Names ) );
            Helper.CheckNotNull( OuterStatement, nameof( OuterStatement ) );
        }

        SqlCTEStatement( SqlCTEStatement o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _content = o._content;
            else
            {
                _content = new SNode<SqlTokenIdentifier, SqlCTEXmlNamespace, SqlCTENameList, ISqlStatement>( items );
                CheckContent();
            }
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlCTEStatement( this, leading, content, trailing );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

        public StatementKnownName StatementKnownName => StatementKnownName.CTE;

        public SqlTokenTerminal StatementTerminator => OuterStatement.StatementTerminator;

        public SqlTokenIdentifier WithT => _content.V1;

        public SqlCTEXmlNamespace XmlNamespaces => _content.V2;

        public bool HasXmlNamespaces => _content.V2 != null;

        public SqlCTENameList Names => _content.V3;

        public ISqlStatement OuterStatement => _content.V4;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

    }


}
