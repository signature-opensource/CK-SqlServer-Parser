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
    /// Defines "next value for {sequence}>" expression.
    /// </summary>
    public sealed class SqlCTEStatement : SqlNode, ISqlNamedStatement
    {
        readonly SNode<SqlTokenIdentifier,SqlCTENameList,ISqlStatement> _content;

        public SqlCTEStatement( 
                SqlTokenIdentifier withT,
                SqlCTENameList names,
                ISqlStatement outerStatement )
            : base( null, null )
        {
            _content = new SNode<SqlTokenIdentifier, SqlCTENameList, ISqlStatement>( withT, names, outerStatement );
            CheckContent();
        }

        void CheckContent()
        {
            SNode.CheckToken( WithT, nameof( WithT ), SqlTokenType.With );
            SNode.CheckNotNull( Names, nameof( Names ) );
            SNode.CheckNotNull( OuterStatement, nameof( OuterStatement ) );
        }

        SqlCTEStatement( SqlCTEStatement o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _content = o._content;
            else
            {
                _content = new SNode<SqlTokenIdentifier, SqlCTENameList, ISqlStatement>( items );
                CheckContent();
            }
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlCTEStatement( this, leading, children, trailing );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public StatementKnownName StatementKnownName => StatementKnownName.CTE;

        public SqlTokenTerminal StatementTerminator => OuterStatement.StatementTerminator;

        public SqlTokenIdentifier WithT => _content.V1;

        public SqlCTENameList Names => _content.V2;

        public ISqlStatement OuterStatement => _content.V3;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor ) => visitor.Visit( this );

    }


}
