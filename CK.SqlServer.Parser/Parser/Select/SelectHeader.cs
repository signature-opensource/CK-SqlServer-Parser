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
    /// Captures SELECT [ ALL | DISTINCT ] [TOP ( expression ) [PERCENT] [ WITH TIES ] ] 
    /// </summary>
    public sealed class SelectHeader : SqlNode
    {
        readonly SNode<SqlTokenIdentifier, SqlTokenIdentifier, SqlTokenIdentifier, ISqlNode, SqlTokenIdentifier, SqlTokenIdentifier, SqlTokenIdentifier> _content;

        public SelectHeader( SqlTokenIdentifier select, SqlTokenIdentifier allOrDistinct = null, SqlTokenIdentifier top = null, ISqlNode topExpression = null, SqlTokenIdentifier percent = null, SqlTokenIdentifier with = null, SqlTokenIdentifier ties = null )
            : base( null, null )
        {
            _content = new SNode<SqlTokenIdentifier, SqlTokenIdentifier, SqlTokenIdentifier, ISqlNode, SqlTokenIdentifier, SqlTokenIdentifier, SqlTokenIdentifier>(
                select, 
                allOrDistinct, 
                top, 
                topExpression, 
                percent, 
                with, 
                ties );
            CheckContent();
        }

        SelectHeader( SelectHeader o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _content = o._content;
            else
            {
                _content = new SNode<SqlTokenIdentifier, SqlTokenIdentifier, SqlTokenIdentifier, ISqlNode, SqlTokenIdentifier, SqlTokenIdentifier, SqlTokenIdentifier>( items );
                CheckContent();
            }
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return new SelectHeader( this, leading, content, trailing );
        }

        void CheckContent()
        {
            Helper.CheckToken( SelectT, nameof( SelectT ), SqlTokenType.Select );
            Helper.CheckNullableToken( AllOrDistinctT, nameof( AllOrDistinctT ), SqlTokenType.All, SqlTokenType.Distinct );
            Helper.CheckNullableToken( TopT, nameof( TopT ), SqlTokenType.Top );
            Helper.CheckBothNullOrNot( TopT, nameof( TopT ), TopExpression, nameof(TopExpression) );
            Helper.CheckNullableToken( PercentT, nameof( PercentT ), SqlTokenType.Percent );
            Helper.CheckNullableToken( WithT, nameof( WithT ), SqlTokenType.With );
            Helper.CheckNullableToken( TiesT, nameof( TiesT ), SqlTokenType.Ties );
            Helper.CheckBothNullOrNot( WithT, nameof( WithT ), TiesT, nameof( TiesT ) );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

        public SqlTokenIdentifier SelectT => _content.V1;

        public SqlTokenIdentifier AllOrDistinctT => _content.V2;

        public SqlTokenIdentifier TopT => _content.V3;

        public ISqlNode TopExpression => _content.V4;

        public SqlTokenIdentifier PercentT => _content.V5;

        public SqlTokenIdentifier WithT => _content.V6;

        public SqlTokenIdentifier TiesT => _content.V7;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

    }
}
