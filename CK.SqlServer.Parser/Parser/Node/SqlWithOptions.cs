using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CK.SqlServer.Parser
{

    public sealed class SqlWithOptions : ASqlNodePrefixedSeparatedList<SqlTokenIdentifier,ISqlNode,SqlTokenComma>
    {
        public SqlWithOptions( 
            SqlTokenIdentifier withT,
            IEnumerable<ISqlNode> commaSeparatedOptions )
            : base( 0, withT, commaSeparatedOptions )
        {
            CheckContent();
        }

        void CheckContent()
        {
            Helper.CheckToken( WithT, nameof( WithT ), SqlTokenType.With );
        }

        SqlWithOptions( SqlWithOptions o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( o, 0, leading, items, trailing )
        {
            if( items != null ) CheckContent();
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlWithOptions( this, leading, content, trailing );
        }

        public SqlTokenIdentifier WithT => Prefix;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

    }


}
