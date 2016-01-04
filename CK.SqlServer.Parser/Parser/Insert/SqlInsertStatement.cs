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
    /// 
    /// </summary>
    public sealed class SqlInsertStatement : SqlNode, ISqlNamedStatement
    {
        readonly SNode<InsOrUpdHeader, SqlTokenIdentifier, ISqlNode, SqlWithParOptions, SqlEnclosedIdentifierCommaList, SqlOutputClause, ISqlNode, SqlTokenTerminal> _content;

        public SqlInsertStatement( 
            InsOrUpdHeader header, 
            SqlTokenIdentifier intoT, 
            ISqlNode target, 
            SqlWithParOptions options, 
            SqlEnclosedIdentifierCommaList columns,
            SqlOutputClause outputClause,
            ISqlNode values,
            SqlTokenTerminal terminator )
            : base( null, null )
        {
            _content = new SNode<InsOrUpdHeader, SqlTokenIdentifier, ISqlNode, SqlWithParOptions, SqlEnclosedIdentifierCommaList, SqlOutputClause, ISqlNode, SqlTokenTerminal>( 
                header, 
                intoT, 
                target, 
                options,
                columns,
                outputClause,
                values, 
                terminator );
            CheckContent();
        }

        void CheckContent()
        {
            SNode.CheckNotNull( Header, nameof( Header ) );
            SNode.CheckNullableToken( IntoT, nameof( IntoT ), SqlTokenType.Into );
            SNode.CheckNotNull( IntoTarget, nameof( IntoTarget ) );
            SNode.CheckNotNull( Values, nameof( Values ) );
        }

        SqlInsertStatement( SqlInsertStatement o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _content = o._content;
            else
            {
                _content = new SNode<InsOrUpdHeader, SqlTokenIdentifier, ISqlNode, SqlWithParOptions, SqlEnclosedIdentifierCommaList, SqlOutputClause, ISqlNode, SqlTokenTerminal>( items );
                CheckContent();
            }
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlInsertStatement( this, leading, children, trailing );
        }

        public StatementKnownName StatementKnownName => StatementKnownName.Insert;

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public InsOrUpdHeader Header => _content.V1;

        public bool HasIntoTarget => _content.V3 != null;

        public SqlTokenIdentifier IntoT => _content.V2;

        public ISqlNode IntoTarget => _content.V3;

        public bool HasOptions => _content.V4 != null;

        public SqlWithParOptions Options => _content.V4;

        public bool HasColumns => _content.V5 != null;

        public SqlEnclosedIdentifierCommaList Columns => _content.V5;

        public bool HasOutputClause => _content.V6 != null;

        public SqlOutputClause OutputClause => _content.V6;

        public ISqlNode Values => _content.V7;

        public SqlTokenTerminal StatementTerminator => _content.V8;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor ) => visitor.Visit( this );

    }


}
