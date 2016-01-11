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
    public sealed class SqlDeleteStatement : SqlNode, ISqlNamedStatement
    {
        readonly SNode<
            MIUDHeader,
            IUDTarget,
            SqlOutputClause,
            SelectFrom,
            SqlTokenIdentifier,
            ISqlNode,
            SqlOptionParOptions,
            SqlTokenTerminal> _content;

        public SqlDeleteStatement( 
            MIUDHeader header,
            IUDTarget target,
            SqlOutputClause outputClause,
            SelectFrom from,
            SqlTokenIdentifier whereT,
            ISqlNode whereExpression,
            SqlOptionParOptions options,
            SqlTokenTerminal terminator )
            : base( null, null )
        {
            _content = new SNode<MIUDHeader, IUDTarget, SqlOutputClause, SelectFrom, SqlTokenIdentifier, ISqlNode, SqlOptionParOptions, SqlTokenTerminal>( 
                header, 
                target,
                outputClause,
                from,
                whereT,
                whereExpression,
                options,
                terminator );
            CheckContent();
        }

        void CheckContent()
        {
            SNode.CheckNotNull( Header, nameof( Header ) );
            SNode.CheckNotNull( Target, nameof( Target ) );
            SNode.CheckNullableToken( WhereT, nameof( WhereT ), SqlTokenType.Where );
            SNode.CheckBothNullOrNot( WhereT, nameof( WhereT ), WhereExpression, nameof( WhereExpression ) );
        }

        SqlDeleteStatement( SqlDeleteStatement o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _content = o._content;
            else
            {
                _content = new SNode<MIUDHeader, IUDTarget, SqlOutputClause, SelectFrom, SqlTokenIdentifier, ISqlNode, SqlOptionParOptions, SqlTokenTerminal>( items );
                CheckContent();
            }
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlDeleteStatement( this, leading, children, trailing );
        }

        public StatementKnownName StatementKnownName => StatementKnownName.Delete;

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public MIUDHeader Header => _content.V1;

        public IUDTarget Target => _content.V2;

        public bool HasOutputClause => _content.V3 != null;

        public SqlOutputClause OutputClause => _content.V3;

        public bool HasFrom => _content.V4 != null;

        public SelectFrom From => _content.V4;

        public bool HasWhere => _content.V5 != null;

        public SqlTokenIdentifier WhereT => _content.V5;

        public ISqlNode WhereExpression => _content.V6;

        public bool HasOptions => _content.V7 != null;

        public SqlOptionParOptions Options => _content.V7;

        public SqlTokenTerminal StatementTerminator => _content.V8;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor ) => visitor.Visit( this );

    }


}
