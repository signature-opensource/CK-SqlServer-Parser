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
    /// Captures {INSERT|UPDATE|MERGE|DELETE} [ TOP ( expression ) [ PERCENT ] ] 
    /// </summary>
    public sealed class MIUDHeader : SqlNode
    {
        readonly SNode<SqlTokenIdentifier, SqlTokenIdentifier, ISqlNode, SqlTokenIdentifier> _content;

        public MIUDHeader( SqlTokenIdentifier statementT, SqlTokenIdentifier top = null, ISqlNode topExpression = null, SqlTokenIdentifier percent = null )
            : base( null, null )
        {
            _content = new SNode<SqlTokenIdentifier, SqlTokenIdentifier, ISqlNode, SqlTokenIdentifier>(
                statementT,
                top, 
                topExpression, 
                percent );
            CheckContent();
        }

        MIUDHeader( MIUDHeader o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
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
            return new MIUDHeader( this, leading, children, trailing );
        }

        void CheckContent()
        {
            Helper.CheckToken( StatementT, nameof( StatementT ), SqlTokenType.Insert, SqlTokenType.Update, SqlTokenType.Merge, SqlTokenType.Delete );
            Helper.CheckNullableToken( TopT, nameof( TopT ), SqlTokenType.Top );
            Helper.CheckBothNullOrNot( TopT, nameof( TopT ), TopExpression, nameof(TopExpression) );
            Helper.CheckNullableToken( PercentT, nameof( PercentT ), SqlTokenType.Percent );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

        public SqlTokenIdentifier StatementT => _content.V1;

        public bool HasTop => _content.V2 != null;

        public SqlTokenIdentifier TopT => _content.V2;

        public ISqlNode TopExpression => _content.V3;

        public SqlTokenIdentifier PercentT => _content.V4;


        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor ) => visitor.Visit( this );

    }
}
