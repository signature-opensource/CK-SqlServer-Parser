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
            SqlTOneLocationFinder,
            SqlTokenIdentifier,
            SqlTOneLocationFinder>;

    /// <summary>
    /// <para>
    /// (after|before) <see cref="SqlTOneLocationFinder"/> | between <see cref="SqlTOneLocationFinder"/> and <see cref="SqlTOneLocationFinder"/>
    /// </para>
    /// This is used by <see cref="SqlTInScope"/>.
    /// </summary>
    public sealed class SqlTRangeLocationFinder : SqlNonTokenAutoWidth
    {
        readonly CNode _content;

        public SqlTRangeLocationFinder( 
            SqlTokenIdentifier afterOrBeforeOrBetween,
            SqlTOneLocationFinder firstLoc, 
            SqlTokenIdentifier andT,
            SqlTOneLocationFinder secondLoc )
               : base( null, null )
        {
            _content = new CNode( afterOrBeforeOrBetween, firstLoc, andT, secondLoc );
            CheckContent();
        }

        void CheckContent()
        {
            Helper.CheckToken( AfterOrBeforeOrBetweenT, nameof( AfterOrBeforeOrBetweenT ), SqlTokenType.After, SqlTokenType.Before, SqlTokenType.Between );
            if( AfterOrBeforeOrBetweenT.TokenType == SqlTokenType.Between )
            {
                Helper.CheckToken( AndT, nameof( AndT ), SqlTokenType.And );
                Helper.CheckNotNull( SecondLocation, nameof( SecondLocation ) );
            }
            else 
            {
                Helper.CheckNull( AndT, nameof( AndT ) );
                Helper.CheckNull( SecondLocation, nameof( SecondLocation ) );
            }
        }

        SqlTRangeLocationFinder( SqlTRangeLocationFinder o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
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
            return new SqlTRangeLocationFinder( this, leading, content, trailing );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

        public SqlTokenIdentifier AfterOrBeforeOrBetweenT => _content.V1;

        public SqlTOneLocationFinder FirstLocation => _content.V2;

        public SqlTokenIdentifier AndT => _content.V3;

        public SqlTOneLocationFinder SecondLocation => _content.V4;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

    }

}
