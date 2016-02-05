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
    /// Captures a select statement: it is a <see cref="Select"/> specification, optionally enclosed in <see cref="SqlPar"/>
    /// and followed by a satement terminator.
    /// </summary>
    public sealed class SqlSelectStatement : SqlNode, ISqlNamedStatement, ISelectSpecification
    {
        readonly SNode<ISqlNode, SqlTokenTerminal> _content;

        public SqlSelectStatement( ISqlNode selectNode, SqlTokenTerminal statementTerminator = null )
            : base( null, null )
        {
            _content = new SNode<ISqlNode, SqlTokenTerminal>( selectNode, statementTerminator );
            CheckContent();
        }

        void CheckContent()
        {
            Helper.CheckUnPar<ISelectSpecification>( SelectNode, nameof( SelectNode ) );
        }

        SqlSelectStatement( SqlSelectStatement o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _content = o._content;
            else
            {
                _content = new SNode<ISqlNode, SqlTokenTerminal>( items );
                CheckContent();
            }
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlSelectStatement( this, leading, content, trailing );
        }

        public StatementKnownName StatementKnownName => StatementKnownName.Select;

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

        public ISqlNode SelectNode => _content.V1;

        public ISelectSpecification Select => (ISelectSpecification)_content.V1.UnPar;

        public SqlTokenTerminal StatementTerminator => _content.V2;

        public SelectOperatorKind SelectOperator => Select.SelectOperator;

        public SelectColumnList Columns => Select.Columns;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

    }

}
