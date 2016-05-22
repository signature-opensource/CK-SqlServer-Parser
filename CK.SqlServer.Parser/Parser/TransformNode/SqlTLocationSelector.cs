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
    public sealed class SqlTLocationSelector : SqlNonToken
    {
        readonly SNode<
            SqlTokenIdentifier,
            SqlTokenTerminal,
            SqlTokenLiteralInteger,
            ISqlNode> _content;

        public SqlTLocationSelector( SqlTokenIdentifier firtOrLastOrSingle, SqlTokenTerminal plusOrMinusT, SqlTokenLiteralInteger offset, ISqlNode rangeOrString )
            : base( null, null )
        {
            _content = new SNode<SqlTokenIdentifier, SqlTokenTerminal, SqlTokenLiteralInteger, ISqlNode>( firtOrLastOrSingle, plusOrMinusT, offset, rangeOrString );
            CheckContent();
        }

        void CheckContent()
        {
            Helper.CheckNullableToken( FirstOrLastOrSingleT, nameof( FirstOrLastOrSingleT ), SqlTokenType.First, SqlTokenType.Last, SqlTokenType.Single );
            if( FirstOrLastOrSingleT != null )
            {
                if( FirstOrLastOrSingleT.TokenType == SqlTokenType.Single )
                {
                    if( PlusOrMinusT != null || Offset != null ) throw new ArgumentException( "Invalid offset after 'single'." );
                }
                else if( Offset != null )
                {
                    if( FirstOrLastOrSingleT.TokenType == SqlTokenType.Last )
                    {
                        if( PlusOrMinusT == null || PlusOrMinusT.TokenType == SqlTokenType.Plus )
                        {
                            throw new ArgumentException( "'last' offset requires a minus sign: 'last - 2'." );
                        }
                    }
                    else
                    {
                        if( PlusOrMinusT == null || PlusOrMinusT.TokenType == SqlTokenType.Minus )
                        {
                            throw new ArgumentException( "'first' offset requires a plus sign: 'first + 1'." );
                        }
                    }
                }
            }
            Helper.CheckNotNull( RangeOrString, nameof( RangeOrString ) );
        }

        SqlTLocationSelector( SqlTLocationSelector o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _content = o._content;
            else
            {
                _content = new SNode<SqlTokenIdentifier, SqlTokenTerminal, SqlTokenLiteralInteger, ISqlNode>( items );
                CheckContent();
            }
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlTLocationSelector( this, leading, content, trailing );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

        public SqlTokenIdentifier FirstOrLastOrSingleT => _content.V1;

        public SqlTokenTerminal PlusOrMinusT => _content.V2;

        public SqlTokenLiteralInteger Offset => _content.V3;

        /// <summary>
        /// Gets a <see cref="SqlTokenLiteralString"/> or a range.
        /// </summary>
        public ISqlNode RangeOrString => _content.V4;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

    }

}
