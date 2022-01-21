using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CK.SqlServer.Parser
{
    /// <summary>
    /// Captures options expressed as: OPTION ( ... ).
    /// </summary>
    public sealed class SqlOptionParOptions : ASqlNodePrefixedEnclosedSeparatedList<SqlTokenIdentifier,SqlTokenOpenPar, ISqlNode, SqlTokenComma, SqlTokenClosePar>
    {
        public SqlOptionParOptions( SqlTokenIdentifier optionT,
                                    SqlTokenOpenPar opener,
                                    IEnumerable<ISqlNode> items,
                                    SqlTokenClosePar closer )
            : base( 0, optionT, opener, items, closer )
        {
            CheckContent();
        }

        void CheckContent()
        {
            Helper.CheckToken( OptionT, nameof( OptionT ), SqlTokenType.Option );
        }

        SqlOptionParOptions( SqlOptionParOptions o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( o, 0, leading, items, trailing )
        {
            if( items != null ) CheckContent();
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlOptionParOptions( this, leading, content, trailing );
        }

        public SqlTokenIdentifier OptionT => Prefix;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

    }


}
