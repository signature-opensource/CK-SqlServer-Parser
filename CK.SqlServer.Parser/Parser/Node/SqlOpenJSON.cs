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

    public sealed class SqlOpenJSON : SqlNonTokenAutoWidth
    {
        readonly SNode<SqlTokenIdentifier, SqlEnclosedCommaList, SqlWithParOptions> _content;

        public SqlOpenJSON( SqlTokenIdentifier openJSON, SqlEnclosedCommaList parameters, SqlWithParOptions options, ImmutableList<SqlTrivia> leading = null, ImmutableList<SqlTrivia> trailing = null )
            : base( leading, trailing )
        {
            _content = new SNode<SqlTokenIdentifier, SqlEnclosedCommaList, SqlWithParOptions>( openJSON, parameters, options );
            CheckContent();
        }

        void CheckContent()
        {
            Helper.CheckToken( FunName, nameof( FunName ), SqlTokenType.OpenJSON );
        }

        SqlOpenJSON( SqlOpenJSON o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _content = o._content;
            else
            {
                _content = new SNode<SqlTokenIdentifier, SqlEnclosedCommaList, SqlWithParOptions>( items );
                CheckContent();
            }
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlOpenJSON( this, leading, content, trailing );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

        public SqlTokenIdentifier FunName => _content.V1;

        public SqlEnclosedCommaList Parameters => _content.V2;

        public bool HasSchema => _content.V3 != null;

        public SqlWithParOptions Schema => _content.V3;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

    }


}
