#region Proprietary License
/*----------------------------------------------------------------------------
* This file (CK.SqlServer.Parser\Parser\Expr\Select\SelectColumn.cs) is part of CK-Database. 
* Copyright © 2007-2014, Invenietis <http://www.invenietis.com>. All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;

namespace CK.SqlServer.Parser
{
    /// <summary>
    /// Captures a select column definition. 
    /// </summary>
    public class SelectColumn : SqlItem
    {
        readonly ISqlIdentifier _colName;
        readonly SqlToken _asOrEqual;
        readonly SqlExpr _definition;

        static readonly SqlTokenIdentifier _autoAsT = new SqlTokenIdentifier( SqlTokenType.As, "as", SqlTrivia.OneSpace, SqlTrivia.OneSpace );
        static readonly SqlTokenIdentifier _autoAsTNoLeft = new SqlTokenIdentifier( SqlTokenType.As, "as", null, SqlTrivia.OneSpace );
        static readonly SqlTokenIdentifier _autoAsTNoRight = new SqlTokenIdentifier( SqlTokenType.As, "as", SqlTrivia.OneSpace, null );
        static readonly SqlTokenIdentifier _autoAsTNoSpace = new SqlTokenIdentifier( SqlTokenType.As, "as", null, null );

        public SelectColumn( ISqlIdentifier colName, SqlTokenTerminal assignT, SqlExpr definition )
            : this( null, Build( colName, assignT, definition ), null )
        {
        }

        public SelectColumn( SqlExpr definition, SqlTokenIdentifier asT, ISqlIdentifier colName )
            : this( null, Build( definition, asT, colName ), null )
        {
        }

        public SelectColumn( SqlExpr definition, ISqlIdentifier colName = null )
            : this( null, Build( definition, colName ), null )
        {
        }

        static SqlNode[] Build( ISqlIdentifier colName, SqlTokenTerminal assignT, SqlExpr definition )
        {
            if( colName == null ) throw new ArgumentNullException( "colName" );
            if( assignT == null ) throw new ArgumentNullException( "assignT" );
            if( assignT.TokenType != SqlTokenType.Assign ) throw new ArgumentException( "Assign token expected.", "assignT" );
            if( definition == null ) throw new ArgumentNullException( "definition" );
            return CreateArray( (SqlNode)colName, assignT, definition );
        }

        static SqlNode[] Build( SqlExpr definition, SqlTokenIdentifier asT, ISqlIdentifier colName )
        {
            if( definition == null ) throw new ArgumentNullException( "definition" );
            if( colName == null ) throw new ArgumentNullException( "colName" );
            if( asT == null )
            {
                var leftTrivia = definition.FullTrailingTrivias.Any();
                var rightTrivia = colName.FullLeadingTrivias.Any();
                if( !leftTrivia )
                {
                    if( !rightTrivia ) asT = _autoAsT;
                    else asT = _autoAsTNoRight;
                }
                else
                {
                    if( !rightTrivia )
                        asT = _autoAsTNoLeft;
                    else asT = _autoAsTNoSpace;
                }
            }
            else if( asT.TokenType != SqlTokenType.As ) throw new ArgumentException( "As token expected.", "asT" );
            return CreateArray<SqlNode>( definition, asT, (SqlNode)colName );
        }

        static SqlNode[] Build( SqlExpr definition, ISqlIdentifier colName )
        {
            if( definition == null ) throw new ArgumentNullException( "definition" );
            if( colName == null ) return CreateArray( definition );
            return Build( definition, null, colName );
        }

        internal SelectColumn( ImmutableList<SqlTrivia> leading, SqlNode[] items, ImmutableList<SqlTrivia> trailing )
            : base( leading, items, trailing )
        {
            if( Slots.Length == 1 ) _definition = (SqlExpr)Slots[0];
            else
            {
                _asOrEqual = (SqlToken)Slots[1];
                if( _asOrEqual is SqlTokenTerminal )
                {
                    _colName = (ISqlIdentifier)Slots[0];
                    _definition = (SqlExpr)Slots[2];
                }
                else
                {
                    _colName = (ISqlIdentifier)Slots[2];
                    _definition = (SqlExpr)Slots[0];
                }
            }
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IReadOnlyList<SqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SelectColumn( leading, EnsureArray( children ), trailing );
        }

        public ISqlIdentifier ColumnName { get { return _colName; } }

        public bool IsEqualSyntax { get { return _asOrEqual is SqlTokenTerminal; } }

        public bool IsAsSyntax { get { return _asOrEqual is SqlTokenIdentifier; } }

        public SqlToken AsOrEqualT { get { return _asOrEqual; } }
        
        public SqlExpr Definition { get { return _definition; } }

        [DebuggerStepThrough]
        internal protected override T Accept<T>( ISqlItemVisitor<T> visitor )
        {
            return visitor.Visit( this );
        }
    }


}
