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

    /// <summary>
    /// (first [+ n] [out of n] 
    /// | last [- n] [out of n] 
    /// | single 
    /// | all[[out of] n]) (<see cref="ISqlHasStringValue"/> | <see cref="SqlTNodeSimplePattern"/>) 
    /// </summary>
    public sealed class SqlTLocationFinder : SqlNonTokenAutoWidth
    {
        readonly CNode _content;

        public SqlTLocationFinder( 
            SqlTokenIdentifier firtOrLastOrSingleOrAllOrEach, 
            SqlTokenTerminal plusOrMinusT, 
            SqlTokenLiteralInteger offset, 
            SqlTokenIdentifier outT,
            SqlTokenIdentifier ofT,
            SqlTokenLiteralInteger expectedMatchCount, 
            ISqlNode pattern )
               : base( null, null )
        {
            _content = new CNode( firtOrLastOrSingleOrAllOrEach, plusOrMinusT, offset, outT, ofT, expectedMatchCount, pattern );
            CheckContent();
        }

        void CheckContent()
        {
            Helper.CheckToken( FirstOrLastOrSingleOrAllOrEachT, nameof( FirstOrLastOrSingleOrAllOrEachT ), SqlTokenType.First, SqlTokenType.Last, SqlTokenType.Single, SqlTokenType.All, SqlTokenType.Each );
            if( FirstOrLastOrSingleOrAllOrEachT.TokenType == SqlTokenType.Single || FirstOrLastOrSingleOrAllOrEachT.TokenType == SqlTokenType.All )
            {
                if( PlusOrMinusT != null || Offset != null ) throw new ArgumentException( "Invalid offset after 'single' or 'all'." );
            }
            else if( Offset != null )
            {
                if( FirstOrLastOrSingleOrAllOrEachT.TokenType == SqlTokenType.Last )
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
            if( FirstOrLastOrSingleOrAllOrEachT.TokenType == SqlTokenType.All
                || FirstOrLastOrSingleOrAllOrEachT.TokenType == SqlTokenType.Each )
            {
                if( ExpectedMatchCount == null ) Helper.CheckNull( OutT, nameof( OutT ) );
                Helper.CheckBothNullOrNot( OutT, nameof( OutT ), OfT, nameof( OfT ) );
            }
            else
            {
                Helper.CheckAllNullOrNot( OutT, nameof( OutT ), OfT, nameof( OfT ), ExpectedMatchCount, nameof( ExpectedMatchCount ) );
            }
            Helper.CheckNotNull<ISqlHasStringValue,SqlTNodeSimplePattern>( Pattern, nameof( Pattern ) );
        }

        SqlTLocationFinder( SqlTLocationFinder o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
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
            return new SqlTLocationFinder( this, leading, content, trailing );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

        public SqlTokenIdentifier FirstOrLastOrSingleOrAllOrEachT => _content.V1;

        public SqlTokenTerminal PlusOrMinusT => _content.V2;

        public SqlTokenLiteralInteger Offset => _content.V3;

        public SqlTokenIdentifier OutT => _content.V4;

        public SqlTokenIdentifier OfT => _content.V5;

        public SqlTokenLiteralInteger ExpectedMatchCount => _content.V6;

        /// <summary>
        /// Gets a <see cref="ISqlHasStringValue"/> or a <see cref="SqlTNodeSimplePattern"/>.
        /// </summary>
        public ISqlNode Pattern => _content.V7;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

    }

}
