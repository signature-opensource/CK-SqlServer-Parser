using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using CK.Core;
using System.Collections.Immutable;

namespace CK.SqlServer.Parser
{
    public class SqlExprStStoredProc : SqlExprBaseSt, ISqlServerStoredProcedure
    {
        public SqlExprStStoredProc( 
            SqlTokenIdentifier alterOrCreate, 
            SqlTokenIdentifier type, 
            SqlExprMultiIdentifier name, 
            SqlExprParameterList parameters, 
            SqlExprUnmodeledItems options, 
            SqlTokenIdentifier asToken, 
            SqlExprStatementList bodyStatements, 
            SqlTokenTerminal term )
            : base( Build( alterOrCreate, type, name, parameters, options, asToken, null, bodyStatements, null ), term )
        {
        }

        public SqlExprStStoredProc( 
            SqlTokenIdentifier alterOrCreate, 
            SqlTokenIdentifier type, 
            SqlExprMultiIdentifier name, 
            SqlExprParameterList parameters, 
            SqlExprUnmodeledItems options, 
            SqlTokenIdentifier asToken, 
            SqlTokenIdentifier begin, 
            SqlExprStatementList bodyStatements, 
            SqlTokenIdentifier end, 
            SqlTokenTerminal term )
            : base( Build( alterOrCreate, type, name, parameters, options, asToken, begin, bodyStatements, end ), term )
        {
        }

        SqlExprStStoredProc( ImmutableList<SqlTrivia> leading, ISqlNode[] items, ImmutableList<SqlTrivia> trailing )
            : base( leading, items, trailing )
        {
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IReadOnlyList<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlExprStStoredProc( leading, EnsureArray( children ), trailing );
        }

        static ISqlNode[] Build( SqlTokenIdentifier alterOrCreate, SqlTokenIdentifier type, SqlExprMultiIdentifier name, SqlExprParameterList parameters, SqlExprUnmodeledItems options, SqlTokenIdentifier asToken, SqlTokenIdentifier begin, SqlExprStatementList bodyStatements, SqlTokenIdentifier end )
        {
            if( options != null )
            {
                if( begin != null )
                {
                    if( end == null ) throw new ArgumentNullException( "end can not be null if begin exists." );
                    return CreateArray<SqlNode>( alterOrCreate, type, name, parameters, options, asToken, begin, bodyStatements, end );
                }
                else
                {
                    return CreateArray<SqlNode>( alterOrCreate, type, name, parameters, options, asToken, bodyStatements );
                }
            }
            else
            {
                if( begin != null )
                {
                    if( end == null ) throw new ArgumentNullException( "end can not be null if begin exists." );
                    return CreateArray<SqlNode>( alterOrCreate, type, name, parameters, asToken, begin, bodyStatements, end );
                }
                else
                {
                    return CreateArray<SqlNode>( alterOrCreate, type, name, parameters, asToken, bodyStatements );
                }
            }
        }

        public SqlTokenIdentifier AlterOrCreateT { get { return (SqlTokenIdentifier)Slots[0]; } }

        public SqlTokenIdentifier ObjectTypeT { get { return (SqlTokenIdentifier)Slots[1]; } }

        /// <summary>
        /// Gets the name of the procedure (may start with the Schema).
        /// </summary>
        public string ObjectName 
        {
            get { return Name.ToString(); } 
        }

        /// <summary>
        /// Gets the name of the procedure (may start with the Schema).
        /// </summary>
        public SqlExprMultiIdentifier Name { get { return (SqlExprMultiIdentifier)Slots[2]; } }

        public SqlExprParameterList Parameters { get { return (SqlExprParameterList)Slots[3]; } }

        ISqlServerParameterList ISqlServerCallableObject.Parameters { get { return (ISqlServerParameterList)Slots[3]; } }

        SqlServerObjectType ISqlServerObject.ObjectType { get { return SqlServerObjectType.Procedure; } }

        string ISqlServerObject.ToStringSignature( bool withOptions )
        {
            return withOptions ? Header.ToStringCompact() : Slots.Skip( 1 ).Take( 3 ).ToStringCompact();
        }

        public bool HasOptions { get { return SlotsLengthWithoutTerminator == 9 || SlotsLengthWithoutTerminator == 7; } }

        public SqlExprUnmodeledItems Options { get { return HasOptions ? (SqlExprUnmodeledItems)Slots[4] : null; } }

        public IEnumerable<ISqlNode> Header => Slots.Skip( 1 ).Take( HasOptions ? 4 : 3 );

        public SqlTokenIdentifier AsT { get { return (SqlTokenIdentifier)Slots[HasOptions ? 5 : 4]; } }

        public bool HasBeginEnd { get { return SlotsLengthWithoutTerminator == 8 || SlotsLengthWithoutTerminator == 6; } }

        public SqlTokenIdentifier BeginT { get { return HasBeginEnd ? (SqlTokenIdentifier)Slots[SlotsLengthWithoutTerminator - 3] : null; } }

        public SqlExprStatementList BodyStatements { get { return (SqlExprStatementList)Slots[HasBeginEnd ? SlotsLengthWithoutTerminator - 2 : SlotsLengthWithoutTerminator - 1]; } }

        public SqlTokenIdentifier EndT { get { return HasBeginEnd ? (SqlTokenIdentifier)Slots[ SlotsLengthWithoutTerminator - 1 ] : null; } }

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor )
        {
            return visitor.Visit( this );
        }

    }
}
