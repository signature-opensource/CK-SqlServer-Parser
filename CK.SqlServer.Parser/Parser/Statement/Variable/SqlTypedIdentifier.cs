using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CK.SqlServer.Parser
{
    using CNode = SNode<SqlTokenIdentifier, SqlTokenIdentifier, ISqlUnifiedTypeDecl>;

    /// <summary>
    /// An identifier (a <see cref="SqlTokenIdentifier"/>, typically a variable name) followed by an
    /// optional 'as' and a type declaration (<see cref="ISqlUnifiedTypeDecl"/>).
    /// </summary>
    public sealed class SqlTypedIdentifier : SqlNonTokenAutoWidth
    {
        readonly CNode _content;

        public SqlTypedIdentifier( SqlTokenIdentifier identifier, SqlTokenIdentifier optAsToken, ISqlUnifiedTypeDecl type )
            : base( null, null )
        {
            _content = new CNode( identifier, optAsToken, type );
            CheckContent();
        }

        void CheckContent()
        {
            Helper.CheckNotNull( Identifier, nameof( Identifier ) );
            Helper.CheckNullableToken( AsT, nameof( AsT ), SqlTokenType.As );
            Helper.CheckNotNull( TypeDecl, nameof( TypeDecl ) );
        }

        SqlTypedIdentifier( SqlTypedIdentifier o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
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
            return new SqlTypedIdentifier( this, leading, content, trailing );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

        public SqlTokenIdentifier Identifier => _content.V1;

        /// <summary>
        /// Gets the optional AS token that may appear in function parameters between the parameter name
        /// and the type.
        /// </summary>
        public SqlTokenIdentifier AsT => _content.V2;

        public ISqlUnifiedTypeDecl TypeDecl => _content.V3;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );
    }

}
