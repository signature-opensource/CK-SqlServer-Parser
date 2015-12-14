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
    /// Select "For" operator.
    /// </summary>
    public class SelectFor : SqlExpr, ISelectSpecification
    {
        public SelectFor( ISelectSpecification select, SqlTokenIdentifier forToken, SqlExpr content )
            : this( null, CreateArray<SqlNode>( SqlToken.EmptyOpenPar, (SqlNode)select, forToken, content, SqlToken.EmptyClosePar ), null )
        {
        }

        internal SelectFor( ImmutableList<SqlTrivia> leading, SqlNode[] items, ImmutableList<SqlTrivia> trailing )
            : base( leading, items, trailing )
        {
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IReadOnlyList<SqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SelectFor( leading, EnsureArray( children ), trailing );
        }

        public ISelectSpecification Select { get { return (ISelectSpecification)Slots[1]; } }

        public SqlExpr SelectExpr { get { return (SqlExpr)Slots[1]; } }

        public SqlExpr ForExpression { get { return (SqlExpr)Slots[3]; } }

        [DebuggerStepThrough]
        internal protected override T Accept<T>( ISqlItemVisitor<T> visitor )
        {
            return visitor.Visit( this );
        }

        public SqlTokenType CombinationKind
        {
            get { return SqlTokenType.For; }
        }

        public SelectColumnList Columns
        {
            get { return Select.Columns; }
        }

    }


}
