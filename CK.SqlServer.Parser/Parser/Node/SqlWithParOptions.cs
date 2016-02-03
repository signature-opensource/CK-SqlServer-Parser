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

    public sealed class SqlWithParOptions : ASqlNodePrefixedEnclosedSeparatedList<SqlTokenIdentifier,SqlTokenOpenPar, ISqlNode, SqlTokenComma, SqlTokenClosePar>
    {
        public SqlWithParOptions( 
            SqlTokenIdentifier withT,
            SqlTokenOpenPar opener,
            IEnumerable<ISqlNode> items,
            SqlTokenClosePar closer)
            : base( 0, withT, opener, items, closer )
        {
            CheckContent();
        }

        void CheckContent()
        {
            Helper.CheckToken( WithT, nameof( WithT ), SqlTokenType.With );
        }

        SqlWithParOptions( SqlWithParOptions o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( o, 0, leading, items, trailing )
        {
            if( items != null ) CheckContent();
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlWithParOptions( this, leading, content, trailing );
        }

        public SqlTokenIdentifier WithT => Prefix;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

    }


}
