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
    using CNode = SNode<
            SqlTokenIdentifier,
            SqlTokenIdentifier,
            SqlTokenIdentifier,
            SqlTokenIdentifier,
            SqlTokenIdentifier,
            ISqlNamedStatement,
            SqlTokenTerminal>;

    /// <summary>
    /// Combine select.
    /// </summary>
    public sealed class SqlTCombineSelect : SqlNonTokenAutoWidth, ISqlTStatement
    {
        readonly CNode _content;

        public SqlTCombineSelect( 
            SqlTokenIdentifier combineT, 
            SqlTokenIdentifier selectT,
            SqlTokenIdentifier operatorT,
            SqlTokenIdentifier allT,
            SqlTokenIdentifier withT,
            ISqlNamedStatement select,
            SqlTokenTerminal terminator)
            : base( null, null )
        {
            _content = new CNode( combineT, selectT, operatorT, allT, withT, select, terminator );
            CheckContent();
        }

        void CheckContent()
        {
            Helper.CheckToken( CombineT, nameof( CombineT ), SqlTokenType.Combine );
            Helper.CheckToken( SelectT, nameof( SelectT ), SqlTokenType.Select );
            Helper.CheckToken( OperatorT, nameof( OperatorT ), SqlTokenType.Union, SqlTokenType.Intersect, SqlTokenType.Except );
            Helper.CheckNullableToken( AllT, nameof( AllT ), SqlTokenType.All );
            Helper.CheckToken( WithT, nameof( WithT ), SqlTokenType.With );
            Helper.CheckNotNull( Select, nameof( Select ) );
        }

        SqlTCombineSelect( SqlTCombineSelect o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
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
            return new SqlTCombineSelect( this, leading, content, trailing );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

        public SqlTokenIdentifier CombineT => _content.V1;

        public SqlTokenIdentifier SelectT => _content.V2;

        public SqlTokenIdentifier OperatorT => _content.V3;

        public SqlTokenIdentifier AllT => _content.V4;

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

        public SqlTokenIdentifier WithT => _content.V5;

        public ISqlNamedStatement Select => _content.V6;

        public SqlTokenTerminal StatementTerminator => _content.V7;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

    }

}
