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
    /// Defines "next value for {sequence}>" expression.
    /// </summary>
    public sealed class SqlNextValueFor : SqlNonToken
    {
        readonly SNode<SqlTokenIdentifier, SqlTokenIdentifier, SqlTokenIdentifier, ISqlIdentifier> _content;

        public SqlNextValueFor( SqlTokenIdentifier nextT, SqlTokenIdentifier valueT, SqlTokenIdentifier forT, ISqlIdentifier seqName )
            : base( null, null )
        {
            _content = new SNode<SqlTokenIdentifier, SqlTokenIdentifier, SqlTokenIdentifier, ISqlIdentifier>( nextT, valueT, forT, seqName );
            CheckContent();
        }

        void CheckContent()
        {
            Helper.CheckToken( NextT, nameof( NextT ), SqlTokenType.Next );
            Helper.CheckToken( ValueT, nameof( ValueT ), SqlTokenType.Value );
            Helper.CheckToken( ForT, nameof( ForT ), SqlTokenType.For );
            Helper.CheckNotNull( SequenceName, nameof( SequenceName ) );
        }

        SqlNextValueFor( SqlNextValueFor o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _content = o._content;
            else
            {
                _content = new SNode<SqlTokenIdentifier, SqlTokenIdentifier, SqlTokenIdentifier, ISqlIdentifier>( items );
                CheckContent();
            }
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlNextValueFor( this, leading, content, trailing );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

        public SqlTokenIdentifier NextT => _content.V1;

        public SqlTokenIdentifier ValueT => _content.V2;

        public SqlTokenIdentifier ForT => _content.V3;

        public ISqlIdentifier SequenceName=> _content.V4;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

    }


}
