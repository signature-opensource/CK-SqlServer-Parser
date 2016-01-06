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
    public sealed class SqlUpdateStatement : SqlNode, ISqlNamedStatement
    {
        readonly SNode<
            CUDHeader,
            CUDTarget,
            SqlTokenIdentifier,
            SqlCommaList,
            SqlOutputClause,
            SelectFrom,
            SqlTokenIdentifier,
            ISqlNode,
            SqlOptionParOptions,
            SqlTokenTerminal> _content;

        public SqlUpdateStatement( 
            CUDHeader header,
            CUDTarget target,
            SqlTokenIdentifier setT,
            SqlCommaList assigns,
            SqlOutputClause outputClause,
            SelectFrom from,
            SqlTokenIdentifier whereT,
            ISqlNode whereExpression,
            SqlOptionParOptions options,
            SqlTokenTerminal terminator )
            : base( null, null )
        {
            _content = new SNode<CUDHeader, CUDTarget, SqlTokenIdentifier, SqlCommaList, SqlOutputClause, SelectFrom, SqlTokenIdentifier, ISqlNode, SqlOptionParOptions, SqlTokenTerminal>( 
                header, 
                target,
                setT,
                assigns,
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
            SNode.CheckToken( SetT, nameof( SetT ), SqlTokenType.Set );
            SNode.CheckNotNull( Assigns, nameof( Assigns ) );
            SNode.CheckNullableToken( WhereT, nameof( WhereT ), SqlTokenType.Where );
            SNode.CheckBothNullOrNot( WhereT, nameof( WhereT ), WhereExpression, nameof( WhereExpression ) );
        }

        SqlUpdateStatement( SqlUpdateStatement o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _content = o._content;
            else
            {
                _content = new SNode<CUDHeader, CUDTarget, SqlTokenIdentifier, SqlCommaList, SqlOutputClause, SelectFrom, SqlTokenIdentifier, ISqlNode, SqlOptionParOptions, SqlTokenTerminal>( items );
                CheckContent();
            }
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlUpdateStatement( this, leading, children, trailing );
        }

        public StatementKnownName StatementKnownName => StatementKnownName.Update;

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public CUDHeader Header => _content.V1;

        public CUDTarget Target => _content.V2;

        public SqlTokenIdentifier SetT => _content.V3;

        public SqlCommaList Assigns => _content.V4;

        public bool HasOutputClause => _content.V5 != null;

        public SqlOutputClause OutputClause => _content.V5;

        public bool HasFrom => _content.V6 != null;

        public SelectFrom From => _content.V6;

        public bool HasWhere => _content.V7 != null;

        public SqlTokenIdentifier WhereT => _content.V7;

        public ISqlNode WhereExpression => _content.V8;

        public bool HasOptions => _content.V9 != null;

        public SqlOptionParOptions Options => _content.V9;

        public SqlTokenTerminal StatementTerminator => _content.V10;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor ) => visitor.Visit( this );

    }


}
