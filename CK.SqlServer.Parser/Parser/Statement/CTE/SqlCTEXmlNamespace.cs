using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CK.SqlServer.Parser
{
    using CNode = SNode<
            SqlTokenIdentifier,
            SqlEnclosedCommaList,
            SqlTokenComma>;

    /// <summary>
    /// Defines the optional first "XMLNAMESPACES ('ns' as json)," in a <see cref="SqlCTEStatement"/>.
    /// </summary>
    public sealed class SqlCTEXmlNamespace : SqlNonTokenAutoWidth
    {
        readonly CNode _content;

        public SqlCTEXmlNamespace( SqlTokenIdentifier xmlNamespacesT,
                                   SqlEnclosedCommaList ns,
                                   SqlTokenComma commaT )
            : base( null, null )
        {
            _content = new CNode( xmlNamespacesT, ns, commaT );
            CheckContent();
        }

        void CheckContent()
        {
            Helper.CheckToken( XmlNamespacesT, nameof( XmlNamespacesT ), SqlTokenType.XmlNamespaces );
            Helper.CheckNotNull( Namespaces, nameof( Namespaces ) );
            Helper.CheckNotNull( CommaT, nameof( CommaT ) );
        }

        SqlCTEXmlNamespace( SqlCTEXmlNamespace o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
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
            return new SqlCTEXmlNamespace( this, leading, content, trailing );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

        public SqlTokenIdentifier XmlNamespacesT => _content.V1;

        public SqlEnclosedCommaList Namespaces => _content.V2;

        public SqlTokenComma CommaT => _content.V3;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

    }


}
