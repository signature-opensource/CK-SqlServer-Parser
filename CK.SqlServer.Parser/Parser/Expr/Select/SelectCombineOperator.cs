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
    public class SelectCombineOperator : SqlExpr, ISelectSpecification
    {
        public SelectCombineOperator( ISelectSpecification left, SqlTokenIdentifier exceptUnionOrIntercept, ISelectSpecification right, SelectOrderBy orderBy = null, SelectFor forPart = null )
            : this( null, Build( left, exceptUnionOrIntercept, null, right, orderBy, forPart ), null )
        {
            if( !IsValidOperator( exceptUnionOrIntercept.TokenType ) ) throw new ArgumentException();
        }

        public SelectCombineOperator( ISelectSpecification left, SqlTokenIdentifier unionT, SqlTokenIdentifier allT, ISelectSpecification right, SelectOrderBy orderBy = null, SelectFor forPart = null )
            : this( null, Build( left, unionT, allT, right, orderBy, forPart ), null )
        {
            if( unionT.TokenType == SqlTokenType.Union && allT != null && !allT.NameEquals( "all" ) ) throw new ArgumentException();
        }

        static SqlNode[] Build( ISelectSpecification left, SqlTokenIdentifier opT, SqlTokenIdentifier allT, ISelectSpecification right, SelectOrderBy orderBy, SelectFor forPart )
        {
            Debug.Assert( left != null && opT != null && right != null );
            SqlNode o = allT != null ? (SqlNode)new SqlTokenList<SqlTokenIdentifier>( opT, allT ) : opT;
            return Build( SqlToken.EmptyOpenPar, left, o, right, orderBy, forPart, SqlToken.EmptyClosePar );
        }

        static SqlNode[] Build( SqlTokenList<SqlTokenOpenPar> opener, ISelectSpecification left, SqlNode op, ISelectSpecification right, SelectOrderBy orderBy, SelectFor forPart, SqlTokenList<SqlTokenClosePar> closer )
        {
            Debug.Assert( opener != null && left != null && op != null && right != null && closer != null );
            if( orderBy != null )
            {
                if( forPart != null )
                {
                    return CreateArray<SqlNode>( opener, (SqlNode)left, op, (SqlNode)right, orderBy, forPart, closer );
                }
                return CreateArray( opener, (SqlNode)left, op, (SqlNode)right, orderBy, closer );
            }
            else if( forPart != null )
            {
                return CreateArray( opener, (SqlNode)left, op, (SqlNode)right, forPart, closer );
            }
            return CreateArray( opener, (SqlNode)left, op, (SqlNode)right, closer );
        }

        internal SelectCombineOperator( ImmutableList<SqlTrivia> leading, SqlNode[] items, ImmutableList<SqlTrivia> trailing )
            : base( leading, items, trailing )
        {
            Debug.Assert( Slots.Length >= 5 && Slots.Length <= 7 );
            Debug.Assert( Slots[1] is ISelectSpecification && Slots[3] is ISelectSpecification );
            Debug.Assert( Slots.Length != 6 || (Slots[4] is SelectOrderBy || Slots[4] is SelectFor) );
            Debug.Assert( Slots.Length < 7 || (Slots[4] is SelectOrderBy && Slots[5] is SelectFor) );
            Debug.Assert( IsValidOperator( OperatorT.TokenType ) 
                                && (UnionAll == null
                                    || (UnionAll != null
                                        && UnionAll.Tokens[0].TokenType == SqlTokenType.Union
                                        && UnionAll.Tokens[1] is SqlTokenIdentifier
                                        && ((SqlTokenIdentifier)UnionAll.Tokens[1]).NameEquals( "all" ))) );
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IReadOnlyList<SqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SelectCombineOperator( leading, EnsureArray( children ), trailing );
        }

        static public bool IsValidOperator( SqlTokenType op )
        {
            return op == SqlTokenType.Union || op == SqlTokenType.Except || op == SqlTokenType.Intersect;
        }

        public SelectColumnList Columns { get { return LeftSelect.Columns; } }

        public SqlExpr Left { get { return (SqlExpr)Slots[1]; } }

        public ISelectSpecification LeftSelect { get { return (ISelectSpecification)Slots[1]; } }

        SqlTokenList<SqlToken> UnionAll { get { return Slots[2] as SqlTokenList<SqlToken>; } }

        SqlTokenIdentifier OperatorT { get { return Slots[2] is SqlTokenIdentifier ? (SqlTokenIdentifier)Slots[2] : ((SqlTokenList<SqlTokenIdentifier>)Slots[2]).Tokens[0]; } }

        /// <summary>
        /// Gets the operator token type: it can be: <see cref="SqlTokenType.Union"/>, <see cref="SqlTokenType.Except"/>, <see cref="SqlTokenType.Intersect"/>.
        /// </summary>
        public SqlTokenType CombinationKind { get { return OperatorT.TokenType; } }

        public SqlNode Operator { get { return Slots[2]; } }

        public bool IsUnionDistinct { get { return UnionAll == null && OperatorT.TokenType == SqlTokenType.Union; } }

        public bool IsUnionAll { get { return UnionAll != null; } }

        public bool IsExcept { get { return OperatorT.TokenType == SqlTokenType.Except; } }

        public bool IsIntersect { get { return OperatorT.TokenType == SqlTokenType.Intersect; } }

        public SqlExpr Right { get { return (SqlExpr)Slots[3]; } }

        public ISelectSpecification RightSelect { get { return (ISelectSpecification)Slots[3]; } }

        public SelectOrderBy OrderByClause { get { return Slots.Length == 6 ? Slots[4] as SelectOrderBy : (Slots.Length == 7 ? (SelectOrderBy)Slots[4] : null); } }

        public SelectFor ForClause { get { return Slots.Length == 6 ? Slots[4] as SelectFor : (Slots.Length == 7 ? (SelectFor)Slots[5] : null); } }

        public ISelectSpecification SetExtensions( SelectOrderBy orderBy, SelectFor forPart )
        {
            SelectOrderBy o = OrderByClause;
            SelectFor f = ForClause;
            if( orderBy == o && forPart == f ) return this;
            return new SelectCombineOperator( LeadingTrivias, Build( Opener, LeftSelect, Operator, RightSelect, orderBy, forPart, Closer ), TrailingTrivias );
        }

        [DebuggerStepThrough]
        internal protected override SqlNode Accept( SqlItemVisitor visitor )
        {
            return visitor.Visit( this );
        }
    }
}
