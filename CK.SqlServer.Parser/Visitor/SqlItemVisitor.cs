using System;
using System.Collections.Generic;
using System.Linq;
using CK.Core;

namespace CK.SqlServer.Parser
{
    public class SqlItemVisitor
    {
        public virtual ISqlNode VisitItem( ISqlNode e )
        {
            return ((SqlNode)e).Accept( this );
        }

        protected List<ISqlNode> VisitItems( IEnumerable<ISqlNode> nodes, ISqlNode prefixToKeep = null, ISqlNode suffixToKeep = null )
        {
            List<ISqlNode> modified = null;
            int i = 0;
            foreach( var a in nodes )
            {
                var ve = VisitItem( a );
                if( !ReferenceEquals( a, ve ) )
                {
                    if( modified == null )
                    {
                        modified = new List<ISqlNode>( i+1 );
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
            List<ISqlNode> modified = VisitItems( e.ItemsWithoutParenthesis, e.Opener, e.Closer );
            if( modified == null ) return e;
            return (SqlExpr)e.InternalClone( e.LeadingTrivias, modified, e.TrailingTrivias );
        }

        protected ISqlNode VisitStandard( ISqlNode e )
        {
            List<ISqlNode> modified = VisitItems( e.ChildrenNodes );
            if( modified == null ) return e;
            return ((SqlNode)e).InternalClone( e.LeadingTrivias, modified, e.TrailingTrivias );
        }

        public virtual ISqlNode Visit( SqlNodeExternal e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit<T>( SqlTokenList<T> e ) where T : SqlToken
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlPar e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlTokenLiteralInteger e )
        {
            return e;
        }

        public virtual ISqlNode Visit( SqlTokenError e )
        {
            return e;
        }

        public virtual ISqlNode Visit( SqlTokenLiteralBinary e )
        {
            return e;
        }

        public virtual ISqlNode Visit( SqlTokenLiteralFloat e )
        {
            return e;
        }

        public virtual ISqlNode Visit( SqlTokenLiteralDecimal e )
        {
            return e;
        }

        public virtual ISqlNode Visit( SqlTokenLiteralMoney e )
        {
            return e;
        }

        public virtual ISqlNode Visit( SqlTokenLiteralString e )
        {
            return e;
        }

        public virtual ISqlNode Visit( SqlTokenTerminal e )
        {
            return e;
        }

        public virtual ISqlNode Visit( SqlTokenIdentifier e )
        {
            return e;
        }


        public virtual ISqlNode Visit( SqlExprUnmodeledItems e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlExprRawItemList e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlExprKoCall e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlNoExprOverClause e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlExprCollate e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlExprStIf e )
        {
            return VisitStandard( e );
        }

        public ISqlNode Visit( SqlExprCursor e )
        {
            return VisitStandard( e );
        }

        public ISqlNode Visit( SqlNoExprIdentifierList e )
        {
            return VisitStandard( e );
        }

        public ISqlNode Visit( SqlExprCursorSql92 e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlExprStDeclareCursor e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlExprStBeginTran e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlExprStatementList e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlExprStBlock e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlExprStTryCatch e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlExprStUnmodeled e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlExprStStoredProc e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlExprStFunctionScalar e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlExprStReturn e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlExprStSetVar e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlExprStSetOpt e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlExprStGoto e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlExprStMonoStatement e )
        {
            return e;
        }

        public virtual ISqlNode Visit( SqlExprStLabelDef e )
        {
            return e;
        }

        public virtual ISqlNode Visit( SqlExprStEmpty e )
        {
            return e;
        }

        public virtual ISqlNode Visit( SqlExprStView e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlExprColumnList e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlNoExprExecuteAs e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlExprStDeclare e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlExprDeclareList e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlExprDeclare e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlExprCast e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlExprCommaList e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlExprIdentifier e )
        {
            return e;
        }

        public virtual ISqlNode Visit( SqlExprMultiIdentifier e )
        {
            return e;
        }

        public virtual ISqlNode Visit( SqlExprLiteral e )
        {
            return e;
        }

        public virtual ISqlNode Visit( SqlExprNull e )
        {
            return e;
        }

        public virtual ISqlNode Visit( SqlExprUnaryOperator e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlExprTypeDecl e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlExprTypeDeclDecimal e )
        {
            return e;
        }

        public virtual ISqlNode Visit( SqlExprTypeDeclDateAndTime e )
        {
            return e;
        }

        public virtual ISqlNode Visit( SqlExprTypeDeclSimple e )
        {
            return e;
        }

        public virtual ISqlNode Visit( SqlExprTypeDeclWithSize e )
        {
            return e;
        }

        public virtual ISqlNode Visit( SqlExprTypeDeclUserDefined e )
        {
            return e;
        }

        public virtual ISqlNode Visit( SqlExprTypedIdentifier e )
        {
            return e;
        }

        public virtual ISqlNode Visit( SqlExprParameter e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlExprParameterDefaultValue e )
        {
            return e;
        }

        public virtual ISqlNode Visit( SqlExprParameterList e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlExprAssign e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlExprBinaryOperator e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlExprIsNull e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlExprLike e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlExprBetween e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlExprIn e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlExprCase e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlExprCaseWhenSelector e )
        {
            return VisitStandard( e );
        }


        #region Select

        public virtual ISqlNode Visit( SelectQuery e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SelectSpecification e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SelectColumn e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SelectColumnList e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SelectHeader e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SelectInto e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SelectFrom e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SelectWhere e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SelectGroupBy e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SelectCombineOperator e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SelectOrderBy e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SelectOrderByColumnList e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SelectOrderByColumn e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SelectOrderByOffset e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SelectFor e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SelectOption e )
        {
            return VisitStandard( e );
        }

        #endregion

        public virtual ISqlNode Visit( SqlExprStFunctionInlineTable e )
        {
            return VisitStandard( e );
        }
    }
}
