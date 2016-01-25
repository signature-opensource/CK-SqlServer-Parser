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

    public sealed class SqlOptionParOptions : ASqlNodePrefixedEnclosedSeparatedList<SqlTokenIdentifier,SqlTokenOpenPar, ISqlNode, SqlTokenComma, SqlTokenClosePar>
    {
        public SqlOptionParOptions( 
            SqlTokenIdentifier optionT,
            SqlTokenOpenPar opener,
            IEnumerable<ISqlNode> items,
            SqlTokenClosePar closer)
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

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlOptionParOptions( this, leading, children, trailing );
        }

        public SqlTokenIdentifier OptionT => Prefix;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor ) => visitor.Visit( this );

    }


}
