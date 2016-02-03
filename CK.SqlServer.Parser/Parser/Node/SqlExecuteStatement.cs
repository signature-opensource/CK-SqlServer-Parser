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

    public sealed class SqlExecuteStatement : SqlNode, ISqlExecuteStatement
    {
        readonly SNode<SqlTokenIdentifier, ISqlIdentifier, SqlTokenTerminal, ISqlIdentifier, SqlCallParameterList, SqlWithOptions, SqlTokenTerminal> _content;

        public SqlExecuteStatement( 
            SqlTokenIdentifier execT, 
            ISqlIdentifier returnVar,
            SqlTokenTerminal returnVarAssign, 
            ISqlIdentifier  name, 
            SqlCallParameterList parameters, 
            SqlWithOptions options,
            SqlTokenTerminal term )
            : base( null, null )
        {
            _content = new SNode<SqlTokenIdentifier, ISqlIdentifier, SqlTokenTerminal, ISqlIdentifier, SqlCallParameterList, SqlWithOptions, SqlTokenTerminal>(
                execT,
                returnVar,
                returnVarAssign,
                name,
                parameters,
                options,
                term );
            CheckContent();
        }

        void CheckContent()
        {
            Helper.CheckToken( ExecT, nameof( ExecT ), SqlTokenType.Execute );
            Helper.CheckNullableToken( ReturnVarAssignT, nameof( ReturnVarAssignT ), SqlTokenType.Assign );
            Helper.CheckBothNullOrNot( ReturnVar, nameof( ReturnVar ), ReturnVarAssignT, nameof( ReturnVarAssignT ) );
            Helper.CheckNotNull( Name, nameof( Name ) );
            Helper.CheckNotNull( Parameters, nameof( Parameters ) );
        }

        SqlExecuteStatement( SqlExecuteStatement o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _content = o._content;
            else
            {
                _content = new SNode<SqlTokenIdentifier, ISqlIdentifier, SqlTokenTerminal, ISqlIdentifier, SqlCallParameterList, SqlWithOptions, SqlTokenTerminal>( items );
                CheckContent();
            }
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlExecuteStatement( this, leading, content, trailing );
        }

        bool ISqlExecuteStatement.IsExecuteString => false;

        public StatementKnownName StatementKnownName => StatementKnownName.Execute;

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

        public SqlTokenIdentifier ExecT => _content.V1;

        public ISqlIdentifier ReturnVar => _content.V2;

        public SqlTokenTerminal ReturnVarAssignT => _content.V3;

        public ISqlIdentifier Name => _content.V4;

        public SqlCallParameterList Parameters => _content.V5;

        public SqlWithOptions Options => _content.V6;

        public SqlTokenTerminal StatementTerminator => _content.V7;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

    }


}
