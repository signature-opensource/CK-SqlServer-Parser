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
    /// Captures the optional "Group by ... having ..." select part.
    /// Even if it seems that "having" can exist without "group by" clause, I have not found any use of it: 
    /// I decided to subordinate the "having" to the "group by".
    /// </summary>
    public sealed class SelectGroupBy : SqlNode
    {
        readonly SNode<SqlTokenIdentifier, SqlTokenIdentifier, ISqlNode, SqlTokenIdentifier, ISqlNode> _content;

        public SelectGroupBy( SqlTokenIdentifier groupToken, SqlTokenIdentifier byT, ISqlNode groupExpression, SqlTokenIdentifier havingT = null, ISqlNode havingExpression = null )
            : base( null, null )
        {
            _content = new SNode<SqlTokenIdentifier, SqlTokenIdentifier, ISqlNode, SqlTokenIdentifier, ISqlNode>(
                groupToken, 
                byT, 
                groupExpression, 
                havingT, 
                havingExpression );
            CheckContent();
        }

        void CheckContent()
        {
            SNode.CheckToken( GroupT, nameof( GroupT ), SqlTokenType.Group );
            SNode.CheckToken( ByT, nameof( ByT ), SqlTokenType.By );
            SNode.CheckNotNull( GroupExpression, nameof( GroupExpression ) );
            SNode.CheckNullableToken( HavingT, nameof( HavingT ), SqlTokenType.Having );
            SNode.CheckBothNullOrNot( HavingT, nameof( HavingT ), HavingExpression, nameof( HavingExpression ) );
        }

        SelectGroupBy( SelectGroupBy o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _content = o._content;
            else
            {
                _content = new SNode<SqlTokenIdentifier, SqlTokenIdentifier, ISqlNode, SqlTokenIdentifier, ISqlNode>( items );
                CheckContent();
            }
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IReadOnlyList<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SelectGroupBy( this, leading, children, trailing );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public SqlTokenIdentifier GroupT => _content.V1;

        public SqlTokenIdentifier ByT => _content.V2;

        public ISqlNode GroupExpression => _content.V3;

        public SqlTokenIdentifier HavingT => _content.V4;

        public ISqlNode HavingExpression => _content.V5;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor ) => visitor.Visit( this );

    }


}
