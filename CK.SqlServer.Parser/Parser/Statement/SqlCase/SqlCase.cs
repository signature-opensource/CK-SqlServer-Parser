using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CK.SqlServer.Parser
{
    using CNode = SNode<SqlTokenIdentifier, ISqlNode, SqlCaseWhenList, SqlTokenIdentifier, ISqlNode, SqlTokenIdentifier>;

    public sealed class SqlCase : SqlNonTokenAutoWidth
    {
        readonly CNode _content;

        public SqlCase( SqlTokenIdentifier caseToken,
                        ISqlNode expr,
                        SqlCaseWhenList whenSelector,
                        SqlTokenIdentifier elseToken,
                        ISqlNode elseExpr,
                        SqlTokenIdentifier endToken )
            : base( null, null )
        {
            _content = new CNode( caseToken,
                                  expr,
                                  whenSelector,
                                  elseToken,
                                  elseExpr,
                                  endToken );
            CheckContent();
        }

        void CheckContent()
        {
            Helper.CheckToken( CaseT, nameof( CaseT ), SqlTokenType.Case );
            Helper.CheckNotNull( WhenList, nameof( WhenList ) );
            Helper.CheckNullableToken( ElseT, nameof( ElseT ), SqlTokenType.Else );
            Helper.CheckToken( EndT, nameof( EndT ), SqlTokenType.End );
            Helper.CheckBothNullOrNot( ElseT, nameof( ElseT ), ElseExpression, nameof( ElseExpression ) );
        }

        SqlCase( SqlCase o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _content = o._content;
            else
            {
                _content = new CNode( items );
                CheckContent();
            }
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlCase( this, leading, content, trailing );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

        /// <summary>
        /// Gets whether this is a simple case: "case Expression when V0 then C0 when V1 then C1 end".
        /// </summary>
        public bool IsSimpleCase => _content.V2 != null;

        /// <summary>
        /// Gets whether this is a search case: "case when E0 = V0 then C0 when E1 = V1 then C1 end". 
        /// </summary>
        public bool IsSearchCase => _content.V2 == null;

        /// <summary>
        /// Gets the first case token.
        /// </summary>
        public SqlTokenIdentifier CaseT => _content.V1;

        /// <summary>
        /// Gets the simple case expression. Null if <see cref="IsSearchCase"/> is true.
        /// </summary>
        public ISqlNode Expression => _content.V2;

        /// <summary>
        /// Gets the {when E0 = V0 then C0}+ selector.
        /// </summary>
        public SqlCaseWhenList WhenList => _content.V3;

        /// <summary>
        /// Gets whether the else clause exists.
        /// </summary>
        public bool HasElse => _content.V4 != null;

        /// <summary>
        /// Gets the else token if it exists.
        /// </summary>
        public SqlTokenIdentifier ElseT => _content.V4;

        /// <summary>
        /// Gets the else expression if it exists.
        /// </summary>
        public ISqlNode ElseExpression => _content.V5;

        /// <summary>
        /// Gets the end token.
        /// </summary>
        public SqlTokenIdentifier EndT => _content.V6;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

    }


}
