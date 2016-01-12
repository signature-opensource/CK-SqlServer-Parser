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
    /// 
    /// </summary>
    public sealed class SqlRaiserror : SqlNode, ISqlNamedStatement
    {
        readonly SNode<SqlTokenIdentifier, SqlEnclosedCommaList, SqlWithOptions, SqlTokenTerminal> _content;

        public SqlRaiserror( SqlTokenIdentifier raiserrorT, SqlEnclosedCommaList parameters, SqlWithOptions options, SqlTokenTerminal terminator )
            : base( null, null )
        {
            _content = new SNode<SqlTokenIdentifier, SqlEnclosedCommaList, SqlWithOptions, SqlTokenTerminal>( raiserrorT, parameters, options, terminator );
            CheckContent();
        }

        void CheckContent()
        {
            SNode.CheckToken( RaiserorT, nameof( RaiserorT ), SqlTokenType.Raiserror );
            SNode.CheckNotNull( Parameters, nameof( Parameters ) );
        }

        SqlRaiserror( SqlRaiserror o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _content = o._content;
            else
            {
                _content = new SNode<SqlTokenIdentifier, SqlEnclosedCommaList, SqlWithOptions, SqlTokenTerminal>( items );
                CheckContent();
            }
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlRaiserror( this, leading, children, trailing );
        }

        public StatementKnownName StatementKnownName => StatementKnownName.Raiserror;

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public SqlTokenIdentifier RaiserorT => _content.V1;

        public SqlEnclosedCommaList Parameters => _content.V2;

        public bool HasOptions => _content.V3 != null;

        public SqlWithOptions Options => _content.V3;

        public SqlTokenTerminal StatementTerminator => _content.V4;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor ) => visitor.Visit( this );

    }


}
