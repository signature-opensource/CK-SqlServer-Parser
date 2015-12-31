using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;

namespace CK.SqlServer.Parser
{
    /// <summary>
    /// A try/catch block is defined by begin try...end try begin catch...end catch.
    /// </summary>
    public sealed class SqlTryCatch : SqlNode, ISqlNamedStatement
    {
        readonly SNode<
            SqlTokenIdentifier, SqlTokenIdentifier, 
            SqlStatementList, 
            SqlTokenIdentifier, SqlTokenIdentifier, SqlTokenIdentifier, SqlTokenIdentifier, 
            SqlStatementList, 
            SqlTokenIdentifier, SqlTokenIdentifier, SqlTokenTerminal> _content;

        public SqlTryCatch( SqlTokenIdentifier beginT, SqlTokenIdentifier tryT,
                                  SqlStatementList body, 
                                  SqlTokenIdentifier endT, SqlTokenIdentifier endTryT, SqlTokenIdentifier beginCT, SqlTokenIdentifier catchT, 
                                  SqlStatementList bodycatch, 
                                  SqlTokenIdentifier endCT, SqlTokenIdentifier endCatchT,
                                  SqlTokenTerminal statementTerminator = null )
            : base( null, null )
        {
            _content = new SNode<SqlTokenIdentifier, SqlTokenIdentifier, SqlStatementList, SqlTokenIdentifier, SqlTokenIdentifier, SqlTokenIdentifier, SqlTokenIdentifier, SqlStatementList, SqlTokenIdentifier, SqlTokenIdentifier, SqlTokenTerminal>(
                                beginT, tryT,
                                body, 
                                endT, endTryT, beginCT, catchT, 
                                bodycatch, 
                                endCT, endCatchT,
                                statementTerminator );
            CheckContent();
        }

        SqlTryCatch( SqlTryCatch o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _content = o._content;
            else
            {
                _content = new SNode<SqlTokenIdentifier, SqlTokenIdentifier, SqlStatementList, SqlTokenIdentifier, SqlTokenIdentifier, SqlTokenIdentifier, SqlTokenIdentifier, SqlStatementList, SqlTokenIdentifier, SqlTokenIdentifier, SqlTokenTerminal>( items );
                CheckContent();
            }
        }

        void CheckContent()
        {
            SNode.CheckToken( BeginT, nameof( BeginT ), SqlTokenType.Begin );
            SNode.CheckToken( TryT, nameof( TryT ), SqlTokenType.Try );

            SNode.CheckNotNull( Body, nameof( Body ) );

            SNode.CheckToken( EndT, nameof( EndT ), SqlTokenType.End );
            SNode.CheckToken( EndTryT, nameof( EndTryT ), SqlTokenType.Try );
            SNode.CheckToken( BeginCT, nameof( BeginCT ), SqlTokenType.Begin );
            SNode.CheckToken( CatchT, nameof( CatchT ), SqlTokenType.Catch );

            SNode.CheckNotNull( BodyCatch, nameof( BodyCatch ) );

            SNode.CheckToken( EndCT, nameof( EndCT ), SqlTokenType.End );
            SNode.CheckToken( EndCatchT, nameof( EndCatchT ), SqlTokenType.Catch );
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlTryCatch( this, leading, children, trailing );
        }

        public StatementKnownName StatementKnownName => StatementKnownName.TryCatch;

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public SqlTokenIdentifier BeginT => _content.V1;

        public SqlTokenIdentifier TryT => _content.V2;

        public SqlStatementList Body => _content.V3;

        public SqlTokenIdentifier EndT => _content.V4;

        public SqlTokenIdentifier EndTryT => _content.V5;

        public SqlTokenIdentifier BeginCT => _content.V6;

        public SqlTokenIdentifier CatchT => _content.V7;

        public SqlStatementList BodyCatch => _content.V8;

        public SqlTokenIdentifier EndCT => _content.V9;

        public SqlTokenIdentifier EndCatchT => _content.V10;

        public SqlTokenTerminal StatementTerminator => _content.V11;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor ) => visitor.Visit( this );

    }


}
