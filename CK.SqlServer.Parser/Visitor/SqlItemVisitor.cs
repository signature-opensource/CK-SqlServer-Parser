using System;
using System.Collections.Generic;
using System.Linq;
using CK.Core;

namespace CK.SqlServer.Parser
{
    public class SqlItemVisitor : ISqlItemVisitor<SqlNode>
    {
        public virtual SqlNode VisitItem( SqlNode e )
        {
            return e.Accept( this );
        }

        protected List<SqlNode> VisitItems( IEnumerable<SqlNode> nodes, SqlNode prefixToKeep = null, SqlNode suffixToKeep = null )
        {
            List<SqlNode> modified = null;
            int i = 0;
            foreach( var a in nodes )
            {
                var ve = VisitItem( a );
                if( !ReferenceEquals( a, ve ) )
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

        protected SqlNode VisitStandard( SqlNode e )
        {
            List<SqlNode> modified = VisitItems( e.ChildrenNodes );
            if( modified == null ) return e;
            return (SqlItem)e.InternalClone( e.LeadingTrivias, modified, e.TrailingTrivias );
        }

        public virtual SqlNode Visit( SqlPar e )
        {
            return VisitStandard( e );
        }

        public virtual SqlNode Visit( SqlExprUnmodeledItems e )
        {
            return VisitStandard( e );
        }

        public virtual SqlNode Visit( SqlExprRawItemList e )
        {
            return VisitStandard( e );
        }

        public virtual SqlNode Visit( SqlExprKoCall e )
        {
            return VisitStandard( e );
        }

        public virtual SqlNode Visit( SqlNoExprOverClause e )
        {
            return VisitStandard( e );
        }

        public virtual SqlNode Visit( SqlExprCollate e )
        {
            return VisitStandard( e );
        }

        public virtual SqlNode Visit( SqlExprStIf e )
        {
            return VisitStandard( e );
        }

        public SqlNode Visit( SqlExprCursor e )
        {
            return VisitStandard( e );
        }

        public SqlNode Visit( SqlNoExprIdentifierList e )
        {
            return VisitStandard( e );
        }

        public SqlNode Visit( SqlExprCursorSql92 e )
        {
            return VisitStandard( e );
        }

        public virtual SqlNode Visit( SqlExprStDeclareCursor e )
        {
            return VisitStandard( e );
        }

        public virtual SqlNode Visit( SqlExprStBeginTran e )
        {
            return VisitStandard( e );
        }

        public virtual SqlNode Visit( SqlExprStatementList e )
        {
            return VisitStandard( e );
        }

        public virtual SqlNode Visit( SqlExprStBlock e )
        {
            return VisitStandard( e );
        }

        public virtual SqlNode Visit( SqlExprStTryCatch e )
        {
            return VisitStandard( e );
        }

        public virtual SqlNode Visit( SqlExprStUnmodeled e )
        {
            return VisitStandard( e );
        }

        public virtual SqlNode Visit( SqlExprStStoredProc e )
        {
            return VisitStandard( e );
        }

        public virtual SqlNode Visit( SqlExprStFunctionScalar e )
        {
            return VisitStandard( e );
        }

        public virtual SqlNode Visit( SqlExprStReturn e )
        {
            return VisitStandard( e );
        }

        public virtual SqlNode Visit( SqlExprStSetVar e )
        {
            return VisitStandard( e );
        }

        public virtual SqlNode Visit( SqlExprStSetOpt e )
        {
            return VisitStandard( e );
        }

        public virtual SqlNode Visit( SqlExprStGoto e )
        {
            return VisitStandard( e );
        }

        public virtual SqlNode Visit( SqlExprStMonoStatement e )
        {
            return e;
        }

        public virtual SqlNode Visit( SqlExprStLabelDef e )
        {
            return e;
        }

        public virtual SqlNode Visit( SqlExprStEmpty e )
        {
            return e;
        }

        public virtual SqlNode Visit( SqlExprStView e )
        {
            return VisitStandard( e );
        }

        public virtual SqlNode Visit( SqlExprColumnList e )
        {
            return VisitStandard( e );
        }

        public virtual SqlNode Visit( SqlNoExprExecuteAs e )
        {
            return VisitStandard( e );
        }

        public virtual SqlNode Visit( SqlExprStDeclare e )
        {
            return VisitStandard( e );
        }

        public virtual SqlNode Visit( SqlExprDeclareList e )
        {
            return VisitStandard( e );
        }

        public virtual SqlNode Visit( SqlExprDeclare e )
        {
            return VisitStandard( e );
        }

        public virtual SqlNode Visit( SqlExprCast e )
        {
            return VisitStandard( e );
        }

        public virtual SqlNode Visit( SqlExprCommaList e )
        {
            return VisitStandard( e );
        }

        public virtual SqlNode Visit( SqlExprIdentifier e )
        {
            return e;
        }

        public virtual SqlNode Visit( SqlExprMultiIdentifier e )
        {
            return e;
        }

        public virtual SqlNode Visit( SqlExprLiteral e )
        {
            return e;
        }

        public virtual SqlNode Visit( SqlExprNull e )
        {
            return e;
        }

        public virtual SqlNode Visit( SqlExprUnaryOperator e )
        {
            return VisitStandard( e );
        }

        public virtual SqlNode Visit( SqlExprTypeDecl e )
        {
            return VisitStandard( e );
        }

        public virtual SqlNode Visit( SqlExprTypeDeclDecimal e )
        {
            return e;
        }

        public virtual SqlNode Visit( SqlExprTypeDeclDateAndTime e )
        {
            return e;
        }

        public virtual SqlNode Visit( SqlExprTypeDeclSimple e )
        {
            return e;
        }

        public virtual SqlNode Visit( SqlExprTypeDeclWithSize e )
        {
            return e;
        }

        public virtual SqlNode Visit( SqlExprTypeDeclUserDefined e )
        {
            return e;
        }

        public virtual SqlNode Visit( SqlExprTypedIdentifier e )
        {
            return e;
        }

        public virtual SqlNode Visit( SqlExprParameter e )
        {
            return VisitStandard( e );
        }

        public virtual SqlNode Visit( SqlExprParameterDefaultValue e )
        {
            return e;
        }

        public virtual SqlNode Visit( SqlExprParameterList e )
        {
            return VisitStandard( e );
        }

        public virtual SqlNode Visit( SqlExprAssign e )
        {
            return VisitStandard( e );
        }

        public virtual SqlNode Visit( SqlExprBinaryOperator e )
        {
            return VisitStandard( e );
        }

        public virtual SqlNode Visit( SqlExprIsNull e )
        {
            return VisitStandard( e );
        }

        public virtual SqlNode Visit( SqlExprLike e )
        {
            return VisitStandard( e );
        }

        public virtual SqlNode Visit( SqlExprBetween e )
        {
            return VisitStandard( e );
        }

        public virtual SqlNode Visit( SqlExprIn e )
        {
            return VisitStandard( e );
        }

        public virtual SqlNode Visit( SqlExprCase e )
        {
            return VisitStandard( e );
        }

        public virtual SqlNode Visit( SqlExprCaseWhenSelector e )
        {
            return VisitStandard( e );
        }


        #region Select

        public virtual SqlNode Visit( SelectQuery e )
        {
            return VisitStandard( e );
        }

        public virtual SqlNode Visit( SelectSpecification e )
        {
            return VisitStandard( e );
        }

        public virtual SqlNode Visit( SelectColumn e )
        {
            return VisitStandard( e );
        }

        public virtual SqlNode Visit( SelectColumnList e )
        {
            return VisitStandard( e );
        }

        public virtual SqlNode Visit( SelectHeader e )
        {
            return VisitStandard( e );
        }

        public virtual SqlNode Visit( SelectInto e )
        {
            return VisitStandard( e );
        }

        public virtual SqlNode Visit( SelectFrom e )
        {
            return VisitStandard( e );
        }

        public virtual SqlNode Visit( SelectWhere e )
        {
            return VisitStandard( e );
        }

        public virtual SqlNode Visit( SelectGroupBy e )
        {
            return VisitStandard( e );
        }

        public virtual SqlNode Visit( SelectCombineOperator e )
        {
            return VisitStandard( e );
        }

        public virtual SqlNode Visit( SelectOrderBy e )
        {
            return VisitStandard( e );
        }

        public virtual SqlNode Visit( SelectOrderByColumnList e )
        {
            return VisitStandard( e );
        }

        public virtual SqlNode Visit( SelectOrderByColumn e )
        {
            return VisitStandard( e );
        }

        public virtual SqlNode Visit( SelectOrderByOffset e )
        {
            return VisitStandard( e );
        }

        public virtual SqlNode Visit( SelectFor e )
        {
            return VisitStandard( e );
        }

        public virtual SqlNode Visit( SelectOption e )
        {
            return VisitStandard( e );
        }

        #endregion

        public virtual SqlNode Visit( SqlExprStFunctionInlineTable e )
        {
            return VisitStandard( e );
        }
    }
}
