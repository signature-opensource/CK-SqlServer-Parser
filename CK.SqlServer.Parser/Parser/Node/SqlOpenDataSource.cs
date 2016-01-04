using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using CK.Core;
using System.Collections.Immutable;

namespace CK.SqlServer.Parser
{
    public sealed class SqlOpenDataSource : SqlNode, ISqlIdentifier
    {
        readonly SNode<SqlTokenIdentifier, SqlEnclosedCommaList> _content;

        public SqlOpenDataSource( SqlTokenIdentifier openDataSourceT, SqlEnclosedCommaList parameters )
            : base( null, null )
        {
            _content = new SNode<SqlTokenIdentifier, SqlEnclosedCommaList>( openDataSourceT, parameters );
            CheckContent();
        }

        void CheckContent()
        {
            SNode.CheckToken( OpenDataSourceT, nameof( OpenDataSourceT ), SqlTokenType.OpenDataSource );
            SNode.CheckNotNull( Parameters, nameof( Parameters ) );
        }

        SqlOpenDataSource( SqlOpenDataSource o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _content = o._content;
            else
            {
                _content = new SNode<SqlTokenIdentifier, SqlEnclosedCommaList>( items );
                CheckContent();
            }
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlOpenDataSource( this, leading, children, trailing );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public SqlTokenIdentifier OpenDataSourceT => _content.V1;

        public SqlEnclosedCommaList Parameters => _content.V2;

        public IReadOnlyList<ISqlIdentifier> Identifiers => ((ISqlIdentifier)OpenDataSourceT).Identifiers;

        bool ISqlIdentifier.IsVariable => false;

        public bool IsOpenDataSouce => true;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor ) => visitor.Visit( this );

    }
}
