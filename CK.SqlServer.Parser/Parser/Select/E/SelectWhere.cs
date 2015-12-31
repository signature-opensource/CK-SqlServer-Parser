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
    /// Captures the optional "Where ..." select part.
    /// </summary>
    public sealed class SelectWhere : SqlNode
    {
        readonly SNode<SqlTokenIdentifier, ISqlNode> _content;

        public SelectWhere( SqlTokenIdentifier whereT, ISqlNode expression )
            : base( null, null )
        {
            _content = new SNode<SqlTokenIdentifier, ISqlNode>( whereT, expression );
            CheckContent();
        }

        void CheckContent()
        {
            SNode.CheckToken( WhereT, nameof( WhereT ), SqlTokenType.Where );
            SNode.CheckNotNull( Expression, nameof( Expression ) );
        }

        SelectWhere( SelectWhere o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _content = o._content;
            else
            {
                _content = new SNode<SqlTokenIdentifier, ISqlNode>( items );
                CheckContent();
            }
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SelectWhere( this, leading, children, trailing );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public SqlTokenIdentifier WhereT => _content.V1;

        public ISqlNode Expression => _content.V2;


        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor ) => visitor.Visit( this );

    }


}
