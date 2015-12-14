#region Proprietary License
/*----------------------------------------------------------------------------
* This file (CK.SqlServer.Parser\Parser\SqlItemVisitor.cs) is part of CK-Database. 
* Copyright © 2007-2014, Invenietis <http://www.invenietis.com>. All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using CK.Core;

namespace CK.SqlServer.Parser
{
    public class SqlItemVisitor : ISqlItemVisitor<SqlItem>
    {
        public virtual SqlItem VisitItem( SqlItem e )
        {
            return e.Accept( this );
        }

        protected List<SqlNode> VisitItems( IEnumerable<SqlNode> nodes, SqlNode prefixToKeep = null, SqlNode suffixToKeep = null )
        {
            List<SqlNode> modified = null;
            int i = 0;
            foreach( var a in nodes )
            {
                var ce = a as SqlItem;
                if( ce != null )
                {
                    SqlItem ve = VisitItem( ce );
                    if( !ReferenceEquals( ce, ve ) )
                    {
                        if( modified == null )
                        {
                            modified = new List<SqlNode>( i+1 );
                            if( prefixToKeep != null ) modified.Add( prefixToKeep );
                            if( i > 0 )
                            {
                                using( var oldE = nodes.GetEnumerator() )
                                {
                                    int j = i;
                                    while( --j > 0 ) 
                                    {
                                        oldE.MoveNext();
                                        modified.Add( oldE.Current );
                                    }
                                }
                            }
                        }
                        modified[i] = ve;
                    }
                }
                ++i;
            }
            if( modified != null && suffixToKeep != null ) modified.Add( suffixToKeep );
            return modified;
        }

        protected SqlExpr VisitStandard( SqlExpr e )
        {
            List<SqlNode> modified = VisitItems( e.ItemsWithoutParenthesis, e.Opener, e.Closer );
            if( modified == null ) return e;
            return (SqlExpr)e.InternalClone( e.LeadingTrivias, modified, e.TrailingTrivias );
        }

        protected SqlItem VisitStandard( SqlItem e )
        {
            List<SqlNode> modified = VisitItems( e.ChildrenNodes );
            if( modified == null ) return e;
            return (SqlItem)e.InternalClone( e.LeadingTrivias, modified, e.TrailingTrivias );
        }

        public virtual SqlItem Visit( SqlPar e )
        {
            return VisitStandard( e );
        }

        public virtual SqlItem Visit( SqlExprUnmodeledItems e )
        {
            return VisitStandard( e );
        }

        public virtual SqlItem Visit( SqlExprRawItemList e )
        {
            return VisitStandard( e );
        }

        public virtual SqlItem Visit( SqlExprKoCall e )
        {
            return VisitStandard( e );
        }

        public virtual SqlItem Visit( SqlNoExprOverClause e )
        {
            return VisitStandard( e );
        }

        public virtual SqlItem Visit( SqlExprCollate e )
        {
            return VisitStandard( e );
        }

        public virtual SqlItem Visit( SqlExprStIf e )
        {
            return VisitStandard( e );
        }

        public SqlItem Visit( SqlExprCursor e )
        {
            return VisitStandard( e );
        }

        public SqlItem Visit( SqlNoExprIdentifierList e )
        {
            return VisitStandard( e );
        }

        public SqlItem Visit( SqlExprCursorSql92 e )
        {
            return VisitStandard( e );
        }

        public virtual SqlItem Visit( SqlExprStDeclareCursor e )
        {
            return VisitStandard( e );
        }

        public virtual SqlItem Visit( SqlExprStBeginTran e )
        {
            return VisitStandard( e );
        }

        public virtual SqlItem Visit( SqlExprStatementList e )
        {
            return VisitStandard( e );
        }

        public virtual SqlItem Visit( SqlExprStBlock e )
        {
            return VisitStandard( e );
        }

        public virtual SqlItem Visit( SqlExprStTryCatch e )
        {
            return VisitStandard( e );
        }

        public virtual SqlItem Visit( SqlExprStUnmodeled e )
        {
            return VisitStandard( e );
        }

        public virtual SqlItem Visit( SqlExprStStoredProc e )
        {
            return VisitStandard( e );
        }

        public virtual SqlItem Visit( SqlExprStFunctionScalar e )
        {
            return VisitStandard( e );
        }

        public virtual SqlItem Visit( SqlExprStReturn e )
        {
            return VisitStandard( e );
        }

        public virtual SqlItem Visit( SqlExprStSetVar e )
        {
            return VisitStandard( e );
        }

        public virtual SqlItem Visit( SqlExprStSetOpt e )
        {
            return VisitStandard( e );
        }

        public virtual SqlItem Visit( SqlExprStGoto e )
        {
            return VisitStandard( e );
        }

        public virtual SqlItem Visit( SqlExprStMonoStatement e )
        {
            return e;
        }

        public virtual SqlItem Visit( SqlExprStLabelDef e )
        {
            return e;
        }

        public virtual SqlItem Visit( SqlExprStEmpty e )
        {
            return e;
        }

        public virtual SqlItem Visit( SqlExprStView e )
        {
            return VisitStandard( e );
        }

        public virtual SqlItem Visit( SqlExprColumnList e )
        {
            return VisitStandard( e );
        }

        public virtual SqlItem Visit( SqlNoExprExecuteAs e )
        {
            return VisitStandard( e );
        }

        public virtual SqlItem Visit( SqlExprStDeclare e )
        {
            return VisitStandard( e );
        }

        public virtual SqlItem Visit( SqlExprDeclareList e )
        {
            return VisitStandard( e );
        }

        public virtual SqlItem Visit( SqlExprDeclare e )
        {
            return VisitStandard( e );
        }

        public virtual SqlItem Visit( SqlExprCast e )
        {
            return VisitStandard( e );
        }

        public virtual SqlItem Visit( SqlExprCommaList e )
        {
            return VisitStandard( e );
        }

        public virtual SqlItem Visit( SqlExprIdentifier e )
        {
            return e;
        }

        public virtual SqlItem Visit( SqlExprMultiIdentifier e )
        {
            return e;
        }

        public virtual SqlItem Visit( SqlExprLiteral e )
        {
            return e;
        }

        public virtual SqlItem Visit( SqlExprNull e )
        {
            return e;
        }

        public virtual SqlItem Visit( SqlExprUnaryOperator e )
        {
            return VisitStandard( e );
        }

        public virtual SqlItem Visit( SqlExprTypeDecl e )
        {
            return VisitStandard( e );
        }

        public virtual SqlItem Visit( SqlExprTypeDeclDecimal e )
        {
            return e;
        }

        public virtual SqlItem Visit( SqlExprTypeDeclDateAndTime e )
        {
            return e;
        }

        public virtual SqlItem Visit( SqlExprTypeDeclSimple e )
        {
            return e;
        }

        public virtual SqlItem Visit( SqlExprTypeDeclWithSize e )
        {
            return e;
        }

        public virtual SqlItem Visit( SqlExprTypeDeclUserDefined e )
        {
            return e;
        }

        public virtual SqlItem Visit( SqlExprTypedIdentifier e )
        {
            return e;
        }

        public virtual SqlItem Visit( SqlExprParameter e )
        {
            return VisitStandard( e );
        }

        public virtual SqlItem Visit( SqlExprParameterDefaultValue e )
        {
            return e;
        }

        public virtual SqlItem Visit( SqlExprParameterList e )
        {
            return VisitStandard( e );
        }

        public virtual SqlItem Visit( SqlExprAssign e )
        {
            return VisitStandard( e );
        }

        public virtual SqlItem Visit( SqlExprBinaryOperator e )
        {
            return VisitStandard( e );
        }

        public virtual SqlItem Visit( SqlExprIsNull e )
        {
            return VisitStandard( e );
        }

        public virtual SqlItem Visit( SqlExprLike e )
        {
            return VisitStandard( e );
        }

        public virtual SqlItem Visit( SqlExprBetween e )
        {
            return VisitStandard( e );
        }

        public virtual SqlItem Visit( SqlExprIn e )
        {
            return VisitStandard( e );
        }

        public virtual SqlItem Visit( SqlExprCase e )
        {
            return VisitStandard( e );
        }

        public virtual SqlItem Visit( SqlExprCaseWhenSelector e )
        {
            return VisitStandard( e );
        }


        #region Select

        public virtual SqlItem Visit( SelectQuery e )
        {
            return VisitStandard( e );
        }

        public virtual SqlItem Visit( SelectSpecification e )
        {
            return VisitStandard( e );
        }

        public virtual SqlItem Visit( SelectColumn e )
        {
            return VisitStandard( e );
        }

        public virtual SqlItem Visit( SelectColumnList e )
        {
            return VisitStandard( e );
        }

        public virtual SqlItem Visit( SelectHeader e )
        {
            return VisitStandard( e );
        }

        public virtual SqlItem Visit( SelectInto e )
        {
            return VisitStandard( e );
        }

        public virtual SqlItem Visit( SelectFrom e )
        {
            return VisitStandard( e );
        }

        public virtual SqlItem Visit( SelectWhere e )
        {
            return VisitStandard( e );
        }

        public virtual SqlItem Visit( SelectGroupBy e )
        {
            return VisitStandard( e );
        }

        public virtual SqlItem Visit( SelectCombineOperator e )
        {
            return VisitStandard( e );
        }

        public virtual SqlItem Visit( SelectOrderBy e )
        {
            return VisitStandard( e );
        }

        public virtual SqlItem Visit( SelectOrderByColumnList e )
        {
            return VisitStandard( e );
        }

        public virtual SqlItem Visit( SelectOrderByColumn e )
        {
            return VisitStandard( e );
        }

        public virtual SqlItem Visit( SelectOrderByOffset e )
        {
            return VisitStandard( e );
        }

        public virtual SqlItem Visit( SelectFor e )
        {
            return VisitStandard( e );
        }

        public virtual SqlItem Visit( SelectOption e )
        {
            return VisitStandard( e );
        }

        #endregion

        public virtual SqlItem Visit( SqlExprStFunctionInlineTable e )
        {
            return VisitStandard( e );
        }
    }
}
