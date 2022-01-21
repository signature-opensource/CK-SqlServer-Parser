using CK.Core;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CK.SqlServer.Parser
{
    using CNode = SNode<
            SqlCreateOrAlter,
            SqlTokenIdentifier,
            ISqlIdentifier,
            SqlTokenIdentifier,
            ISqlIdentifier,
            SqlTokenIdentifier,
            SqlTStatementList,
            SqlTokenTerminal>;

    public sealed class SqlTransformer : SqlNonTokenAutoWidth, ISqlNamedStatement, ISqlServerTransformer, ISqlFullNameHolder
    {
        readonly CNode _content;

        public SqlTransformer( SqlCreateOrAlter createOrAlter,
                               SqlTokenIdentifier transfomerT,
                               ISqlIdentifier name,
                               SqlTokenIdentifier onT,
                               ISqlIdentifier targetName,
                               SqlTokenIdentifier asT,
                               SqlTStatementList body,
                               SqlTokenTerminal term )
            : base( null, null )
        {
            _content = new CNode( createOrAlter,
                                  transfomerT,
                                  name,
                                  onT,
                                  targetName,
                                  asT,
                                  body,
                                  term );
            CheckContent();
        }

        SqlTransformer( SqlTransformer o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _content = o._content;
            else
            {
                _content = new CNode( items );
                CheckContent();
            }
        }

        void CheckContent()
        {
            Helper.CheckNotNull( CreateOrAlter, nameof( CreateOrAlter ) );
            Helper.CheckToken( TransformerT, nameof( TransformerT ), SqlTokenType.Transformer );
            Helper.CheckNullableToken( OnT, nameof( OnT ), SqlTokenType.On );
            Helper.CheckBothNullOrNot( OnT, nameof( OnT ), TargetFullName, nameof( TargetFullName ) );
            Helper.CheckToken( AsT, nameof( AsT ), SqlTokenType.As );
            Helper.CheckNotNull( Body, nameof( Body ) );
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlTransformer( this, leading, content, trailing );
        }

        public StatementKnownName StatementKnownName => StatementKnownName.CreateTransformer;

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

        public SqlCreateOrAlter CreateOrAlter => _content.V1;

        public SqlTokenIdentifier TransformerT => _content.V2;

        /// <summary>
        /// Gets the optional name of this transformer.
        /// </summary>
        public ISqlIdentifier FullName => _content.V3;

        string ISqlServerTransformer.SchemaName => _content.V3?.ToStringHyperCompact();

        public SqlTokenIdentifier OnT => _content.V4;

        public ISqlIdentifier TargetFullName => _content.V5;

        public string TargetSchemaName => _content.V5?.ToStringHyperCompact();

        public SqlTokenIdentifier AsT => _content.V6;

        public SqlTStatementList Body => _content.V7;

        public SqlTokenTerminal StatementTerminator => _content.V8;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

        IEnumerable<ISqlServerComment> ISqlServerParsedText.HeaderComments => FullLeadingTrivias.Cast<ISqlServerComment>();

        void ISqlServerParsedText.Write( StringBuilder b ) => Write( SqlTextWriter.CreateDefault( b ) );

        object ISqlServerTransformer.Transform( IActivityMonitor monitor, object target )
        {
            var t = target as ISqlNode;
            if( t == null ) throw new ArgumentException( "Invalid target object type.", nameof(target) );
            return SqlServerParser.LateBoundTransform( monitor, this, t );
        }


    }
}
