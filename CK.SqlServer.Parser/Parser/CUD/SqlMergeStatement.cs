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
    public sealed class SqlMergeStatement : SqlNode, ISqlNamedStatement
    {
        readonly SNode<MIUDHeader, SqlTokenIdentifier, ISqlNode, SqlWithParOptions, SqlTokenIdentifier, SqlTokenIdentifier, SqlTokenIdentifier, ISqlNode, SqlTokenTerminal> _content;

        public SqlMergeStatement( 
            MIUDHeader header, 
            SqlTokenIdentifier intoT, 
            ISqlNode targetTable, 
            SqlWithParOptions withMergeHints,
            SqlTokenIdentifier asT,
            SqlTokenIdentifier targetAliasName,
            SqlTokenIdentifier usingT,
            ISqlNode unmodeledRemaider,
            SqlTokenTerminal terminator )
            : base( null, null )
        {
            _content = new SNode<MIUDHeader, SqlTokenIdentifier, ISqlNode, SqlWithParOptions, SqlTokenIdentifier, SqlTokenIdentifier, SqlTokenIdentifier, ISqlNode, SqlTokenTerminal>( 
                header, 
                intoT, 
                targetTable,
                withMergeHints,
                asT,
                targetAliasName,
                usingT,
                unmodeledRemaider,
                terminator );
            CheckContent();
        }

        void CheckContent()
        {
            SNode.CheckNotNull( Header, nameof( Header ) );
            SNode.CheckNullableToken( IntoT, nameof( IntoT ), SqlTokenType.Into );
            SNode.CheckNotNull( TargetTable, nameof( TargetTable ) );
            SNode.CheckNullableToken( AsT, nameof( AsT ), SqlTokenType.As );
            SNode.CheckToken( UsingT, nameof( UsingT ), SqlTokenType.Using );
            SNode.CheckNotNull( UnmodeledRemaider, nameof( UnmodeledRemaider ) );
        }

        SqlMergeStatement( SqlMergeStatement o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _content = o._content;
            else
            {
                _content = new SNode<MIUDHeader, SqlTokenIdentifier, ISqlNode, SqlWithParOptions, SqlTokenIdentifier, SqlTokenIdentifier, SqlTokenIdentifier, ISqlNode, SqlTokenTerminal>( items );
                CheckContent();
            }
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlMergeStatement( this, leading, children, trailing );
        }

        public StatementKnownName StatementKnownName => StatementKnownName.Merge;

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public MIUDHeader Header => _content.V1;

        public bool HasIntoTarget => _content.V3 != null;

        public SqlTokenIdentifier IntoT => _content.V2;

        public ISqlNode TargetTable => _content.V3;

        public bool HasWithMergeHints => _content.V4 != null;

        public SqlWithParOptions WithMergeHints => _content.V4;

        public SqlTokenIdentifier AsT => _content.V5;

        public bool HasTargetAliasName => _content.V6 != null;

        public SqlTokenIdentifier TargetAliasName => _content.V6;

        public SqlTokenIdentifier UsingT => _content.V7;

        public ISqlNode UnmodeledRemaider => _content.V8;

        public SqlTokenTerminal StatementTerminator => _content.V9;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor ) => visitor.Visit( this );

    }


}
