using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;

namespace CK.SqlServer.Parser
{
    public class SqlExprAssign : SqlExpr
    {
        public SqlExprAssign( ISqlIdentifier identifier, SqlTokenTerminal assignT, SqlExpr right )
            : this( null, Build( identifier, assignT, right ), null )
        {
        }

        static ISqlNode[] Build( ISqlIdentifier identifier, SqlTokenTerminal assignT, SqlExpr right )
        {
            if( identifier == null ) throw new ArgumentNullException( "identifier" );
            if( assignT == null ) throw new ArgumentNullException( "assignTok" );
            if( right == null ) throw new ArgumentNullException( "right" );
            if( (assignT.TokenType & SqlTokenType.IsAssignOperator) == 0 ) throw new ArgumentException( "Invalid assign token.", "assignT" );
            return CreateArray<SqlNode>( SqlToken.EmptyOpenPar, (SqlNode)identifier, assignT, right, SqlToken.EmptyClosePar );
        }

        internal SqlExprAssign( ImmutableList<SqlTrivia> leading, ISqlNode[] items, ImmutableList<SqlTrivia> trailing )
            : base( leading, items, trailing )
        {
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IReadOnlyList<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlExprAssign( leading, EnsureArray( children ), trailing );
        }

        public ISqlIdentifier Identifier { get { return (ISqlIdentifier)Slots[1]; } }

        public SqlTokenTerminal AssignT { get { return (SqlTokenTerminal)Slots[2]; } }

        public SqlExpr Right { get { return (SqlExpr)Slots[3]; } }

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor )
        {
            return visitor.Visit( this );
        }
    }
}
