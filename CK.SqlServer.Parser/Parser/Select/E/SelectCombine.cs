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
    /// Operator that combines two <see cref="ISelectSpecification"/> with Union, Uinion All, Except or Intersect.
    /// </summary>
    public sealed class SelectCombine : SqlNode, ISelectSpecification
    {
        readonly SNode<ISqlNode, SqlTokenIdentifier, SqlTokenIdentifier, ISqlNode> _content;

        public SelectCombine(
            ISqlNode leftNode, 
            SqlTokenIdentifier unionT, 
            SqlTokenIdentifier allT,
            ISqlNode rightNode )
            : base( null, null )
        {
            _content = new SNode<ISqlNode, SqlTokenIdentifier, SqlTokenIdentifier, ISqlNode>( leftNode, unionT, allT, rightNode );
            CheckContent();
        }

        void CheckContent()
        {
            SNode.CheckUnPar<ISelectSpecification>( LeftNode, nameof( LeftNode ) );
            SNode.CheckToken( OperatorT, nameof( OperatorT ), SqlTokenType.Union, SqlTokenType.Intersect, SqlTokenType.Except );
            SNode.CheckNullableToken( AllT, nameof( AllT ), SqlTokenType.All );
            if( AllTokens != null ) SNode.CheckToken( OperatorT, nameof( OperatorT ), SqlTokenType.Union );
            SNode.CheckNotNull( RightNode, nameof( RightNode ) );
        }

        SelectCombine( SelectCombine o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _content = o._content;
            else
            {
                _content = new SNode<ISqlNode, SqlTokenIdentifier, SqlTokenIdentifier, ISqlNode>( items );
                CheckContent();
            }
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SelectCombine( this, leading, children, trailing );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public ISqlNode LeftNode => _content.V1;

        public ISelectSpecification Left => (ISelectSpecification)_content.V1.UnPar;

        public SelectColumnList Columns => Left.Columns;

        public SqlTokenIdentifier OperatorT => _content.V2;

        public SqlTokenIdentifier AllT => _content.V3;

        /// <summary>
        /// Gets the operator token type: it can be: <see cref="SelectOperatorKind.UnionDistinct"/>, 
        /// <see cref="SelectOperatorKind.UnionAll"/>, <see cref="SelectOperatorKind.Except"/> 
        /// or <see cref="SelectOperatorKind.Intersect"/>.
        /// </summary>
        public SelectOperatorKind SelectOperator
        {
            get
            {
                return OperatorT.TokenType == SqlTokenType.Union
                        ? AllT == null ? SelectOperatorKind.UnionDistinct : SelectOperatorKind.UnionAll
                        : (OperatorT.TokenType == SqlTokenType.Except 
                            ? SelectOperatorKind.Except 
                            : SelectOperatorKind.Intersect);
            }
        } 

        public bool IsUnionDistinct => OperatorT.TokenType == SqlTokenType.Union && AllT == null;

        public bool IsUnionAll => OperatorT.TokenType == SqlTokenType.Union && AllT != null;

        public bool IsExcept => OperatorT.TokenType == SqlTokenType.Except;

        public bool IsIntersect => OperatorT.TokenType == SqlTokenType.Intersect;

        public ISqlNode RightNode => _content.V4;

        public ISelectSpecification Right => (ISelectSpecification)_content.V4.UnPar;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor ) => visitor.Visit( this );
    }
}
