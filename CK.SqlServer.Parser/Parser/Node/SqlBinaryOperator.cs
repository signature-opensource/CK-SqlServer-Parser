using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CK.SqlServer.Parser
{
    public sealed class SqlBinaryOperator : SqlNonTokenAutoWidth
    {
        readonly SNode<ISqlNode, SqlToken, ISqlNode> _content;

        public SqlBinaryOperator( ISqlNode left, SqlToken op, ISqlNode right )
            : base( null, null )
        {
            _content = new SNode<ISqlNode, SqlToken, ISqlNode>( left, op, right );
            CheckContent();
        }

        void CheckContent()
        {
            Helper.CheckNotNull( Left, nameof( Left ) );
            Helper.CheckToken( Operator, nameof( Operator ), IsValidBinaryOperator );
            Helper.CheckNotNull( Right, nameof( Right ) );
        }

        SqlBinaryOperator( SqlBinaryOperator o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _content = o._content;
            else
            {
                _content = new SNode<ISqlNode, SqlToken, ISqlNode>( items );
                CheckContent();
            }
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlBinaryOperator( this, leading, content, trailing );
        }

        static public bool IsValidBinaryOperator( SqlTokenType op )
        {
            if( op > 0 )
            {
                if( (op & SqlTokenType.IsCompareOperator) != 0 ) return true;
                if( (op & SqlTokenType.IsBasicOperator) != 0 )
                {
                    if( op != SqlTokenType.BitwiseNot && op != SqlTokenType.Is) return true;
                }
                else if( op == SqlTokenType.And 
                         || op == SqlTokenType.Or 
                         || op == SqlTokenType.Dot
                         // This should be useless since :: seems to appear only after identifiers
                         // and this is handled directly as a multi identifier but in doubt,
                         // consider it as a valid binary operator...
                         || op == SqlTokenType.DoubleColons ) return true;
            }
            return false;
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

        public ISqlNode Left => _content.V1;

        public SqlToken Operator => _content.V2;

        public ISqlNode Right => _content.V3;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );
    }
}
