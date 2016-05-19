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
    /// </summary>
    public sealed class SqlWithForXml : SqlNonToken, ISqlNamedStatement
    {
        readonly SNode<SqlTokenIdentifier, SqlTokenIdentifier, SqlEnclosedCommaList, ISqlStatement> _content;

        public SqlWithForXml( 
                SqlTokenIdentifier withT,
                SqlTokenIdentifier xmlNamespacesT,
                SqlEnclosedCommaList ns,
                ISqlStatement outerStatement )
            : base( null, null )
        {
            _content = new SNode<SqlTokenIdentifier, SqlTokenIdentifier, SqlEnclosedCommaList, ISqlStatement>( withT, xmlNamespacesT, ns, outerStatement );
            CheckContent();
        }

        void CheckContent()
        {
            Helper.CheckToken( WithT, nameof( WithT ), SqlTokenType.With );
            Helper.CheckToken( XmlNamespacesT, nameof( XmlNamespacesT ), SqlTokenType.XmlNamespaces );
            Helper.CheckNotNull( Namespaces, nameof( Namespaces ) );
            Helper.CheckNotNull( OuterStatement, nameof( OuterStatement ) );
        }

        SqlWithForXml( SqlWithForXml o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _content = o._content;
            else
            {
                _content = new SNode<SqlTokenIdentifier, SqlTokenIdentifier, SqlEnclosedCommaList, ISqlStatement>( items );
                CheckContent();
            }
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlWithForXml( this, leading, content, trailing );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

        public StatementKnownName StatementKnownName => StatementKnownName.WithForXml;

        public SqlTokenTerminal StatementTerminator => OuterStatement.StatementTerminator;

        public SqlTokenIdentifier WithT => _content.V1;

        public SqlTokenIdentifier XmlNamespacesT => _content.V2;

        public SqlEnclosedCommaList Namespaces => _content.V3;

        public ISqlStatement OuterStatement => _content.V4;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

    }


}
