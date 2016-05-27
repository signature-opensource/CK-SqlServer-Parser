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
            SqlTokenTerminal,
            SqlTokenLiteralInteger,
            SqlTokenIdentifier,
            SqlTokenIdentifier,
            SqlTokenLiteralInteger,
            ISqlNode>;

    public sealed class SqlTLocationSelector : SqlNonToken
    {
        readonly CNode _content;

        public SqlTLocationSelector( 
            SqlTokenIdentifier firtOrLastOrSingleOrAll, 
            SqlTokenTerminal plusOrMinusT, 
            SqlTokenLiteralInteger offset, 
            SqlTokenIdentifier outT,
            SqlTokenIdentifier ofT,
            SqlTokenLiteralInteger expectedMatchCount, 
            ISqlNode rangeOrString )
               : base( null, null )
        {
            _content = new CNode( firtOrLastOrSingleOrAll, plusOrMinusT, offset, outT, ofT, expectedMatchCount, rangeOrString );
            CheckContent();
        }

        void CheckContent()
        {
            Helper.CheckToken( FirstOrLastOrSingleOrAllT, nameof( FirstOrLastOrSingleOrAllT ), SqlTokenType.First, SqlTokenType.Last, SqlTokenType.Single, SqlTokenType.All );
            if( FirstOrLastOrSingleOrAllT.TokenType == SqlTokenType.Single || FirstOrLastOrSingleOrAllT.TokenType == SqlTokenType.All )
            {
                if( PlusOrMinusT != null || Offset != null ) throw new ArgumentException( "Invalid offset after 'single' or 'all'." );
            }
            else if( Offset != null )
            {
                if( FirstOrLastOrSingleOrAllT.TokenType == SqlTokenType.Last )
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
            Helper.CheckNullableToken( OutT, nameof( OutT ), SqlTokenType.Out );
            Helper.CheckNullableToken( OfT, nameof( OfT ), SqlTokenType.Of );
            if( FirstOrLastOrSingleOrAllT.TokenType == SqlTokenType.Last )
            {
                if( ExpectedMatchCount != null )
                {
                    Helper.CheckBothNullOrNot( OutT, nameof( OutT ), OfT, nameof( OfT ) );
                }
            }
            else
            {
                Helper.CheckAllNullOrNot( OutT, nameof( OutT ), OfT, nameof( OfT ), ExpectedMatchCount, nameof( ExpectedMatchCount ) );
            }
            Helper.CheckNotNull( RangeOrString, nameof( RangeOrString ) );
        }

        SqlTLocationSelector( SqlTLocationSelector o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
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
            return new SqlTLocationSelector( this, leading, content, trailing );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

        public SqlTokenIdentifier FirstOrLastOrSingleOrAllT => _content.V1;

        public SqlTokenTerminal PlusOrMinusT => _content.V2;

        public SqlTokenLiteralInteger Offset => _content.V3;

        public SqlTokenIdentifier OutT => _content.V4;

        public SqlTokenIdentifier OfT => _content.V5;

        public SqlTokenLiteralInteger ExpectedMatchCount => _content.V6;

        /// <summary>
        /// Gets a <see cref="ISqlHasStringValue"/> or a <see cref="SqlTNodeRange"/>.
        /// </summary>
        public ISqlNode RangeOrString => _content.V7;

        public bool IsStringLocation => _content.V7 is ISqlHasStringValue;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

    }

}
