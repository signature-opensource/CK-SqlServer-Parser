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
    public class SqlExprStSetOpt : SqlExprBaseSt
    {
        public SqlExprStSetOpt( SqlTokenIdentifier setToken, SqlExpr list, SqlTokenTerminal terminator )
            : base( Build( setToken, list ),  terminator )
        {
        }

        SqlExprStSetOpt( ImmutableList<SqlTrivia> leading, SqlNode[] items, ImmutableList<SqlTrivia> trailing )
            : base( leading, items, trailing )
        {
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IReadOnlyList<SqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlExprStSetOpt( leading, EnsureArray( children ), trailing );
        }

        static SqlNode[] Build( SqlTokenIdentifier setToken, SqlExpr list )
        {
            if( setToken == null || setToken.TokenType != SqlTokenType.Set ) throw new ArgumentException( "setToken" );
            if( list == null ) throw new ArgumentException( "list" );
            return CreateArray<SqlNode>( setToken, list );
        }

        public SqlTokenIdentifier SetT { get { return (SqlTokenIdentifier)Slots[0]; } }

        public SqlExpr List { get { return (SqlExpr)Slots[1]; } }

        [DebuggerStepThrough]
        internal protected override T Accept<T>( ISqlItemVisitor<T> visitor )
        {
            return visitor.Visit( this );
        }

    }


}
