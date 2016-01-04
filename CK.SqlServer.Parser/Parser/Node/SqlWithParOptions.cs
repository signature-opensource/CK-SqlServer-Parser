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

    public sealed class SqlWithParOptions : SqlNode
    {
        readonly SNode<SqlTokenIdentifier, SqlEnclosedCommaList> _content;

        public SqlWithParOptions( 
            SqlTokenIdentifier withT,
            SqlEnclosedCommaList options )
            : base( null, null )
        {
            _content = new SNode<SqlTokenIdentifier, SqlEnclosedCommaList>( withT, options );
            CheckContent();
        }

        void CheckContent()
        {
            SNode.CheckToken( withT, nameof( withT ), SqlTokenType.With );
            SNode.CheckNotNull( Options, nameof( Options ) );
        }

        SqlWithParOptions( SqlWithParOptions o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _content = o._content;
            else
            {
                _content = new SNode<SqlTokenIdentifier, SqlEnclosedCommaList>( items );
                CheckContent();
            }
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlWithParOptions( this, leading, children, trailing );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public SqlTokenIdentifier withT => _content.V1;

        /// <summary>
        /// Gets the {when E0 = V0 then C0}+ selector.
        /// </summary>
        public SqlEnclosedCommaList Options => _content.V2;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor ) => visitor.Visit( this );

    }


}
