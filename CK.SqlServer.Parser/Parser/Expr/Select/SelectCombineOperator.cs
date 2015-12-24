using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;

namespace CK.SqlServer.Parser
{
    /// <summary>
    /// Combination of two select through Union, Except or Intersect.
    /// </summary>
    public class SelectCombineOperator : SqlNode, ISelectSpecification
    {
        readonly SNode<
                    ISelectSpecification,
                    SqlTokenIdentifier,
                    SqlTokenIdentifier,
                    ISelectSpecification,
                    SelectOrderBy,
                    SelectFor> _content;

        public SelectCombineOperator( 
            ISelectSpecification left, 
            SqlTokenIdentifier unionT, 
            SqlTokenIdentifier allT, 
            ISelectSpecification right, 
            SelectOrderBy orderBy = null, 
            SelectFor forPart = null )
            : base( null, null )
        {
            _content = new SNode<ISelectSpecification, SqlTokenIdentifier, SqlTokenIdentifier, ISelectSpecification, SelectOrderBy, SelectFor>( left, unionT, allT, right, orderBy, forPart );
            if( unionT.TokenType == SqlTokenType.Union && allT != null && !allT.NameEquals( "all" ) ) throw new ArgumentException();
        }

        void CheckContent()
        {
            SNode.CheckNotNull( Left, nameof( Left ) );
            SNode.CheckToken( OperatorT, nameof( OperatorT ), SqlTokenType.Union, SqlTokenType.Intersect, SqlTokenType.Except );
            SNode.CheckNullableToken( AllT, nameof( AllT ), SqlTokenType.All );
            if( AllTokens != null ) SNode.CheckToken( OperatorT, nameof( OperatorT ), SqlTokenType.Union );
            SNode.CheckNotNull( Right, nameof( Right ) );
        }

        SelectCombineOperator( SelectCombineOperator o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _content = o._content;
            else
            {
                _content = new SNode<ISelectSpecification, SqlTokenIdentifier, SqlTokenIdentifier, ISelectSpecification, SelectOrderBy, SelectFor>( items );
                CheckContent();
            }
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IReadOnlyList<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SelectCombineOperator( this, leading, children, trailing );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public ISelectSpecification Left => _content.V1;

        public SelectColumnList Columns => Left.Columns;

        public SqlTokenIdentifier OperatorT => _content.V2;

        public SqlTokenIdentifier AllT => _content.V3;

        /// <summary>
        /// Gets the operator token type: it can be: <see cref="SqlTokenType.Union"/>, 
        /// <see cref="SqlTokenType.Except"/>, <see cref="SqlTokenType.Intersect"/>.
        /// </summary>
        public SqlTokenType CombinationKind { get { return OperatorT.TokenType; } }

        public bool IsUnionDistinct => OperatorT.TokenType == SqlTokenType.Union && AllT == null;

        public bool IsUnionAll => OperatorT.TokenType == SqlTokenType.Union && AllT != null;

        public bool IsExcept => OperatorT.TokenType == SqlTokenType.Except;

        public bool IsIntersect => OperatorT.TokenType == SqlTokenType.Intersect;

        public ISelectSpecification Right => _content.V4;

        public SelectOrderBy OrderByClause => _content.V5;

        public SelectFor ForClause => _content.V6;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor ) => visitor.Visit( this );
    }
}
