#region Proprietary License
/*----------------------------------------------------------------------------
* This file (CK.SqlServer.Parser\Parser\Expr\SqlExprStFunctionScalar.cs) is part of CK-Database. 
* Copyright © 2007-2014, Invenietis <http://www.invenietis.com>. All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using CK.Core;
using System.Collections.Immutable;

namespace CK.SqlServer.Parser
{
    public class SqlExprStFunctionScalar : SqlExprStFunction
    {
        public SqlExprStFunctionScalar( 
            SqlTokenIdentifier alterOrCreate, 
            SqlTokenIdentifier type, 
            SqlExprMultiIdentifier name, 
            SqlExprParameterList parameters,
            SqlTokenIdentifier returns,
            SqlExprTypeDecl returrnScalarType,
            SqlExprUnmodeledItems options,
            SqlTokenIdentifier asToken,
            SqlTokenIdentifier begin,
            SqlExprStatementList bodyStatements, 
            SqlTokenIdentifier end, 
            SqlTokenTerminal term )
            : base( Build( alterOrCreate, type, name, parameters, returns, returrnScalarType, options, asToken, begin, bodyStatements, end ), term )
        {
        }

        SqlExprStFunctionScalar( ImmutableList<SqlTrivia> leading, ISqlNode[] items, ImmutableList<SqlTrivia> trailing )
            : base( leading, items, trailing )
        {
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IReadOnlyList<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlExprStFunctionScalar( leading, EnsureArray( children ), trailing );
        }

        static ISqlNode[] Build(
            SqlTokenIdentifier alterOrCreate,
            SqlTokenIdentifier type,
            SqlExprMultiIdentifier name,
            SqlExprParameterList parameters,
            SqlTokenIdentifier returns,
            SqlExprTypeDecl returrnScalarType,
            SqlExprUnmodeledItems options,
            SqlTokenIdentifier asToken,
            SqlTokenIdentifier begin,
            SqlExprStatementList bodyStatements,
            SqlTokenIdentifier end )
        {
            if( options != null )
            {
                if( asToken != null )
                {
                    return CreateArray<SqlNode>( alterOrCreate, type, name, parameters, returns, returrnScalarType, options, asToken, begin, bodyStatements, end );
                }
                else
                {
                    return CreateArray<SqlNode>( alterOrCreate, type, name, parameters, returns, returrnScalarType, options, begin, bodyStatements, end );
                }
            }
            else
            {
                if( asToken != null )
                {
                    return CreateArray<SqlNode>( alterOrCreate, type, name, parameters, returns, returrnScalarType, asToken, begin, bodyStatements, end );
                }
                else
                {
                    return CreateArray<SqlNode>( alterOrCreate, type, name, parameters, returns, returrnScalarType, begin, bodyStatements, end );
                }
            }
        }

        public SqlExprTypeDecl ReturnedType { get { return (SqlExprTypeDecl)Slots[5]; } }

        public SqlTokenIdentifier BeginT { get { return (SqlTokenIdentifier)Slots[SlotsLengthWithoutTerminator - 3]; } }

        public SqlExprStatementList BodyStatements { get { return (SqlExprStatementList)Slots[SlotsLengthWithoutTerminator - 2]; } }

        public SqlTokenIdentifier EndT { get { return (SqlTokenIdentifier)Slots[SlotsLengthWithoutTerminator - 1]; } }

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor )
        {
            return visitor.Visit( this );
        }


    }
}
