using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;

namespace CK.SqlServer.Parser
{
    /// <summary>
    /// Captures a select column definition. 
    /// </summary>
    public sealed class SqlOverClause : SqlNode
    {
        readonly SNode<SqlTokenIdentifier, SqlTokenOpenPar, SqlNodeList, SqlTokenClosePar> _content;

        public SqlOverClause( SqlTokenIdentifier overT, SqlTokenOpenPar openPar, SqlNodeList overExpression, SqlTokenClosePar closePar )
            : base( null, null )
        {
            _content = new SNode<SqlTokenIdentifier, SqlTokenOpenPar, SqlNodeList, SqlTokenClosePar>( overT, openPar, overExpression, closePar );
            CheckContent();
        }

        void CheckContent()
        {
            SNode.CheckToken( OverT, nameof( OverT ), SqlTokenType.Over );
            SNode.CheckNotNull( OpenPar, nameof( OpenPar ) );
            SNode.CheckNotNull( OverContent, nameof( OverContent ) );
            SNode.CheckNotNull( ClosePar, nameof( ClosePar ) );
        }

        SqlOverClause( SqlOverClause o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _content = o._content;
            else
            {
                _content = new SNode<SqlTokenIdentifier, SqlTokenOpenPar, SqlNodeList, SqlTokenClosePar>( items );
                CheckContent();
            }
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IReadOnlyList<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlOverClause( this, leading, children, trailing );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public SqlTokenIdentifier OverT => _content.V1;

        public SqlTokenOpenPar OpenPar => _content.V2;

        public SqlNodeList OverContent => _content.V3;

        public SqlTokenClosePar ClosePar => _content.V4;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor ) => visitor.Visit( this );

    }

}
