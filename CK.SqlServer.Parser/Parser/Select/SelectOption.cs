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
    /// <summary>
    /// Captures the optional "Option ( ... )" select part.
    /// </summary>
    public class SelectOption : ASqlNodeArrayBased
    {
        public SelectOption( SqlTokenIdentifier optionToken, ISqlNode content )
            : this( null, CreateArray( optionToken, content ), null )
        {
        }

        internal SelectOption( ImmutableList<SqlTrivia> leading, ISqlNode[] items, ImmutableList<SqlTrivia> trailing )
            : base( leading, items, trailing )
        {
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IReadOnlyList<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SelectOption( leading, EnsureArray( children ), trailing );
        }

        public ISqlNode Content { get { return Children[1]; } }

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor )
        {
            return visitor.Visit( this );
        }

    }


}
