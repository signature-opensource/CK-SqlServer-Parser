using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using CK.Core;
using System.Collections.Immutable;

namespace CK.SqlServer.Parser
{
    public class SqlCast : SqlNode
    {
        readonly SNode<SqlTokenIdentifier, SqlTokenOpenPar, ISqlNode, SqlTokenIdentifier, ISqlUnifiedTypeDecl, SqlTokenClosePar> _content;

        public SqlCast( SqlTokenIdentifier castT, SqlTokenOpenPar openPar, ISqlNode e, SqlTokenIdentifier asT, ISqlUnifiedTypeDecl type, SqlTokenClosePar closePar )
            : base( null, null )
        {
            _content = new SNode<SqlTokenIdentifier, SqlTokenOpenPar, ISqlNode, SqlTokenIdentifier, ISqlUnifiedTypeDecl, SqlTokenClosePar>(
                castT, 
                openPar, 
                e, 
                asT, 
                type, 
                closePar );
            CheckContent();
        }

        SqlCast( SqlCast o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _content = o._content;
            else
            {
                _content = new SNode<SqlTokenIdentifier, SqlTokenOpenPar, ISqlNode, SqlTokenIdentifier, ISqlUnifiedTypeDecl, SqlTokenClosePar>( items );
                CheckContent();
            }
        }

        void CheckContent()
        {
            SNode.CheckToken( CastT, nameof( CastT ), SqlTokenType.Cast );
            SNode.CheckNotNull( OpenPar, nameof( OpenPar ) );
            SNode.CheckToken( AsT, nameof( AsT ), SqlTokenType.As );
            SNode.CheckNotNull( Type, nameof( Type ) );
            SNode.CheckNotNull( ClosePar, nameof( ClosePar ) );
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IReadOnlyList<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlCast( this, leading, children, trailing );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public SqlTokenIdentifier CastT => _content.V1;

        public SqlTokenOpenPar OpenPar => _content.V2;

        public ISqlNode Expression => _content.V3;

        public SqlTokenIdentifier AsT => _content.V4;

        public ISqlUnifiedTypeDecl Type => _content.V5;

        public SqlTokenClosePar ClosePar => _content.V6;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor ) => visitor.Visit( this );
    }
}
