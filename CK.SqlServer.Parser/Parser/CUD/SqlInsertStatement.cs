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
        readonly SNode<MIUDHeader, SqlTokenIdentifier, IUDTarget, SqlEnclosedIdentifierCommaList, SqlOutputClause, ISqlNode, SqlTokenTerminal> _content;

        public SqlInsertStatement( 
            MIUDHeader header, 
            SqlTokenIdentifier intoT,
            IUDTarget target, 
            SqlEnclosedIdentifierCommaList columns,
            SqlOutputClause outputClause,
            ISqlNode values,
            SqlTokenTerminal terminator )
            : base( null, null )
        {
            _content = new SNode<MIUDHeader, SqlTokenIdentifier, IUDTarget, SqlEnclosedIdentifierCommaList, SqlOutputClause, ISqlNode, SqlTokenTerminal>( 
                header, 
                intoT, 
                target,
                columns,
                outputClause,
                values, 
                terminator );
            CheckContent();
        }

        void CheckContent()
        {
            Helper.CheckNotNull( Header, nameof( Header ) );
            Helper.CheckNullableToken( IntoT, nameof( IntoT ), SqlTokenType.Into );
            Helper.CheckNotNull( Target, nameof( Target ) );
            Helper.CheckNotNull( Values, nameof( Values ) );
        }

        SqlInsertStatement( SqlInsertStatement o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _content = o._content;
            else
            {
                _content = new SNode<MIUDHeader, SqlTokenIdentifier, IUDTarget, SqlEnclosedIdentifierCommaList, SqlOutputClause, ISqlNode, SqlTokenTerminal>( items );
                CheckContent();
            }
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlInsertStatement( this, leading, children, trailing );
        }

        public StatementKnownName StatementKnownName => StatementKnownName.Insert;

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

        public MIUDHeader Header => _content.V1;

        public SqlTokenIdentifier IntoT => _content.V2;

        public IUDTarget Target => _content.V3;

        public bool HasColumns => _content.V4 != null;

        public SqlEnclosedIdentifierCommaList Columns => _content.V4;

        public bool HasOutputClause => _content.V5 != null;

        public SqlOutputClause OutputClause => _content.V5;

        public ISqlNode Values => _content.V6;

        public SqlTokenTerminal StatementTerminator => _content.V7;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor ) => visitor.Visit( this );

    }


}
