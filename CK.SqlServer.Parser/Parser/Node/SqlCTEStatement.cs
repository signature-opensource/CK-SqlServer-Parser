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
            Helper.CheckToken( WithT, nameof( WithT ), SqlTokenType.With );
            Helper.CheckNotNull( Names, nameof( Names ) );
            Helper.CheckNotNull( OuterStatement, nameof( OuterStatement ) );
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

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlCTEStatement( this, leading, content, trailing );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

        public StatementKnownName StatementKnownName => StatementKnownName.CTE;

        public SqlTokenTerminal StatementTerminator => OuterStatement.StatementTerminator;

        public SqlTokenIdentifier WithT => _content.V1;

        public SqlCTENameList Names => _content.V2;

        public ISqlStatement OuterStatement => _content.V3;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

    }


}
