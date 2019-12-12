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
            SqlTokenLiteralInteger,
            ISqlNode>;

    /// <summary>
    /// (each|all) [n] (<see cref="ISqlHasStringValue"/> | <see cref="SqlTNodeSimplePattern"/>)
    /// </summary>
    public sealed class SqlTMultiLocationFinder : SqlNonTokenAutoWidth, ISqlTLocationFinder
    {
        readonly CNode _content;

        public SqlTMultiLocationFinder( 
            SqlTokenIdentifier allOrEach, 
            SqlTokenLiteralInteger expectedMatchCount,
            ISqlNode pattern )
               : base( null, null )
        {
            _content = new CNode( allOrEach, expectedMatchCount, pattern );
            CheckContent();
        }

        void CheckContent()
        {
            Helper.CheckToken( AllOrEachT, nameof( AllOrEachT ), SqlTokenType.All, SqlTokenType.Each );
            Helper.CheckNotNull<ISqlHasStringValue,SqlTNodeSimplePattern>( Pattern, nameof( Pattern ) );
        }

        SqlTMultiLocationFinder( SqlTMultiLocationFinder o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
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
            return new SqlTMultiLocationFinder( this, leading, content, trailing );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

        public SqlTokenIdentifier AllOrEachT => _content.V1;

        public SqlTokenLiteralInteger ExpectedMatchCount => _content.V2;

        /// <summary>
        /// Gets a <see cref="ISqlHasStringValue"/> or a <see cref="SqlTNodeSimplePattern"/>.
        /// </summary>
        public ISqlNode Pattern => _content.V3;

        /// <summary>
        /// Gets the normalized cardinality.
        /// </summary>
        public LocationCardinalityInfo GetCardinality() => new LocationCardinalityInfo( this );

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

    }

}
