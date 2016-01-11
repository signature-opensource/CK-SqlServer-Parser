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

        protected virtual ISqlNode VisitStandard( ISqlNode e )
        {
            List<ISqlNode> modified = VisitItems( e.ChildrenNodes );
            if( modified == null ) return e;
            return ((SqlNode)e).InternalDoClone( e.LeadingTrivias, modified, e.TrailingTrivias );
        }

        protected virtual ISqlNode VisitTokenStandard( SqlToken e )
        {
            return e;
        }

        protected virtual ISqlNode VisitTypeDeclStandard( ISqlUnifiedTypeDecl e )
        {
            return e;
        }

        public virtual ISqlNode Visit( SqlNodeExternal e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlOpenXml e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlOpenJSON e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlCallParameter e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlCallParameterList e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlUnnamedStatement e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlNodeList e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlEnclosableCommaList e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlEnclosedCommaList e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlPar e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlTokenLiteralInteger e )
        {
            return VisitTokenStandard( e );
        }

        public virtual ISqlNode Visit( SqlTokenError e )
        {
            return e;
        }

        public virtual ISqlNode Visit( SqlTokenLiteralBinary e )
        {
            return VisitTokenStandard( e );
        }

        public virtual ISqlNode Visit( SqlTokenLiteralFloat e )
        {
            return VisitTokenStandard( e );
        }

        public virtual ISqlNode Visit( SqlTokenLiteralDecimal e )
        {
            return VisitTokenStandard( e );
        }

        public virtual ISqlNode Visit( SqlTokenLiteralMoney e )
        {
            return VisitTokenStandard( e );
        }

        public virtual ISqlNode Visit( SqlTokenLiteralString e )
        {
            return VisitTokenStandard( e );
        }

        public virtual ISqlNode Visit( SqlTokenTerminal e )
        {
            return VisitTokenStandard( e );
        }

        public virtual ISqlNode Visit( SqlTokenIdentifier e )
        {
            return VisitTokenStandard( e );
        }

        public virtual ISqlNode Visit( SqlBasicValue e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlKoCall e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlOverClause e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlCollate e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlIf e )
        {
            return VisitStandard( e );
        }

        public ISqlNode Visit( SqlCursorDefinition e )
        {
            return VisitStandard( e );
        }

        public ISqlNode Visit( SqlIdentifierCommaList e )
        {
            return VisitStandard( e );
        }

        public ISqlNode Visit( SqlCursorDefinition92 e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlDeclareCursor e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlBeginTransaction e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlStatementList e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlBeginEndBlock e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlTryCatch e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlStatement e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlEmptyStatement e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlReturnStatement e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlExecuteStatement e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlExecuteStringStatement e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlSetVariable e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlInsertStatement e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlUpdateStatement e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlDeleteStatement e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlMergeStatement e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlTableValues e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlMultiCommaList e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlWithParOptions e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlOptionParOptions e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlWithOptions e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( MIUDHeader e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( IUDTarget e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlOutputClause e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlSetOption e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlGoto e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlLabelDefinition e )
        {
            return VisitStandard( e );
        }


        public virtual ISqlNode Visit( SqlStoredProcedure e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlFunctionScalar e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlView e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlFunctionInlineTable e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlEnclosedIdentifierCommaList e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlExecuteAs e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlDeclareVariable e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlVariableDeclarationList e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlVariableDeclaration e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlCast e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlCaseWhenList e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlMultiIdentifier e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlOpenDataSource e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlUnaryOperator e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlTypeDeclDecimal e )
        {
            return VisitTypeDeclStandard( e );
        }

        public virtual ISqlNode Visit( SqlTypeDeclDateAndTime e )
        {
            return VisitTypeDeclStandard( e );
        }

        public virtual ISqlNode Visit( SqlTypeDeclSimple e )
        {
            return VisitTypeDeclStandard( e );
        }

        public virtual ISqlNode Visit( SqlTypeDeclWithSize e )
        {
            return VisitTypeDeclStandard( e );
        }

        public virtual ISqlNode Visit( SqlTypeDeclUserDefined e )
        {
            return VisitTypeDeclStandard( e );
        }

        public virtual ISqlNode Visit( SqlTypeDeclTable e )
        {
            return VisitTypeDeclStandard( e );
        }

        public virtual ISqlNode Visit( SqlTypeDeclCursorParameter  e )
        {
            return VisitTypeDeclStandard( e );
        }

        public virtual ISqlNode Visit( SqlTypedIdentifier e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlParameter e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlParameterList e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlAssign e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlCommaList e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlBinaryOperator e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlIsNull e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlLike e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlBetween e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlInValues e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlCase e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlCaseWhenSelector e )
        {
            return VisitStandard( e );
        }


        #region Select

        public virtual ISqlNode Visit( SqlSelectStatement e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SelectSpec e )
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

        public virtual ISqlNode Visit( SelectGroupBy e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SelectCombine e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SelectDecorator e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SelectFor e )
        {
            return VisitStandard( e );
        }


        public virtual ISqlNode Visit( SqlOrderByList e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlOrderByItem e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SelectOrderBy e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlNextValueFor e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlCTEStatement e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlCTENameList e )
        {
            return VisitStandard( e );
        }

        public virtual ISqlNode Visit( SqlCTEName e )
        {
            return VisitStandard( e );
        }

        #endregion

    }
}
