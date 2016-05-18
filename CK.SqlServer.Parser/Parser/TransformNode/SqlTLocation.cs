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
    public sealed class SqlTLocation : SqlNonToken
    {
        readonly SNode<
            SqlTokenIdentifier,
            SqlTokenIdentifier,
            SqlTokenTerminal,
            SqlTokenLiteralInteger,
            ISqlNode> _content;

        public SqlTLocation( SqlTokenIdentifier beforeOrAfterT, SqlTokenIdentifier firtOrLastOrSingle, SqlTokenTerminal plusOrMinusT, SqlTokenLiteralInteger offset, ISqlNode rangeOrString )
            : base( null, null )
        {
            _content = new SNode<SqlTokenIdentifier, SqlTokenIdentifier, SqlTokenTerminal, SqlTokenLiteralInteger, ISqlNode>( beforeOrAfterT, firtOrLastOrSingle, plusOrMinusT, offset, rangeOrString );
            CheckContent();
        }

        void CheckContent()
        {
            Helper.CheckToken( AfterOrBeforeT, nameof( AfterOrBeforeT ), SqlTokenType.After, SqlTokenType.Before );
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

        SqlTLocation( SqlTLocation o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _content = o._content;
            else
            {
                _content = new SNode<SqlTokenIdentifier, SqlTokenIdentifier, SqlTokenTerminal, SqlTokenLiteralInteger, ISqlNode>( items );
                CheckContent();
            }
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlTLocation( this, leading, content, trailing );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

        public SqlTokenIdentifier AfterOrBeforeT => _content.V1;

        /// <summary>
        /// Gets whteher this is "before...".  Otherwise it is "after...".
        /// </summary>
        public bool IsBefore => AfterOrBeforeT.TokenType == SqlTokenType.Before;

        public SqlTokenIdentifier FirstOrLastOrSingleT => _content.V2;

        public SqlTokenTerminal PlusOrMinusT => _content.V3;

        public SqlTokenLiteralInteger Offset => _content.V4;

        /// <summary>
        /// Gets a <see cref="SqlTokenLiteralString"/> or a range.
        /// </summary>
        public ISqlNode RangeOrString => _content.V5;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

    }

}
