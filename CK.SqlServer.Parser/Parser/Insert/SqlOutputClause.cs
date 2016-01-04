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

    public sealed class SqlOutputClause : SqlNode
    {
        readonly SNode<SqlTokenIdentifier, SelectColumnList, SqlTokenIdentifier, ISqlIdentifier, SqlEnclosedIdentifierCommaList> _content;

        public SqlOutputClause( 
            SqlTokenIdentifier outputT,
            SelectColumnList columns,
            SqlTokenIdentifier intoT,
            ISqlIdentifier targetTable,
            SqlEnclosedIdentifierCommaList columnNames )
            : base( null, null )
        {
            _content = new SNode<SqlTokenIdentifier, SelectColumnList, SqlTokenIdentifier, ISqlIdentifier, SqlEnclosedIdentifierCommaList>( outputT, columns, intoT, targetTable, columnNames );
            CheckContent();
        }

        void CheckContent()
        {
            SNode.CheckToken( OutputT, nameof( OutputT ), SqlTokenType.Output );
            SNode.CheckNotNull( Columns, nameof( Columns ) );
            SNode.CheckNullableToken( IntoT, nameof( IntoT ), SqlTokenType.Into );
            SNode.CheckBothNullOrNot( IntoT, nameof( IntoT ), TargetTable, nameof( TargetTable ) );
        }

        SqlOutputClause( SqlOutputClause o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _content = o._content;
            else
            {
                _content = new SNode<SqlTokenIdentifier, SelectColumnList, SqlTokenIdentifier, ISqlIdentifier, SqlEnclosedIdentifierCommaList>( items );
                CheckContent();
            }
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlOutputClause( this, leading, children, trailing );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public SqlTokenIdentifier OutputT => _content.V1;

        public SelectColumnList Columns => _content.V2;

        public SqlTokenIdentifier IntoT => _content.V3;

        public ISqlIdentifier TargetTable => _content.V4;

        public SqlEnclosedIdentifierCommaList ColumnNames => _content.V5;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor ) => visitor.Visit( this );

    }


}
