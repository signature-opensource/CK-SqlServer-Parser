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
    /// Captures {INSERT|UPDATE} [ TOP ( expression ) [ PERCENT ] ] 
    /// </summary>
    public sealed class InsOrUpdHeader : SqlNode
    {
        readonly SNode<SqlTokenIdentifier, SqlTokenIdentifier, ISqlNode, SqlTokenIdentifier> _content;

        public InsOrUpdHeader( SqlTokenIdentifier insOrUpdT, SqlTokenIdentifier top = null, ISqlNode topExpression = null, SqlTokenIdentifier percent = null )
            : base( null, null )
        {
            _content = new SNode<SqlTokenIdentifier, SqlTokenIdentifier, ISqlNode, SqlTokenIdentifier>(
                insOrUpdT,
                top, 
                topExpression, 
                percent );
            CheckContent();
        }

        InsOrUpdHeader( InsOrUpdHeader o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _content = o._content;
            else
            {
                _content = new SNode<SqlTokenIdentifier, SqlTokenIdentifier, ISqlNode, SqlTokenIdentifier>( items );
                CheckContent();
            }
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new InsOrUpdHeader( this, leading, children, trailing );
        }

        void CheckContent()
        {
            SNode.CheckToken( InsertOrUpdateT, nameof( InsertOrUpdateT ), SqlTokenType.Insert, SqlTokenType.Update );
            SNode.CheckNullableToken( TopT, nameof( TopT ), SqlTokenType.Top );
            SNode.CheckBothNullOrNot( TopT, nameof( TopT ), TopExpression, nameof(TopExpression) );
            SNode.CheckNullableToken( PercentT, nameof( PercentT ), SqlTokenType.Percent );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public SqlTokenIdentifier InsertOrUpdateT => _content.V1;

        public bool HasTop => _content.V2 != null;

        public SqlTokenIdentifier TopT => _content.V2;

        public ISqlNode TopExpression => _content.V3;

        public SqlTokenIdentifier PercentT => _content.V4;


        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor ) => visitor.Visit( this );

    }
}
