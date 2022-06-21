using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CK.SqlServer.Parser
{
    public sealed class SqlOpenDataSource : SqlNonTokenAutoWidth, ISqlIdentifier
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
            Helper.CheckToken( OpenDataSourceT, nameof( OpenDataSourceT ), SqlTokenType.OpenDataSource );
            Helper.CheckNotNull( Parameters, nameof( Parameters ) );
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

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlOpenDataSource( this, leading, content, trailing );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

        public SqlTokenIdentifier OpenDataSourceT => _content.V1;

        public ISqlIdentifier SetPartName( int idxPart, string name )
        {
            throw new InvalidOperationException();
        }

        public SqlEnclosedCommaList Parameters => _content.V2;

        public IReadOnlyList<ISqlIdentifier> Identifiers => ((ISqlIdentifier)OpenDataSourceT).Identifiers;

        bool ISqlIdentifier.IsVariable => false;

        public bool IsOpenDataSource => true;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

    }
}
