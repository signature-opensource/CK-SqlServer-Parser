#region Proprietary License
/*----------------------------------------------------------------------------
* This file (CK.SqlServer.Parser\Parser\Expr\Select\SelectHeader.cs) is part of CK-Database. 
* Copyright © 2007-2014, Invenietis <http://www.invenietis.com>. All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

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
    /// Captures SELECT [ ALL | DISTINCT ] [TOP ( expression ) [PERCENT] [ WITH TIES ] ] 
    /// </summary>
    public class SelectHeader : SqlItem
    {
        readonly SqlTokenIdentifier _allOrDistinct;
        readonly SqlTokenIdentifier _top;
        readonly SqlExpr _topExpression;
        readonly SqlTokenIdentifier _percent;
        readonly bool _withTies;

        public SelectHeader( SqlTokenIdentifier select, SqlTokenIdentifier allOrDistinct = null, SqlTokenIdentifier top = null, SqlExpr topExpression = null, SqlTokenIdentifier percent = null, SqlTokenIdentifier with = null, SqlTokenIdentifier ties = null )
            : this( null, Build( select, allOrDistinct, top, topExpression, percent, with, ties ), null )
        {
        }

        internal SelectHeader( ImmutableList<SqlTrivia> leading, ISqlNode[] items, ImmutableList<SqlTrivia> trailing )
            : base( leading, items, trailing )
        {
            _allOrDistinct = (SqlTokenIdentifier)Slots.FirstOrDefault( t => t.IsToken( SqlTokenType.All ) || t.IsToken( SqlTokenType.Distinct ) );
            _top = (SqlTokenIdentifier)Slots.FirstOrDefault( t => t.IsToken( SqlTokenType.Top ) );
            _topExpression = (SqlExpr)Slots.FirstOrDefault( t => t is SqlExpr );
            _percent = (SqlTokenIdentifier)Slots.FirstOrDefault( t => t.IsToken( SqlTokenType.Percent ) );
            _withTies = Slots.Any( t => t.IsToken( SqlTokenType.With ) );
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IReadOnlyList<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SelectHeader( leading, EnsureArray( children ), trailing );
        }

        static ISqlNode[] Build( SqlTokenIdentifier select, SqlTokenIdentifier allOrDistinct, SqlTokenIdentifier top, SqlExpr topExpression, SqlTokenIdentifier percent, SqlTokenIdentifier with, SqlTokenIdentifier ties )
        {
            var exprs = new List<SqlNode>( 9 );
            if( select == null ) throw new ArgumentNullException( "select" );
            exprs.Add( select );
            if( allOrDistinct != null ) exprs.Add( allOrDistinct );
            if( top != null )
            {
                if( topExpression == null ) throw new ArgumentNullException( "topExpression" );
                exprs.Add( top );
                exprs.Add( topExpression );
            }
            if( percent != null ) exprs.Add( percent );
            if( with != null )
            {
                if( ties == null ) throw new ArgumentNullException( "ties" );
                exprs.Add( with );
                exprs.Add( ties );
            }
            return exprs.ToArray();
        }

        public SqlTokenIdentifier SelectT { get { return (SqlTokenIdentifier)Slots[0]; } }
        public SqlTokenIdentifier AllOrDistinctT { get { return _allOrDistinct; } }
        public SqlTokenIdentifier TopT { get { return _top; } }
        public SqlExpr TopExpression { get { return _topExpression; } }
        public SqlTokenIdentifier PercentT { get { return _percent; } }
        public bool WithTies { get { return _withTies; } }

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor )
        {
            return visitor.Visit( this );
        }
    }


}
