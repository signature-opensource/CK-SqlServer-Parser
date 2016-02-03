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
    /// Captures the optional "For xml, browse, json or system_time" select part.
    /// </summary>
    public sealed class SelectFor : SqlNode
    {
        readonly SNode<SqlTokenIdentifier, SqlTokenIdentifier, ISqlNode> _content;

        public SelectFor( SqlTokenIdentifier forT, SqlTokenIdentifier targetType, ISqlNode content )
            : base( null, null )
        {
            _content = new SNode<SqlTokenIdentifier, SqlTokenIdentifier, ISqlNode>( forT, targetType, content );
            CheckContent();
        }

        void CheckContent()
        {
            Helper.CheckToken( ForT, nameof( ForT ), SqlTokenType.For );
            Helper.CheckToken( TargetType, nameof( TargetType ),
                SqlTokenType.XmlDbType,
                SqlTokenType.Browse,
                SqlTokenType.Json,
                SqlTokenType.SystemTime );
            Helper.CheckNotNull( TargetType, nameof( TargetType ) );
            Helper.CheckNotNull( Format, nameof(Format) );
        }

        SelectFor( SelectFor o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _content = o._content;
            else
            {
                _content = new SNode<SqlTokenIdentifier, SqlTokenIdentifier, ISqlNode>( items );
                CheckContent();
            }
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return new SelectFor( this, leading, content, trailing );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

        public SqlTokenIdentifier ForT => _content.V1;

        public SqlTokenIdentifier TargetType => _content.V2;

        public ISqlNode Format => _content.V3;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

    }


}
