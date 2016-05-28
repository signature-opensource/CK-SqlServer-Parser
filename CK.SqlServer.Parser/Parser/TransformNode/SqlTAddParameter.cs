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
    using CNode = SNode<
            SqlTokenIdentifier,
            SqlTokenIdentifier,
            SqlParameterList,
            SqlTokenIdentifier,
            SqlTokenIdentifier,
            SqlTokenTerminal>;

    public sealed class SqlTAddParameter : SqlNonToken, ISqlTStatement
    {
         readonly CNode _content;

        public SqlTAddParameter( SqlTokenIdentifier addT, SqlTokenIdentifier parameterT, SqlParameterList parameters, SqlTokenIdentifier beforeOrAfterT, SqlTokenIdentifier paramName, SqlTokenTerminal terminator )
            : base( null, null )
        {
            _content = new CNode( addT, parameterT, parameters, beforeOrAfterT, paramName, terminator );
            CheckContent();
        }

        void CheckContent()
        {
            Helper.CheckToken( AddT, nameof( AddT ), SqlTokenType.Add );
            Helper.CheckToken( ParameterT, nameof( ParameterT ), SqlTokenType.Parameter );
            Helper.CheckNotNull( Parameters, nameof( Parameters ) );
            Helper.CheckNullableToken( AfterOrBeforeT, nameof( AfterOrBeforeT ), SqlTokenType.After, SqlTokenType.Before );
            Helper.CheckNullableToken( ParameterName, nameof( ParameterName ), SqlTokenType.IdentifierVariable );
            Helper.CheckBothNullOrNot( AfterOrBeforeT, nameof( AfterOrBeforeT ), ParameterName, nameof( ParameterName ) );
        }

        SqlTAddParameter( SqlTAddParameter o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
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
            return new SqlTAddParameter( this, leading, content, trailing );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

        public SqlTokenIdentifier AddT => _content.V1;

        public SqlTokenIdentifier ParameterT => _content.V2;

        public SqlParameterList Parameters => _content.V3;

        public SqlTokenIdentifier AfterOrBeforeT => _content.V4;

        public SqlTokenIdentifier ParameterName => _content.V5;

        public SqlTokenTerminal StatementTerminator => _content.V6;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

    }

}
