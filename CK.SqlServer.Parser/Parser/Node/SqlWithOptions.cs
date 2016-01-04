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

    public sealed class SqlWithOptions : ASqlNodePrefixedSeparatedList<SqlTokenIdentifier,ISqlNode,SqlTokenComma>
    {
        public SqlWithOptions( 
            SqlTokenIdentifier withT,
            IEnumerable<ISqlNode> commaSeparatedOptions )
            : base( 0, withT, commaSeparatedOptions )
        {
            SNode.CheckToken( withT, nameof( withT ), SqlTokenType.With );
        }

        SqlWithOptions( SqlWithOptions o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( o, 0, leading, items, trailing )
        {
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlWithOptions( this, leading, children, trailing );
        }

        public SqlTokenIdentifier withT => Prefix;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor ) => visitor.Visit( this );

    }


}
