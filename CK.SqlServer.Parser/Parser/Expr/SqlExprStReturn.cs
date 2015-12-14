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
    public class SqlExprStReturn : SqlExprBaseSt
    {
        public SqlExprStReturn( SqlTokenIdentifier returnToken, SqlExpr value, SqlTokenTerminal terminator )
            : base( Build( returnToken, value ),  terminator )
        {
        }

        SqlExprStReturn( ImmutableList<SqlTrivia> leading, SqlNode[] items, ImmutableList<SqlTrivia> trailing )
            : base( leading, items, trailing )
        {
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IReadOnlyList<SqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlExprStReturn( leading, EnsureArray( children ), trailing );
        }

        static SqlNode[] Build( SqlTokenIdentifier returnToken, SqlExpr value )
        {
            if( returnToken == null || returnToken.TokenType != SqlTokenType.Return ) throw new ArgumentException( "returnToken" );
            return value != null ? CreateArray<SqlNode>( returnToken, value ) : CreateArray( returnToken );
        }

        public SqlTokenIdentifier ReturnT { get { return (SqlTokenIdentifier)Slots[0]; } }
        
        public SqlExpr Value 
        { 
            get 
            {
                // Slots[1] may not exist (return) or be the terminator (return ;).
                return Slots.Length >= 2 ? Slots[1] as SqlExpr : null; 
            } 
        }

        [DebuggerStepThrough]
        internal protected override T Accept<T>( ISqlItemVisitor<T> visitor )
        {
            return visitor.Visit( this );
        }

    }


}
