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
    public class SqlExprDeclare : SqlItem
    {
        public SqlExprDeclare( SqlExprTypedIdentifier declVar, SqlTokenTerminal assignToken = null, SqlExpr initialValue = null )
            : this( null, Build( declVar, assignToken, initialValue ), null )
        {
        }

        static SqlNode[] Build( SqlExprTypedIdentifier declVar, SqlTokenTerminal assignToken = null, SqlExpr initialValue = null )
        {
            if( declVar == null ) throw new ArgumentNullException( "declVar" );
            if( !declVar.Identifier.IsVariable ) throw new ArgumentException( "Must be a @VariableName", "variable" );
            if( assignToken != null )
            {
                if( assignToken.TokenType != SqlTokenType.Assign ) throw new ArgumentException( "Must be '='.", "assignToken" );
                if( initialValue == null ) throw new ArgumentNullException( "initialValue" );
            }
            else if( initialValue != null ) throw new ArgumentNullException( "assignToken" );
            
            if( assignToken == null )
            {
                return CreateArray<SqlNode>( declVar );
            }
            else
            {
                return CreateArray<SqlNode>( declVar, assignToken, initialValue );
            }
        }

        internal SqlExprDeclare( ImmutableList<SqlTrivia> leading, SqlNode[] items, ImmutableList<SqlTrivia> trailing )
            : base( leading, items, trailing )
        {
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IReadOnlyList<SqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlExprDeclare( leading, EnsureArray( children ), trailing );
        }

        public SqlExprTypedIdentifier Variable { get { return (SqlExprTypedIdentifier)Slots[0]; } }

        public SqlTokenTerminal AssignT { get { return Slots.Length > 1 ? Slots[1] as SqlTokenTerminal : null; } }

        public SqlExpr InitialValue { get { return Slots.Length > 1 ? Slots[2] as SqlExpr : null; } }
        
        public bool HasInitialValue { get { return Slots.Length > 1; } }

        [DebuggerStepThrough]
        internal protected override SqlNode Accept( SqlItemVisitor visitor )
        {
            return visitor.Visit( this );
        }

    }

}
