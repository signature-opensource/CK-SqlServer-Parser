using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

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
    /// | single) (<see cref="ISqlHasStringValue"/> | <see cref="SqlTNodeSimplePattern"/>) 
    /// </summary>
    public sealed class SqlTOneLocationFinder : SqlNonTokenAutoWidth, ISqlTLocationFinder
    {
        readonly CNode _content;

        public SqlTOneLocationFinder( SqlTokenIdentifier firtOrLastOrSingle,
                                      SqlTokenTerminal plusOrMinusT,
                                      SqlTokenLiteralInteger offset,
                                      SqlTokenIdentifier outT,
                                      SqlTokenIdentifier ofT,
                                      SqlTokenLiteralInteger expectedMatchCount,
                                      ISqlNode pattern )
               : base( null, null )
        {
            _content = new CNode( firtOrLastOrSingle, plusOrMinusT, offset, outT, ofT, expectedMatchCount, pattern );
            CheckContent();
        }

        void CheckContent()
        {
            Helper.CheckToken( FirstOrLastOrSingleT, nameof( FirstOrLastOrSingleT ), SqlTokenType.First, SqlTokenType.Last, SqlTokenType.Single );
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
            Helper.CheckNullableToken( OutT, nameof( OutT ), SqlTokenType.Out );
            Helper.CheckNullableToken( OfT, nameof( OfT ), SqlTokenType.Of );
            Helper.CheckAllNullOrNot( OutT, nameof( OutT ), OfT, nameof( OfT ), ExpectedMatchCount, nameof( ExpectedMatchCount ) );
            Helper.CheckNotNull<ISqlHasStringValue,SqlTNodeSimplePattern>( Pattern, nameof( Pattern ) );
        }

        SqlTOneLocationFinder( SqlTOneLocationFinder o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
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
            return new SqlTOneLocationFinder( this, leading, content, trailing );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

        public SqlTokenIdentifier FirstOrLastOrSingleT => _content.V1;

        public SqlTokenTerminal PlusOrMinusT => _content.V2;

        public SqlTokenLiteralInteger Offset => _content.V3;

        public SqlTokenIdentifier OutT => _content.V4;

        public SqlTokenIdentifier OfT => _content.V5;

        public SqlTokenLiteralInteger ExpectedMatchCount => _content.V6;

        /// <summary>
        /// Gets a <see cref="ISqlHasStringValue"/> or a <see cref="SqlTNodeSimplePattern"/>.
        /// </summary>
        public ISqlNode Pattern => _content.V7;

        /// <summary>
        /// Gets the normalized cardinality.
        /// </summary>
        public LocationCardinalityInfo GetCardinality() => new LocationCardinalityInfo( this );

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

    }

}
