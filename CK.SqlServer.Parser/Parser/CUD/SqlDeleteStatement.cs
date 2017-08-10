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
    public sealed class SqlDeleteStatement : SqlNonTokenAutoWidth, ISqlNamedStatement
    {
        readonly SNode<
            MIUDHeader,
            SqlTokenIdentifier,
            IUDTarget,
            SqlOutputClause,
            SelectFrom,
            SqlTokenIdentifier,
            ISqlNode,
            SqlOptionParOptions,
            SqlTokenTerminal> _content;

        public SqlDeleteStatement( 
            MIUDHeader header,
            SqlTokenIdentifier fromTargetT,
            IUDTarget target,
            SqlOutputClause outputClause,
            SelectFrom from,
            SqlTokenIdentifier whereT,
            ISqlNode whereExpression,
            SqlOptionParOptions options,
            SqlTokenTerminal terminator )
            : base( null, null )
        {
            _content = new SNode<MIUDHeader, SqlTokenIdentifier, IUDTarget, SqlOutputClause, SelectFrom, SqlTokenIdentifier, ISqlNode, SqlOptionParOptions, SqlTokenTerminal>( 
                header,
                fromTargetT,
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
            Helper.CheckNotNull( Header, nameof( Header ) );
            Helper.CheckNullableToken( FromTargetT, nameof( FromTargetT ), SqlTokenType.From );
            Helper.CheckNotNull( Target, nameof( Target ) );
            Helper.CheckNullableToken( WhereT, nameof( WhereT ), SqlTokenType.Where );
            Helper.CheckBothNullOrNot( WhereT, nameof( WhereT ), WhereExpression, nameof( WhereExpression ) );
        }

        SqlDeleteStatement( SqlDeleteStatement o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _content = o._content;
            else
            {
                _content = new SNode<MIUDHeader, SqlTokenIdentifier, IUDTarget, SqlOutputClause, SelectFrom, SqlTokenIdentifier, ISqlNode, SqlOptionParOptions, SqlTokenTerminal>( items );
                CheckContent();
            }
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlDeleteStatement( this, leading, content, trailing );
        }

        public StatementKnownName StatementKnownName => StatementKnownName.Delete;

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

        public MIUDHeader Header => _content.V1;

        public SqlTokenIdentifier FromTargetT => _content.V2;

        public IUDTarget Target => _content.V3;

        public bool HasOutputClause => _content.V4 != null;

        public SqlOutputClause OutputClause => _content.V4;

        public bool HasFrom => _content.V5 != null;

        public SelectFrom From => _content.V5;

        public bool HasWhere => _content.V6 != null;

        public SqlTokenIdentifier WhereT => _content.V6;

        public ISqlNode WhereExpression => _content.V7;

        public bool HasOptions => _content.V8 != null;

        public SqlOptionParOptions Options => _content.V8;

        public SqlTokenTerminal StatementTerminator => _content.V9;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

    }


}
