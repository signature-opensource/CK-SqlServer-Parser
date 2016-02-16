using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using CK.Core;
using System.Collections.Immutable;

namespace CK.SqlServer.Parser
{
    public class SqlCast : SqlNonToken
    {
        readonly SNode<SqlTokenIdentifier, SqlTokenOpenPar, ISqlNode, SqlTokenIdentifier, ISqlUnifiedTypeDecl, SqlTokenClosePar> _content;

        public SqlCast( SqlTokenIdentifier castT, SqlTokenOpenPar opener, ISqlNode e, SqlTokenIdentifier asT, ISqlUnifiedTypeDecl type, SqlTokenClosePar closer )
            : base( null, null )
        {
            _content = new SNode<SqlTokenIdentifier, SqlTokenOpenPar, ISqlNode, SqlTokenIdentifier, ISqlUnifiedTypeDecl, SqlTokenClosePar>(
                castT, 
                opener, 
                e, 
                asT, 
                type, 
                closer );
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
            Helper.CheckToken( CastT, nameof( CastT ), SqlTokenType.Cast );
            Helper.CheckNotNull( Opener, nameof( Opener ) );
            Helper.CheckToken( AsT, nameof( AsT ), SqlTokenType.As );
            Helper.CheckNotNull( Type, nameof( Type ) );
            Helper.CheckNotNull( Closer, nameof( Closer ) );
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlCast( this, leading, content, trailing );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

        public SqlTokenIdentifier CastT => _content.V1;

        public SqlTokenOpenPar Opener => _content.V2;

        public ISqlNode Expression => _content.V3;

        public SqlTokenIdentifier AsT => _content.V4;

        public ISqlUnifiedTypeDecl Type => _content.V5;

        public SqlTokenClosePar Closer => _content.V6;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );
    }
}
