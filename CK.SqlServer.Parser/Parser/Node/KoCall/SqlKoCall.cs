using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using CK.Core;
using System.Collections.Immutable;

namespace CK.SqlServer.Parser
{
    using CNode = SNode<ISqlNode, SqlEnclosedCommaList, SqlWithinGroup, SqlOverClause>;

    public sealed class SqlKoCall : SqlNonTokenAutoWidth
    {
        readonly CNode _content;

        public SqlKoCall( ISqlNode funName, SqlEnclosedCommaList parameters, SqlWithinGroup withinGroup = null, SqlOverClause over = null )
            : base( null, null )
        {
            _content = new CNode( funName, parameters, withinGroup, over );
            CheckContent();
        }

        void CheckContent()
        {
            Helper.CheckNotNull( FunName, nameof( FunName ) );
            Helper.CheckNotNull( Parameters, nameof( Parameters ) );
        }

        SqlKoCall( SqlKoCall o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
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
            return new SqlKoCall( this, leading, content, trailing );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

        public ISqlNode FunName => _content.V1;

        public SqlEnclosedCommaList Parameters => _content.V2;

        public SqlWithinGroup WithinGroup => _content.V3;

        public SqlOverClause OverClause => _content.V4;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

    }
}
