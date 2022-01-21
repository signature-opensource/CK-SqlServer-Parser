using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CK.SqlServer.Parser
{

    using CNode = SNode<SqlTokenIdentifier, SqlTokenIdentifier, SqlTokenIdentifier>;

    public sealed class SqlCreateOrAlter : SqlNonTokenAutoWidth
    {
        readonly CNode _content;

        public SqlCreateOrAlter( SqlTokenIdentifier alterOrCreateT, SqlTokenIdentifier orT, SqlTokenIdentifier alterT )
            : base( null, null )
        {
            _content = new CNode( alterOrCreateT, orT, alterT);
            CheckContent();
        }

        void CheckContent()
        {
            Helper.CheckBothNullOrNot( OrT, nameof( OrT ), AlterT, nameof(AlterT) );
            Helper.CheckNullableToken( OrT, nameof( OrT ), SqlTokenType.Or );
            Helper.CheckNullableToken( AlterT, nameof( AlterT), SqlTokenType.Alter );
            if( OrT != null )
            {
                Helper.CheckToken( AlterOrCreateT, nameof( AlterOrCreateT ), SqlTokenType.Create );
            }
            else
            {
                Helper.CheckToken( AlterOrCreateT, nameof( AlterOrCreateT ), SqlTokenType.Alter, SqlTokenType.Create );
            }
        }

        SqlCreateOrAlter( SqlCreateOrAlter o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _content = o._content;
            else
            {
                _content = new CNode( items );
                CheckContent();
            }
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlCreateOrAlter( this, leading, content, trailing );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

        public SqlTokenIdentifier AlterOrCreateT => _content.V1;

        public SqlTokenIdentifier OrT => _content.V2;

        public SqlTokenIdentifier AlterT => _content.V3;

        public CreateOrAlterStatementPrefix StatementPrefix => OrT != null
                                                                ? CreateOrAlterStatementPrefix.CreateOrAlter
                                                                : (AlterOrCreateT.TokenType == SqlTokenType.Create
                                                                    ? CreateOrAlterStatementPrefix.Create
                                                                    : CreateOrAlterStatementPrefix.Alter);

        public SqlCreateOrAlter WithStatementPrefix( CreateOrAlterStatementPrefix prefix )
        {
            if( prefix == CreateOrAlterStatementPrefix.None || prefix == StatementPrefix ) return this;
            switch( prefix )
            {
                case CreateOrAlterStatementPrefix.Alter: return new SqlCreateOrAlter( SqlKeyword.Alter.SetTrivias( FullLeadingTrivias, FullTrailingTrivias ), null, null );
                case CreateOrAlterStatementPrefix.Create: return new SqlCreateOrAlter( SqlKeyword.Create.SetTrivias( FullLeadingTrivias, FullTrailingTrivias ), null, null );
                default:
                {
                    var create = SqlKeyword.Create.SetTrivias( FullLeadingTrivias, SqlTrivia.OneSpace );
                    var alter = SqlKeyword.Alter.SetTrivias( SqlTrivia.OneSpace, FullTrailingTrivias );
                    return new SqlCreateOrAlter( create, SqlKeyword.Or, alter );
                }
            }
        }

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );


    }

}
